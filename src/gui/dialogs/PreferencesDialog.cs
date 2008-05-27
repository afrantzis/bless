// created on 6/3/2005 at 12:21 PM
/*
 *   Copyright (c) 2005, Alexandros Frantzis (alf82 [at] freemail [dot] gr)
 *
 *   This file is part of Bless.
 *
 *   Bless is free software; you can redistribute it and/or modify
 *   it under the terms of the GNU General Public License as published by
 *   the Free Software Foundation; either version 2 of the License, or
 *   (at your option) any later version.
 *
 *   Bless is distributed in the hope that it will be useful,
 *   but WITHOUT ANY WARRANTY; without even the implied warranty of
 *   MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 *   GNU General Public License for more details.
 *
 *   You should have received a copy of the GNU General Public License
 *   along with Bless; if not, write to the Free Software
 *   Foundation, Inc., 59 Temple Place, Suite 330, Boston, MA  02111-1307  USA
 */
using System;
using System.IO;
using System.Collections.Generic;
using Gtk;
using Glade;
using Bless.Tools;
using Bless.Util;
using Mono.Unix;
using Bless.Plugins;

namespace Bless.Gui.Dialogs {

///<summary>
/// A dialog that lets user choose preferences for Bless
///</summary>
public class PreferencesDialog : Dialog
{
	Window mainWindow;
	GeneralPreferences generalPreferences;
	SessionPreferences sessionPreferences;
	TreeIter selectedIter;

	[Glade.Widget] Paned PreferencesPaned;
	[Glade.Widget] TreeView PreferencesTreeView;
	

	public PreferencesDialog(Window parent)
			: base (Catalog.GetString("Bless Preferences"), parent, DialogFlags.DestroyWithParent)
	{
		Glade.XML gxml = new Glade.XML (FileResourcePath.GetDataPath("bless.glade"), "PreferencesPaned", "bless");
		gxml.Autoconnect (this);
		
		mainWindow = parent;
		
		generalPreferences = new GeneralPreferences(mainWindow);
		sessionPreferences = new SessionPreferences(mainWindow);
		LoadPreferencesTreeView();

		this.Modal = false;
		this.TransientFor = parent;
		this.BorderWidth = 6;
		this.AddButton("Close", ResponseType.Close);
		this.Response += new ResponseHandler(OnDialogResponse);
		this.VBox.Add(PreferencesPaned);
		this.VBox.ShowAll();
	}
	
	void LoadPreferencesTreeView()
	{
		TreeStore store = new TreeStore(typeof(string), typeof(IPluginPreferences));
		
		store.AppendValues(Catalog.GetString("General"), generalPreferences);
		store.AppendValues(Catalog.GetString("Session"), sessionPreferences);
		
		TreeIter ti = store.AppendValues(Catalog.GetString("Plugins"), null);
		
		PluginManager pm = PluginManager.GetForType(typeof(GuiPlugin));

		// Get all plugins from all managers that have preferences
		foreach(KeyValuePair<Type, PluginManager> kvp in PluginManager.AllManagers)
			foreach(Plugin p in kvp.Value.Plugins) 
				if (p.PluginPreferences != null)
					store.AppendValues(ti, p.Name, p.PluginPreferences);
		
		PreferencesTreeView.Model = store;
		PreferencesTreeView.AppendColumn("", new CellRendererText (), "text", 0);
		PreferencesTreeView.HeadersVisible = false;
		PreferencesTreeView.Selection.Changed += OnPreferencesTreeViewSelectionChanged;
		PreferencesTreeView.Selection.SelectPath(new TreePath("0"));
	}
	
	void OnPreferencesTreeViewSelectionChanged (object o, EventArgs args)
	{
		TreeSelection sel = (TreeSelection)o;
		TreeModel tm;
		TreeIter ti;

		if (sel.GetSelected(out tm, out ti)) {
			IPluginPreferences ipp = (IPluginPreferences) tm.GetValue (ti, 1);

			// If user tried to select a header row, keep the previous selection
			if (ipp == null) {
				sel.SelectIter(selectedIter);
				return;
			}

			if (PreferencesPaned.Child2 != null)
				PreferencesPaned.Remove(PreferencesPaned.Child2);

			PreferencesPaned.Pack2(ipp.Widget, true, false);
			ipp.LoadPreferences();	

			selectedIter = ti;
		}
	}
	
	void OnDialogResponse(object o, Gtk.ResponseArgs args)
	{
		this.Destroy();
	}

}

class GeneralPreferences : IPluginPreferences
{
	[Glade.Widget] Gtk.VBox GeneralPreferencesVBox;

