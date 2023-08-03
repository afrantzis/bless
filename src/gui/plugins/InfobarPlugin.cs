// created on 6/10/2005 at 3:15 PM
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
using Gtk;
using System;
using Pango;
using Bless.Util;
using Bless.Gui;
using Bless.Plugins;
using Bless.Buffers;
using Bless.Tools;
using Mono.Unix;

namespace Bless.Gui.Plugins {

public class InfobarPlugin : GuiPlugin
{
	DataBook dataBook;
	Infobar widget;
	Window mainWindow;
	UIManager uiManager;
	CheckMenuItem viewStatusbarShowMenuItem;
	CheckMenuItem viewStatusbarOffsetMenuItem;
	CheckMenuItem viewStatusbarSelectionMenuItem;
	CheckMenuItem viewStatusbarOverwriteMenuItem;

	const string uiXml =
		"<menubar>" +
		"	<menu action=\"View\">" +
		"		<menu action=\"Statusbar\" >" +
		"			<menuitem name=\"Show\" action=\"StatusbarShowAction\" />" +
		"			<separator/>" +
		"			<menuitem name=\"Offset\" action=\"StatusbarOffsetAction\" />" +
		"			<menuitem name=\"Selection\" action=\"StatusbarSelectionAction\" />" +
		"			<menuitem name=\"Overwrite\" action=\"StatusbarOverwriteAction\" />" +
		"		</menu>" +
		"	</menu>" +
		"</menubar>";

	public InfobarPlugin(Window mw, UIManager uim)
	{
		mainWindow = mw;
		uiManager = uim;

		name = "Infobar";
		author = "Alexandros Frantzis";
		description = "Advanced statusbar";
	}

	public override bool Load()
	{
		dataBook = (DataBook)GetDataBook(mainWindow);

		widget = new Infobar(dataBook);
		widget.Visible = true;

		Services.UI.Info = widget;

		((VBox)mainWindow.Child).PackEnd(widget, false, false, 0);

		AddMenuItems(uiManager);

		dataBook.PageAdded += new DataView.DataViewEventHandler(OnDataViewAdded);
		dataBook.Removed += new RemovedHandler(OnDataViewRemoved);
		dataBook.SwitchPage += new SwitchPageHandler(OnSwitchPage);

		PreferencesChangedHandler handler = new PreferencesChangedHandler(OnPreferencesChanged);
		Preferences.Proxy.Subscribe("View.Statusbar.Show", "ib1", handler);
		Preferences.Proxy.Subscribe("View.Statusbar.Selection", "ib1", handler);
		Preferences.Proxy.Subscribe("View.Statusbar.Overwrite", "ib1", handler);
		Preferences.Proxy.Subscribe("View.Statusbar.Offset", "ib1", handler);

		loaded = true;
		return true;
	}

	private void AddMenuItems(UIManager uim)
	{
		ToggleActionEntry[] toggleActionEntries = new ToggleActionEntry[] {
					new ToggleActionEntry ("StatusbarShowAction", null, Catalog.GetString("Show"), null, null,
										   new EventHandler(OnStatusbarShow), false),
					new ToggleActionEntry ("StatusbarOffsetAction", null, Catalog.GetString("Offset"), null, null,
										   new EventHandler(OnStatusbarOffset), false),
					new ToggleActionEntry ("StatusbarSelectionAction", null, Catalog.GetString("Selection"), null, null,
										   new EventHandler(OnStatusbarSelection), false),
					new ToggleActionEntry ("StatusbarOverwriteAction", null, Catalog.GetString("Overwrite"), null, null,
										   new EventHandler(OnStatusbarOverwrite), false)
				};

		ActionEntry[] actionEntries = new ActionEntry[] {
										  new ActionEntry ("Statusbar", null, Catalog.GetString("Statusbar"), null, null, null)
									  };

		ActionGroup group = new ActionGroup ("StatusbarActions");
		group.Add (toggleActionEntries);
		group.Add (actionEntries);

		uim.InsertActionGroup(group, 0);
		uim.AddUiFromString(uiXml);

		viewStatusbarShowMenuItem = (CheckMenuItem)uim.GetWidget("/menubar/View/Statusbar/Show");
		viewStatusbarOffsetMenuItem = (CheckMenuItem)uim.GetWidget("/menubar/View/Statusbar/Offset");
		viewStatusbarSelectionMenuItem = (CheckMenuItem)uim.GetWidget("/menubar/View/Statusbar/Selection");
		viewStatusbarOverwriteMenuItem = (CheckMenuItem)uim.GetWidget("/menubar/View/Statusbar/Overwrite");

		uim.EnsureUpdate();
	}

