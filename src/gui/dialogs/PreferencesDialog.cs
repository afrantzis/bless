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
using Gtk;
using Glade;
using Bless.Tools;
using Bless.Util;
using Mono.Unix;

namespace Bless.Gui.Dialogs {

///<summary>
/// A dialog that lets user choose preferences for Bless
///</summary>
public class PreferencesDialog : Dialog
{
	Preferences prefs;
	
	[Glade.Widget] Notebook PreferencesNotebook;
	[Glade.Widget] Entry LayoutFileEntry;
	[Glade.Widget] CheckButton UseCurrentLayoutCheckButton;
	[Glade.Widget] RadioButton UndoLimitedRadioButton;
	[Glade.Widget] RadioButton UndoUnlimitedRadioButton;
	[Glade.Widget] SpinButton UndoActionsSpinButton;
	[Glade.Widget] ComboBox DefaultEditModeComboBox;
	[Glade.Widget] ComboBox DefaultNumberBaseComboBox;
	[Glade.Widget] CheckButton HighlightPatternMatchCheckButton;
	
	[Glade.Widget] CheckButton LoadPreviousSessionCheckButton;
	[Glade.Widget] CheckButton AskBeforeLoadingSessionCheckButton;
	[Glade.Widget] CheckButton RememberCursorPositionCheckButton;
	[Glade.Widget] CheckButton RememberWindowGeometryCheckButton;
	
	enum EditModeEnum { Insert, Overwrite }
	enum NumberBaseEnum { Hexadecimal, Decimal, Octal }
	
	public PreferencesDialog(Preferences p, Window parent)
	: base (Catalog.GetString("Bless Preferences"), parent, DialogFlags.DestroyWithParent)
	{
		Glade.XML gxml = new Glade.XML (FileResourcePath.GetSystemPath("..","data","bless.glade"), "PreferencesNotebook", "bless");
		gxml.Autoconnect (this);
		
		prefs=p;
		LoadPreferences();
		
		// connect handlers
		LayoutFileEntry.Changed+=OnPreferencesChanged;
		UseCurrentLayoutCheckButton.Toggled+=OnPreferencesChanged;
		UndoLimitedRadioButton.Toggled+=OnPreferencesChanged;
		UndoUnlimitedRadioButton.Toggled+=OnPreferencesChanged;
		UndoActionsSpinButton.ValueChanged+=OnPreferencesChanged;
		DefaultEditModeComboBox.Changed+=OnPreferencesChanged;
		DefaultNumberBaseComboBox.Changed+=OnPreferencesChanged;
		HighlightPatternMatchCheckButton.Toggled+=OnPreferencesChanged;
		
		LoadPreviousSessionCheckButton.Toggled+=OnPreferencesChanged;
		AskBeforeLoadingSessionCheckButton.Toggled+=OnPreferencesChanged;
		RememberCursorPositionCheckButton.Toggled+=OnPreferencesChanged;
		RememberWindowGeometryCheckButton.Toggled+=OnPreferencesChanged;
		
		this.Modal=false;
		this.TransientFor=parent;
		this.BorderWidth=6;
		this.AddButton("Close", ResponseType.Close);
		this.Response+=new ResponseHandler(OnDialogResponse);
		this.VBox.Add(PreferencesNotebook);
	}