	[Glade.Widget] Entry LayoutFileEntry;
	[Glade.Widget] CheckButton UseCurrentLayoutCheckButton;
	[Glade.Widget] RadioButton UndoLimitedRadioButton;
	[Glade.Widget] RadioButton UndoUnlimitedRadioButton;
	[Glade.Widget] SpinButton UndoActionsSpinButton;
	[Glade.Widget] ComboBox DefaultEditModeComboBox;
	[Glade.Widget] Entry TempDirEntry;
	[Glade.Widget] Button SelectTempDirButton;
	[Glade.Widget] Button SelectLayoutButton;
		
	enum EditModeEnum { Insert, Overwrite }
	enum NumberBaseEnum { Hexadecimal, Decimal, Octal }

	Window mainWindow;
	Preferences prefs;

	public GeneralPreferences(Window mw)
	{
		mainWindow = mw;
		prefs = Preferences.Instance;
	} 
	
	public Widget Widget {
		get {
			if (GeneralPreferencesVBox == null)
				InitWidget();
			
			return GeneralPreferencesVBox;
		}
	}

	public void LoadPreferences()
	{
		if (GeneralPreferencesVBox == null)
			InitWidget();

		string val;

		//
		//
		val = prefs["Default.Layout.File"];
		LayoutFileEntry.Text = val;

		LoadCheckButtonPreference(
			"Default.Layout.UseCurrent",
			UseCurrentLayoutCheckButton,
			false);

		//
		//
		val = prefs["Undo.Limited"];

		try {
			bool limited = Convert.ToBoolean(val);
			UndoLimitedRadioButton.Active = limited;
			UndoUnlimitedRadioButton.Active = !limited;
		}
		catch (FormatException e) {
			System.Console.WriteLine(e.Message);
			UndoLimitedRadioButton.Active = true;
		}

		//
		val = prefs["Undo.Actions"];

		try {
			int actions = Convert.ToInt32(val);
			UndoActionsSpinButton.Value = actions;
		}
		catch (FormatException e) {
			System.Console.WriteLine(e.Message);
			UndoActionsSpinButton.Value = 100;
		}

		//
		//
		val = prefs["Default.EditMode"];
		if (val != "Insert" && val != "Overwrite")
			val = "Insert";

		{
			EditModeEnum index;
			if (val == "Insert")
				index = EditModeEnum.Insert;
			else
				index = EditModeEnum.Overwrite;

			DefaultEditModeComboBox.Active = (int)index;
		}

		//
		//
		if (prefs["ByteBuffer.TempDir"] != System.IO.Path.GetTempPath())
			TempDirEntry.Text = prefs["ByteBuffer.TempDir"];
		else
			TempDirEntry.Text = "";

	}
	
	public void SavePreferences()
	{
		// All preferences are applied instantly...
		// No need to save them here
	}

	void InitWidget()
	{
		Glade.XML gxml = new Glade.XML (FileResourcePath.GetDataPath("bless.glade"), "GeneralPreferencesVBox", "bless");
		gxml.Autoconnect (this);

		SelectTempDirButton.Clicked += OnSelectTempDirButtonClicked;
		SelectLayoutButton.Clicked += OnSelectLayoutClicked;

		LayoutFileEntry.Changed += OnLayoutFileChanged;
		UseCurrentLayoutCheckButton.Toggled += OnUseCurrentLayoutToggled;
		UndoLimitedRadioButton.Toggled += OnUndoLimitedToggled;
		UndoActionsSpinButton.ValueChanged += OnUndoActionsValueChanged;
		DefaultEditModeComboBox.Changed += OnDefaultEditModeChanged;
	}
	
	void LoadCheckButtonPreference(string key, CheckButton cb, bool defaultValue)
	{
		string val = prefs[key];

		try {
			bool b = Convert.ToBoolean(val);
			cb.Active = b;
		}
		catch (FormatException e) {
			System.Console.WriteLine(e.Message);
			cb.Active = defaultValue;
		}

	}

	void OnSelectLayoutClicked(object o, EventArgs args)
	{
		LayoutSelectionDialog lsd = new LayoutSelectionDialog(null);
		Gtk.ResponseType response = (Gtk.ResponseType)lsd.Run();

		if (response == Gtk.ResponseType.Ok && lsd.SelectedLayout != null) {
			LayoutFileEntry.Text = lsd.SelectedLayout;
		}

		lsd.Destroy();
	}


	private void OnSelectTempDirButtonClicked(object o, EventArgs args)
	{
		FileChooserDialog fcd = new FileChooserDialog(Catalog.GetString("Select Directory"), mainWindow, FileChooserAction.CreateFolder,  Catalog.GetString("Cancel"), ResponseType.Cancel,
								Catalog.GetString("Select"), ResponseType.Accept);
		if ((ResponseType)fcd.Run() == ResponseType.Accept)
			TempDirEntry.Text = fcd.Filename;
		fcd.Destroy();
	}