	void OnDataViewAdded(DataView dv)
	{
		dv.Buffer.Changed += new ByteBuffer.ChangedHandler(OnBufferContentsChanged);
		dv.BufferChanged += new DataView.DataViewEventHandler(OnBufferChanged);
		dv.CursorChanged += new DataView.DataViewEventHandler(OnCursorChanged);
		dv.SelectionChanged += new DataView.DataViewEventHandler(OnSelectionChanged);
	}

	void OnDataViewRemoved(object o, RemovedArgs args)
	{
		DataView dv = ((DataViewDisplay)args.Widget).View;
		dv.Buffer.Changed -= new ByteBuffer.ChangedHandler(OnBufferContentsChanged);
		dv.BufferChanged -= new DataView.DataViewEventHandler(OnBufferChanged);
		dv.CursorChanged -= new DataView.DataViewEventHandler(OnCursorChanged);
		dv.SelectionChanged -= new DataView.DataViewEventHandler(OnSelectionChanged);
	}

	void OnBufferChanged(DataView dv)
	{
		UpdateInfobar(dv);
	}

	void OnBufferContentsChanged(ByteBuffer bb)
	{
		Gtk.Application.Invoke(delegate {
			DataView dv = null;

			// find DataView that owns bb
			foreach (DataViewDisplay dvtemp in dataBook.Children) {
				if (dvtemp.View.Buffer == bb) {
					dv = dvtemp.View;
					break;
				}
			}

			UpdateInfobar(dv);
		});
	}

	void OnSwitchPage(object o, SwitchPageArgs args)
	{
		DataView dv = ((DataViewDisplay)dataBook.GetNthPage((int)args.PageNum)).View;

		UpdateInfobar(dv);
	}

	void OnCursorChanged(DataView dv)
	{
		UpdateInfobar(dv);
	}

	void OnSelectionChanged(DataView dv)
	{
		UpdateInfobar(dv);
	}

	public void OnStatusbarShow(object o, EventArgs args)
	{
		Preferences.Proxy.Change("View.Statusbar.Show", viewStatusbarShowMenuItem.Active.ToString(), "ib1");
	}

	///<summary>Handle the View->Statusbar->Offset command</summary>
	public void OnStatusbarOffset(object o, EventArgs args)
	{
		Preferences.Proxy.Change("View.Statusbar.Offset", viewStatusbarOffsetMenuItem.Active.ToString(), "ib1");
	}

	///<summary>Handle the View->Statusbar->Selection command</summary>
	public void OnStatusbarSelection(object o, EventArgs args)
	{
		Preferences.Proxy.Change("View.Statusbar.Selection", viewStatusbarSelectionMenuItem.Active.ToString(), "ib1");
	}

	///<summary>Handle the View->Statusbar->Overwrite command</summary>
	public void OnStatusbarOverwrite(object o, EventArgs args)
	{
		Preferences.Proxy.Change("View.Statusbar.Overwrite", viewStatusbarOverwriteMenuItem.Active.ToString(), "ib1");
	}


	void UpdateInfobar(DataView dv)
	{
		if (dataBook.NPages == 0)
			return;

		DataView curdv = ((DataViewDisplay)dataBook.CurrentPageWidget).View;
		if (curdv != dv)
			return;

		widget.Update(dv);
	}