	///<summary>
	/// Load the preferences to the gui
	///</summary>
	void LoadPreferences()
	{
		string val;
		
		//
		//
		val=prefs["Default.Layout.File"];
		LayoutFileEntry.Text=val;
		
		LoadCheckButtonPreference(
			"Default.Layout.UseCurrent",
			UseCurrentLayoutCheckButton,
			false);
		
		//
		//
		val=prefs["Undo.Limited"];
		
		try {
			bool limited=Convert.ToBoolean(val);
			UndoLimitedRadioButton.Active=limited;
			UndoUnlimitedRadioButton.Active=!limited;	
		}
		catch(FormatException e) {
			UndoLimitedRadioButton.Active=true;	
		}
		
		//
		val=prefs["Undo.Actions"];
		
		try {
			int actions=Convert.ToInt32(val);
			UndoActionsSpinButton.Value=actions;	
		}
		catch(FormatException e) {
			UndoActionsSpinButton.Value=100;	
		}
		
		//
		//
		val=prefs["Default.EditMode"];
		if (val!="Insert" && val!="Overwrite")
			val="Insert";
		
		{
			EditModeEnum index;
			if (val=="Insert")
				index=EditModeEnum.Insert;
			else
				index=EditModeEnum.Overwrite;
				 
			DefaultEditModeComboBox.Active=(int)index;
		}
		
		//
		//
		val=prefs["Default.NumberBase"];
		if (val!="Octal" && val!="Decimal" && val!="Hexadecimal")
			val="Hexadecimal";
		
		{
			NumberBaseEnum index;
			if (val=="Hexadecimal")
				index=NumberBaseEnum.Hexadecimal;
			else if (val=="Decimal")
				index=NumberBaseEnum.Decimal;
			else
				index=NumberBaseEnum.Octal;	
				 
			DefaultNumberBaseComboBox.Active=(int)index;
		}
		
		LoadCheckButtonPreference(
			"Highlight.PatternMatch",
			HighlightPatternMatchCheckButton,
			true);
		
		//
		// Session
		//
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
	
	void LoadCheckButtonPreference(string key, CheckButton cb, bool defaultValue)
	{
		string val=prefs[key];
		
		try {
			bool b=Convert.ToBoolean(val);
			cb.Active=b;
		}
		catch(FormatException e) {
			cb.Active=defaultValue;
		}
	
	}
	
	///<summary>
	/// Save the preferences from the gui
	///</summary>
	void UpdatePreferences()
	{
		// temporarily disable autosave
		// so that we don't save every time
		// a preference changes
		string autoSavePath=prefs.AutoSavePath;
		prefs.AutoSavePath=null;
		
		prefs["Default.Layout.File"]=LayoutFileEntry.Text;
		prefs["Default.Layout.UseCurrent"]=UseCurrentLayoutCheckButton.Active.ToString();
		prefs["Undo.Limited"]=UndoLimitedRadioButton.Active.ToString();
		prefs["Undo.Actions"]=UndoActionsSpinButton.ValueAsInt.ToString();
		prefs["Highlight.PatternMatch"]=HighlightPatternMatchCheckButton.Active.ToString();
		
		TreeIter iter;

		if (DefaultEditModeComboBox.GetActiveIter (out iter))
			prefs["Default.EditMode"]=(string) DefaultEditModeComboBox.Model.GetValue (iter, 0);
        
		if (DefaultNumberBaseComboBox.GetActiveIter (out iter))
			prefs["Default.NumberBase"]=(string) DefaultNumberBaseComboBox.Model.GetValue (iter, 0);
		
		prefs["Session.LoadPrevious"]=LoadPreviousSessionCheckButton.Active.ToString();
		prefs["Session.AskBeforeLoading"]=AskBeforeLoadingSessionCheckButton.Active.ToString();
		prefs["Session.RememberCursorPosition"]=RememberCursorPositionCheckButton.Active.ToString();
		
		// re-enable autosave
		// to save the preferences when setting the last preference
		prefs.AutoSavePath=autoSavePath;
		
		// set the last preference
		prefs["Session.RememberWindowGeometry"]=RememberWindowGeometryCheckButton.Active.ToString();
	}
		
	void OnDialogResponse(object o, Gtk.ResponseArgs args)
	{
		// update and save the preferences
		UpdatePreferences();
		
		this.Destroy();		
	}
	
	void OnSelectLayoutClicked(object o, EventArgs args)
	{
		LayoutSelectionDialog lsd=new LayoutSelectionDialog(null);
		Gtk.ResponseType response=(Gtk.ResponseType)lsd.Run();
		
		if (response==Gtk.ResponseType.Ok && lsd.SelectedLayout!=null) {
			LayoutFileEntry.Text=lsd.SelectedLayout;
		}
		
		lsd.Destroy();
	}
	
	void OnLoadPreviousSessionToggled(object o, EventArgs args)
	{
		if (LoadPreviousSessionCheckButton.Active) {
			AskBeforeLoadingSessionCheckButton.Sensitive=true;
			RememberCursorPositionCheckButton.Sensitive=true;
			RememberWindowGeometryCheckButton.Sensitive=true;
		}
		else {
			AskBeforeLoadingSessionCheckButton.Sensitive=false;
			RememberCursorPositionCheckButton.Sensitive=false;
			RememberWindowGeometryCheckButton.Sensitive=false;
		}
	}
	
	void OnPreferencesChanged(object o, EventArgs args)
	{
		// update preferences only when closing the dialog...
		// UpdatePreferences();
	}
	
}

} // end namespace
