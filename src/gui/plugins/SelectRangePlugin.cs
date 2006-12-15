/*
 *   Copyright (c) 2006, Alexandros Frantzis (alf82 [at] freemail [dot] gr)
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
using Gtk;
using Glade;
using Bless.Buffers;
using Bless.Util;
using Bless.Gui.Dialogs;
using Bless.Gui;
using Bless.Plugins;

namespace Bless.Gui.Plugins {
	
public class SelectRangePlugin : GuiPlugin
{	
	SelectRangeWidget widget;
	
	const string uiXml =
	"<menubar>"+
	"	<menu action=\"Edit\">"+
	"		<placeholder name=\"Extra\">"+
	"			<menuitem name=\"SelectRange\" action=\"SelectRangeAction\" />"+
	"		</placeholder>"+
	"		<separator/>"+
	"	</menu>"+
	"</menubar>";
	
	Window mainWindow;
	UIManager uiManager;
	DataBook dataBook;
	
	public SelectRangePlugin(Window mw, UIManager uim)
	{
		mainWindow = mw;
		uiManager = uim;
		
		name = "SelectRange";
		author = "Alexandros Frantzis";
		description = "Adds a select range bar";
		loadAfter.Add("EditOperations");
	}
	
	public override bool Load()
	{
		dataBook = (DataBook)GetDataBook(mainWindow);
		widget=new SelectRangeWidget(dataBook);
		widget.Visible=false;
		
		WidgetGroup wgroup=(WidgetGroup)GetWidgetGroup(mainWindow, 0);
		wgroup.Add(widget);
		
		AddMenuItems(uiManager);
		
		loaded=true;
		return true;
	}
	
	private void AddMenuItems(UIManager uim)
	{
		ActionEntry[] actionEntries = new ActionEntry[] {
			new ActionEntry ("SelectRangeAction", Stock.JumpTo, "_Select Range", "<shift><control>R", null,
			                    new EventHandler(OnSelectRangeActivated)),
		};
		
		ActionGroup group = new ActionGroup ("SelectRangeActions");
		group.Add (actionEntries);
		
		uim.InsertActionGroup(group, 0);
		uim.AddUiFromString(uiXml);
		
		uim.EnsureUpdate();
		
	}
	
	///<summary>Handle the Edit->Select Range command</summary>
	public void OnSelectRangeActivated(object o, EventArgs args)
	{
		if (dataBook.NPages > 0) {
			DataView dv=((DataViewDisplay)dataBook.CurrentPageWidget).View;
			widget.LoadWithSelection(dv);
		}
		
		widget.Show();
	}
}
	
///<summary>
/// A widget for the select range operation
///</summary>
public class SelectRangeWidget : Gtk.HBox
{
	[Glade.Widget] Gtk.HBox SelectRangeHBox;
	[Glade.Widget] Gtk.Button SelectButton;
	[Glade.Widget] Gtk.Entry FromEntry;
	[Glade.Widget] Gtk.Entry ToEntry;
	[Glade.Widget] Gtk.Button CloseButton;
	
	DataBook dataBook;
	
	
	public SelectRangeWidget(DataBook db)
	{
		dataBook = db;
		
		Glade.XML gxml = new Glade.XML (FileResourcePath.GetSystemPath("..","data","bless.glade"), "SelectRangeHBox", null);
		gxml.Autoconnect (this);
		
		// set up entry completions
		FromEntry.Completion = new EntryCompletion();
		FromEntry.Completion.Model = new ListStore (typeof (string));
		FromEntry.Completion.TextColumn = 0;
		
		ToEntry.Completion = new EntryCompletion();
		ToEntry.Completion.Model = new ListStore (typeof (string));
		ToEntry.Completion.TextColumn = 0;
		
		// set button sensitivity 
		OnEntryChanged(null, null);
		
		this.Shown += OnWidgetShown;
		
		this.Add(SelectRangeHBox);
		this.ShowAll();
	}
	
	///<summary>Load the widget with data from the DataView's selection</summary>
	public void LoadWithSelection(DataView dv)
	{
		ByteBuffer bb = dv.Buffer;
		
		// load selection only if it isn't very large 
		if (dv.Selection.Size > 0) {
			// make sure selection is sorted
			Bless.Util.Range sel = new Bless.Util.Range(dv.Selection);
			sel.Sort();
			
			FromEntry.Text = sel.Start.ToString();
			ToEntry.Text = sel.End.ToString();
		}
		else {
			FromEntry.Text = dv.CursorOffset.ToString(); 
		}
		
		FromEntry.GrabFocus();
	}
	
	///<summary>
	/// Whether a widget in the SelectRangeWidget has the focus 
	///</summary>
	bool IsFocusInWidget()
	{
		foreach (Gtk.Widget child in SelectRangeHBox.Children) {
			Widget realChild = child;
			
			if (child.GetType() == typeof(Gtk.Alignment))
				realChild = (child as Gtk.Alignment).Child;
				
			if (realChild.HasFocus)
				return true;
		}
		
		return false;
	}
	
	void OnEntryChanged(object o, EventArgs args)
	{
		if (ToEntry.Text.Length > 0 && FromEntry.Text.Length > 0)
			SelectButton.Sensitive = true;
		else
			SelectButton.Sensitive = false;
	}
	
	void OnEntryActivated(object o, EventArgs args)
	{
		if (SelectButton.Sensitive == true)
			SelectButton.Click();
	}
	
	void OnSelectButtonClicked(object o, EventArgs args)
	{
		if (dataBook.NPages == 0)
			return;
			
		DataView dv = ((DataViewDisplay)dataBook.CurrentPageWidget).View;
		
		long fromOffset = -1;
		long toOffset = -1;
		int relative = 0;
		
		try {
			fromOffset = BaseConverter.Parse(FromEntry.Text);
		}
		catch(FormatException e) {
			ErrorAlert ea=new ErrorAlert("Error in From Offset Format", e.Message, null);
			ea.Run();
			ea.Destroy();
			return;
		}
		
		string toString = ToEntry.Text.Trim();
		
		if (toString.StartsWith("+"))
			relative = 1;
		else if (toString.StartsWith("-"))
			relative = -1;
			
		toString = toString.TrimStart(new char[]{'+','-'});
		
		try {
			toOffset = BaseConverter.Parse(toString);
		}
		catch(FormatException e) {
			ErrorAlert ea=new ErrorAlert("Error in To Offset Format", e.Message, null);
			ea.Run();
			ea.Destroy();
			return;
		}
		
		if (relative != 0)
			toOffset = fromOffset + relative * toOffset - 1;
		
		if (toOffset >= 0 && toOffset < dv.Buffer.Size &&
			fromOffset >= 0 && fromOffset < dv.Buffer.Size) { 
				dv.SetSelection(fromOffset, toOffset);
				dv.Display.MakeOffsetVisible(toOffset, DataViewDisplay.ShowType.Closest);
				// append string to drop-down lists
				ListStore ls = (ListStore)ToEntry.Completion.Model;
				ls.AppendValues(ToEntry.Text);
				ls = (ListStore)FromEntry.Completion.Model;
				ls.AppendValues(FromEntry.Text);
		}
		else {
			ErrorAlert ea=new ErrorAlert("Invalid Offset", "The range you specified is outside the file's limits.", null);
			ea.Run();
			ea.Destroy();
		}
		
	}
	
	void OnWidgetShown(object o, EventArgs args)
	{
		FromEntry.GrabFocus();
	}
	
	protected override bool OnKeyPressEvent(Gdk.EventKey e)
	{
		if (e.Key == Gdk.Key.Escape) {
			CloseButton.Click();
			return true;
		}
		else
			return base.OnKeyPressEvent(e);
	}
	
	void OnCloseButtonClicked(object o, EventArgs args)
	{
		// give focus to active dataview if the widget has it
		if (dataBook.NPages > 0 && IsFocusInWidget()) {
			DataViewDisplay curdvd = (DataViewDisplay)dataBook.CurrentPageWidget;
			curdvd.GrabKeyboardFocus();
		}
		
		this.Hide();
	}
}

} // namespace