	void OnPreferencesChanged(Preferences prefs)
	{
		if (prefs["View.Statusbar.Show"] == "True") {
			viewStatusbarShowMenuItem.Active = true;
			viewStatusbarOffsetMenuItem.Sensitive = true;
			viewStatusbarSelectionMenuItem.Sensitive = true;
			viewStatusbarOverwriteMenuItem.Sensitive = true;
		}
		else {
			viewStatusbarShowMenuItem.Active = false;
			viewStatusbarOffsetMenuItem.Sensitive = false;
			viewStatusbarSelectionMenuItem.Sensitive = false;
			viewStatusbarOverwriteMenuItem.Sensitive = false;
		}

		if (Preferences.Instance["View.Statusbar.Offset"] == "True")
			viewStatusbarOffsetMenuItem.Active = true;
		else
			viewStatusbarOffsetMenuItem.Active = false;

		if (Preferences.Instance["View.Statusbar.Selection"] == "True")
			viewStatusbarSelectionMenuItem.Active = true;
		else
			viewStatusbarSelectionMenuItem.Active = false;

		if (Preferences.Instance["View.Statusbar.Overwrite"] == "True")
			viewStatusbarOverwriteMenuItem.Active = true;
		else
			viewStatusbarOverwriteMenuItem.Active = false;
	}
}

///<summary>
/// An advanced statusbar for Bless
///</summary>
public class Infobar : Gtk.HBox, IInfoDisplay
{
	Label MessageLabel;
	Label OffsetLabel;
	Label SelectionLabel;
	Label OverwriteLabel;

	DataBook dataBook;

	int numberBase;

	///<summary>
	///Get or set the visibility of the Offset statusbar
	///</summary>
	public bool OffsetVisible {
		get { return OffsetLabel.Visible; }
		set { OffsetLabel.Visible = value; }
	}

	///<summary>
	///Get or set the visibility of the Selection statusbar
	///</summary>
	public bool SelectionVisible {
		get { return SelectionLabel.Visible; }
		set { SelectionLabel.Visible = value; }
	}

	///<summary>
	///Get or set the visibility of the Overwrite statusbar
	///</summary>
	public bool OverwriteVisible {
		get { return OverwriteLabel.Visible; }
		set { OverwriteLabel.Visible = value; }
	}

	public int NumberBase {
		set {
			numberBase = value;

			if (dataBook.NPages > 0) {
				DataView curdv = ((DataViewDisplay)dataBook.CurrentPageWidget).View;

				UpdateOffset(curdv);
				UpdateSelection(curdv);
			}
		}

		get {
			return numberBase;
		}
	}

	public Infobar(DataBook db)
	{
		dataBook = db;

		MessageLabel = new Label();
		MessageLabel.Ellipsize = Pango.EllipsizeMode.End;
		MessageLabel.SetAlignment(0.0f, 0.5f);
		OffsetLabel = new Label();
		SelectionLabel = new Label();
		OverwriteLabel = new Label();

		EventBox OffsetEB = new EventBox();
		OffsetEB.Add(OffsetLabel);
		OffsetEB.ButtonPressEvent += ChangeNumberBase;

		EventBox SelectionEB = new EventBox();
		SelectionEB.Add(SelectionLabel);
		SelectionEB.ButtonPressEvent += ChangeNumberBase;

		EventBox OverwriteEB = new EventBox();
		OverwriteEB.Add(OverwriteLabel);
		OverwriteEB.ButtonPressEvent += OnOverwriteLabelPressed;

		this.PackStart(MessageLabel, true, true, 20);
		this.PackStart(OffsetEB, false, false, 20);
		this.PackStart(SelectionEB, false, false, 20);
		this.PackStart(OverwriteEB, false, false, 20);

		this.NumberBase = 16;

		PreferencesChangedHandler handler = new PreferencesChangedHandler(OnPreferencesChanged);
		Preferences.Proxy.Subscribe("View.Statusbar.Show", "ib2", handler);
		Preferences.Proxy.Subscribe("View.Statusbar.Selection", "ib2", handler);
		Preferences.Proxy.Subscribe("View.Statusbar.Overwrite", "ib2", handler);
		Preferences.Proxy.Subscribe("View.Statusbar.Offset", "ib2", handler);

		this.ShowAll();
	}

	///<summary>Displays a message in the infobar (for 5 sec)</summary>
	public void DisplayMessage(string message)
	{
		MessageLabel.Text = message;
		GLib.Timeout.Add(5000, ClearStatusMessage);
	}

	///<summary>Clears the status bar message (callback)</summary>
	bool ClearStatusMessage()
	{
		MessageLabel.Text = "";
		return false;
	}

	///<summary>
	/// Updates all the information in the infobar with data from the specified DataView,
	/// if the specified DataView is active
	///</summary>
	public void Update(DataView dv)
	{
		UpdateOffset(dv);
		UpdateSelection(dv);
		UpdateOverwrite(dv);
	}