	private void OnLayoutFileChanged(object o, EventArgs args)
	{
		prefs["Default.Layout.File"] = LayoutFileEntry.Text;
	}

	private void OnUseCurrentLayoutToggled(object o, EventArgs args)
	{
		prefs["Default.Layout.UseCurrent"] = UseCurrentLayoutCheckButton.Active.ToString();
	}

	private void OnUndoLimitedToggled(object o, EventArgs args)
	{
		prefs["Undo.Limited"] = UndoLimitedRadioButton.Active.ToString();
	}

	private void OnUndoActionsValueChanged(object o, EventArgs args)
	{
		prefs["Undo.Actions"] = UndoActionsSpinButton.ValueAsInt.ToString();
	}

	private void OnDefaultEditModeChanged(object o, EventArgs args)
	{
		TreeIter iter;

		if (DefaultEditModeComboBox.GetActiveIter (out iter))
			prefs["Default.EditMode"] = (string) DefaultEditModeComboBox.Model.GetValue (iter, 0);
	}

}

class SessionPreferences : IPluginPreferences
{
	Preferences prefs;

	[Glade.Widget] Gtk.VBox SessionPreferencesVBox;
	
	[Glade.Widget] CheckButton LoadPreviousSessionCheckButton;
	[Glade.Widget] CheckButton AskBeforeLoadingSessionCheckButton;
	[Glade.Widget] CheckButton RememberCursorPositionCheckButton;
	[Glade.Widget] CheckButton RememberWindowGeometryCheckButton;
	
	public SessionPreferences(Window mw)
	{
		prefs = Preferences.Instance;
	} 
	
	public Widget Widget {
		get {
			if (SessionPreferencesVBox == null)
				InitWidget();
			
			return SessionPreferencesVBox;
		}
	}
	

	public void LoadPreferences()
	{
		if (SessionPreferencesVBox == null)
			InitWidget();

		LoadCheckButtonPreference(
			"Session.LoadPrevious",
			LoadPreviousSessionCheckButton,
			true);

		LoadCheckButtonPreference(
			"Session.AskBeforeLoading",
			AskBeforeLoadingSessionCheckButton,
			false);

		LoadCheckButtonPreference(
			"Session.RememberCursorPosition",
			RememberCursorPositionCheckButton,
			true);

		LoadCheckButtonPreference(
			"Session.RememberWindowGeometry",
			RememberWindowGeometryCheckButton,
			true);
	}
	
	public void SavePreferences()
	{
		// All preferences are applied instantly...
		// No need to save them here
	}

	void LoadCheckButtonPreference(string key, CheckButton cb, bool defaultValue)
	{
		string val = prefs[key];

		try {
			bool b = Convert.ToBoolean(val);
			cb.Active = b;
		}
		catch (FormatException e) {
			System.Console.WriteLine(e.Message);
			cb.Active = defaultValue;
		}

	}

	void InitWidget()
	{
		Glade.XML gxml = new Glade.XML (FileResourcePath.GetDataPath("bless.glade"), "SessionPreferencesVBox", "bless");
		gxml.Autoconnect (this);

		LoadPreviousSessionCheckButton.Toggled += OnLoadPreviousSessionToggled;
		AskBeforeLoadingSessionCheckButton.Toggled += AskBeforeLoadingSessionToggled;
		RememberCursorPositionCheckButton.Toggled += RememberCursorPositionToggled;
		RememberWindowGeometryCheckButton.Toggled += RememberWindowGeometryToggled;
	}

	void OnLoadPreviousSessionToggled(object o, EventArgs args)
	{
		if (LoadPreviousSessionCheckButton.Active) {
			AskBeforeLoadingSessionCheckButton.Sensitive = true;
			RememberCursorPositionCheckButton.Sensitive = true;
			RememberWindowGeometryCheckButton.Sensitive = true;
		}
		else {
			AskBeforeLoadingSessionCheckButton.Sensitive = false;
			RememberCursorPositionCheckButton.Sensitive = false;
			RememberWindowGeometryCheckButton.Sensitive = false;
		}


		prefs["Session.LoadPrevious"] = LoadPreviousSessionCheckButton.Active.ToString();
	} 

	void AskBeforeLoadingSessionToggled(object o, EventArgs args)
	{
		prefs["Session.AskBeforeLoading"] = AskBeforeLoadingSessionCheckButton.Active.ToString();
	}

	void RememberCursorPositionToggled(object o, EventArgs args)
	{
		prefs["Session.RememberCursorPosition"] = RememberCursorPositionCheckButton.Active.ToString();
	}

	void RememberWindowGeometryToggled(object o, EventArgs args)
	{
		prefs["Session.RememberWindowGeometry"] = RememberWindowGeometryCheckButton.Active.ToString();
	}
}

} // end namespace
