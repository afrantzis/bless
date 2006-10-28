// created on 6/22/2005 at 12:42 PM
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
using Gtk;
using Glade;
using Bless.Util;
using Bless.Gui.Dialogs;
using Bless.Gui;
using Bless.Tools.Find;
using Bless.Plugins;

namespace Bless.Gui.Plugins {
	
public class GotoOffsetPlugin : GuiPlugin
{	
	GotoOffsetWidget widget;
	
	const string uiXml=
	"<menubar>"+
	"	<menu action=\"Search\">"+
	"		<menuitem name=\"GotoOffset\" action=\"GotoOffsetAction\" />"+
	"		<separator/>"+
	"	</menu>"+
	"</menubar>";
	
	Window mainWindow;
	UIManager uiManager;
	
	public GotoOffsetPlugin(Window mw, UIManager uim)
	{
		mainWindow=mw;
		uiManager=uim;
		
		name="GotoOffset";
		author="Alexandros Frantzis";
		description="Adds a firefox like go to offset bar";
	}
	
	public override bool Load()
	{
		widget=new GotoOffsetWidget((DataBook)GetDataBook(mainWindow));
		widget.Visible=false;
		
		WidgetGroup wgroup=(WidgetGroup)GetWidgetGroup(mainWindow, 0);
		wgroup.Add(widget);
		
		// add the menu items
		AddMenuItems(uiManager);
		
		loaded=true;
		return true;
	}
	
	private void AddMenuItems(UIManager uim)
	{
		ActionEntry[] actionEntries = new ActionEntry[] {
			new ActionEntry ("GotoOffsetAction", Stock.JumpTo, "_Goto Offset", "<control>G", null,
			                    new EventHandler(OnGotoOffsetActivated)),
		};
		
		ActionGroup group = new ActionGroup ("GotoActions");
		group.Add (actionEntries);
		
		uim.InsertActionGroup(group, 0);
		uim.AddUiFromString(uiXml);
		
		uim.EnsureUpdate();
		
	}
	
	///<summary>Handle the Search->Find Next command</summary>
	public void OnGotoOffsetActivated(object o, EventArgs args)
	{
		widget.Show();
	}
}
	
///<summary>
/// A widget for go to offset operation
///</summary>
public class GotoOffsetWidget : Gtk.HBox
{
	[Glade.Widget] Gtk.HBox GotoOffsetHBox;
	[Glade.Widget] Gtk.Button GotoOffsetButton;
	[Glade.Widget] Gtk.Entry OffsetEntry;
	[Glade.Widget] Gtk.Button CloseButton;
	
	DataBook dataBook;
	
	
	public GotoOffsetWidget(DataBook db)
	{
		dataBook=db;
		
		Glade.XML gxml = new Glade.XML (FileResourcePath.GetSystemPath("..","data","bless.glade"), "GotoOffsetHBox", null);
		gxml.Autoconnect (this);
		
		OffsetEntry.Completion=new EntryCompletion();
		OffsetEntry.Completion.Model=new ListStore (typeof (string));
		OffsetEntry.Completion.TextColumn = 0;
		
		// set button sensitivity 
		OnOffsetEntryChanged(null, null);
		
		this.Shown+=OnWidgetShown;
		
		this.Add(GotoOffsetHBox);
		this.ShowAll();
	}
	
	///<summary>
	/// Whether a widget in the GotoOffsetWidget has the focus 
	///</summary>
	bool IsFocusInWidget()
	{
		foreach (Gtk.Widget child in  GotoOffsetHBox.Children) {
			Widget realChild=child;
			
			if (child.GetType()==typeof(Gtk.Alignment))
				realChild=(child as Gtk.Alignment).Child;
				
			if (realChild.HasFocus)
				return true;
		}
		
		return false;
	}
	
	void OnOffsetEntryChanged(object o, EventArgs args)
	{
		if (OffsetEntry.Text.Length > 0)
			GotoOffsetButton.Sensitive=true;
		else
			GotoOffsetButton.Sensitive=false;
	}
	
	void OnOffsetEntryActivated(object o, EventArgs args)
	{
		if (GotoOffsetButton.Sensitive==true)
			GotoOffsetButton.Click();
	}
	
	void OnGotoOffsetClicked(object o, EventArgs args)
	{
		if (dataBook.NPages==0)
			return;
			
		DataView dv=((DataViewDisplay)dataBook.CurrentPageWidget).View;
		
		long offset=-1;
		
		try {
			offset=BaseConverter.Parse(OffsetEntry.Text);
			
			if (offset>=0 && offset<=dv.Buffer.Size) {
				dv.Display.MakeOffsetVisible(offset, DataViewDisplay.ShowType.Closest);
				dv.MoveCursor(offset, 0);
			}
			else {
				ErrorAlert ea=new ErrorAlert("Invalid Offset", "The offset you specified is outside the file's limits.", null);
				ea.Run();
				ea.Destroy();
			}
			
			// append string to drop-down list
			ListStore ls=(ListStore)OffsetEntry.Completion.Model;
			ls.AppendValues(OffsetEntry.Text);
		}
		catch(FormatException e) {
			ErrorAlert ea=new ErrorAlert("Error in Offset Format", e.Message, null);
			ea.Run();
			ea.Destroy();
		}
		
		
	}
	
	void OnWidgetShown(object o, EventArgs args)
	{
		OffsetEntry.GrabFocus();
	}
	
	protected override bool OnKeyPressEvent(Gdk.EventKey e)
	{
		if (e.Key==Gdk.Key.Escape) {
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
			DataViewDisplay curdvd=(DataViewDisplay)dataBook.CurrentPageWidget;
			curdvd.GrabKeyboardFocus();
		}
		
		this.Hide();
	}
}

} // namespace