	///<summary>
	/// Updates the cursor status bar with data from the specified DataView,
	/// if the specified DataView is active
	///</summary>
	public void UpdateOffset(DataView dv)
	{
		if (dataBook.NPages == 0)
			return;

		DataView curdv = ((DataViewDisplay)dataBook.CurrentPageWidget).View;
		if (curdv != dv)
			return;

		long coffset = dv.CursorOffset;

		string coffsetString = BaseConverter.ConvertToString(coffset, numberBase, true, true, 1);
		string sizeString = BaseConverter.ConvertToString(dv.Buffer.Size - 1, numberBase, true, true, 1);

		string str = string.Format(Catalog.GetString("Offset: {0} / {1}"), coffsetString, sizeString);
		OffsetLabel.Text = str;
	}

	///<summary>
	/// Updates the selection status bar with data from the specified DataView,
	/// if the specified DataView is active
	///</summary>
	public void UpdateSelection(DataView dv)
	{
		if (dataBook.NPages == 0)
			return;

		DataView curdv = ((DataViewDisplay)dataBook.CurrentPageWidget).View;
		if (curdv != dv)
			return;

		Bless.Util.Range sel = dv.Selection;
		
		string str;

		if (sel.IsEmpty() == true)
			str = Catalog.GetString("Selection: None");
		else {
			string startString = BaseConverter.ConvertToString(sel.Start, numberBase, true, true, 1);
			string endString = BaseConverter.ConvertToString(sel.End, numberBase, true, true, 1);
			string sizeString = BaseConverter.ConvertToString(sel.Size, numberBase, true, true, 1);

			str = string.Format(Catalog.GetString("Selection: {0} to {1}") + " " + Catalog.GetPluralString("({2} byte)", "({2} bytes)", (int)sel.Size),
					startString, endString, sizeString);
		}

		SelectionLabel.Text = str;
	}

	///<summary>
	/// Updates the overwrite status bar with data from the specified DataView,
	/// if the specified DataView is active
	///</summary>
	public void UpdateOverwrite(DataView dv)
	{
		if (dataBook.NPages == 0)
			return;

		DataView curdv = ((DataViewDisplay)dataBook.CurrentPageWidget).View;
		if (curdv != dv)
			return;

		if (dv.Overwrite == true)
			OverwriteLabel.Text = Catalog.GetString("OVR");
		else
			OverwriteLabel.Text = Catalog.GetString("INS");
	}

	///<summary>
	/// Clears the information displayed in the infobar
	///</summary>
	public void ClearMessage()
	{
		OffsetLabel.Text = Catalog.GetString("Offset: -");
		SelectionLabel.Text = Catalog.GetString("Selection: None");
	}

	///<summary>Handle button press on Ins/Ovr statusbar</summary>
	void OnOverwriteLabelPressed (object o, ButtonPressEventArgs args)
	{
		if (dataBook.NPages > 0) {
			Gdk.EventButton e = args.Event;
			// ignore double and triple-clicks
			if (e.Type != Gdk.EventType.ButtonPress)
				return;

			// set edit mode
			DataView dv = ((DataViewDisplay)dataBook.CurrentPageWidget).View;
			dv.Overwrite = !dv.Overwrite;
		}
	}

	void ChangeNumberBase(object o, ButtonPressEventArgs args)
	{
		if (dataBook.NPages == 0)
			return;

		Gdk.EventButton e = args.Event;
		// ignore double and triple-clicks
		if (e.Type != Gdk.EventType.ButtonPress)
			return;

		// cycle 8, 10 and 16 number bases
		if (this.NumberBase == 8)
			this.NumberBase = 10;
		else if (this.NumberBase == 10)
			this.NumberBase = 16;
		else if (this.NumberBase == 16)
			this.NumberBase = 8;
	}

	void OnPreferencesChanged(Preferences prefs)
	{
		if (prefs["View.Statusbar.Show"] == "True")
			this.Visible = true;
		else
			this.Visible = false;

		if (Preferences.Instance["View.Statusbar.Offset"] == "True")
			this.OffsetVisible = true;
		else
			this.OffsetVisible = false;

		if (Preferences.Instance["View.Statusbar.Selection"] == "True")
			this.SelectionVisible = true;
		else
			this.SelectionVisible = false;

		if (Preferences.Instance["View.Statusbar.Overwrite"] == "True")
			this.OverwriteVisible = true;
		else
			this.OverwriteVisible = false;
	}
}

} // end namespace
