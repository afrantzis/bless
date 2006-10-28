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
using Glade;
using Gtk;
using System;
using Bless.Util;
using Bless.Gui;
using Bless.Plugins;
using Bless.Buffers;
using Bless.Tools;

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
	
	const string uiXml=
	"<menubar>"+
	"	<menu action=\"View\">"+
	"		<menu action=\"Statusbar\" >"+
	"			<menuitem name=\"Show\" action=\"StatusbarShowAction\" />"+
	"			<separator/>"+
	"			<menuitem name=\"Offset\" action=\"StatusbarOffsetAction\" />"+
	"			<menuitem name=\"Selection\" action=\"StatusbarSelectionAction\" />"+
	"			<menuitem name=\"Overwrite\" action=\"StatusbarOverwriteAction\" />"+
	"		</menu>"+
	"	</menu>"+
	"</menubar>";
	
	public InfobarPlugin(Window mw, UIManager uim)
	{
		mainWindow = mw;
		uiManager=uim;
		
		name="Infobar";
		author="Alexandros Frantzis";
		description="Advanced statusbar";
	}
	
	public override bool Load()
	{
		dataBook=(DataBook)GetDataBook(mainWindow);
		
		widget=new Infobar(dataBook);
		widget.Visible=true;
		
		Services.Info=new InfoService(widget);
		
		((VBox)mainWindow.Child).PackEnd(widget, false, false, 0);
		
		AddMenuItems(uiManager);
		
		dataBook.PageAdded += new DataView.DataViewEventHandler(OnDataViewAdded);
		dataBook.Removed += new RemovedHandler(OnDataViewRemoved);
		dataBook.SwitchPage += new SwitchPageHandler(OnSwitchPage);
		
		PreferencesChangedHandler handler=new PreferencesChangedHandler(OnPreferencesChanged);
		Preferences.Proxy.Subscribe("View.Statusbar.Show", "ib1", handler);
		Preferences.Proxy.Subscribe("View.Statusbar.Selection", "ib1", handler);
		Preferences.Proxy.Subscribe("View.Statusbar.Overwrite", "ib1", handler);
		Preferences.Proxy.Subscribe("View.Statusbar.Offset", "ib1", handler);
		
		loaded=true;
		return true;
	}
	
	private void AddMenuItems(UIManager uim)
	{
		ToggleActionEntry[] toggleActionEntries = new ToggleActionEntry[] {
			new ToggleActionEntry ("StatusbarShowAction", null, "Show", null, null,
			                          new EventHandler(OnStatusbarShow), false),
			new ToggleActionEntry ("StatusbarOffsetAction", null, "Offset", null, null,
			                    new EventHandler(OnStatusbarOffset), false),
			new ToggleActionEntry ("StatusbarSelectionAction", null, "Selection", null, null,
			                    new EventHandler(OnStatusbarSelection), false),                    
			new ToggleActionEntry ("StatusbarOverwriteAction", null, "Overwrite", null, null,
			                    new EventHandler(OnStatusbarOverwrite), false)                    
		};
		
		ActionEntry[] actionEntries = new ActionEntry[] {
			new ActionEntry ("Statusbar", null, "Statusbar", null, null, null)
		};
		
		ActionGroup group = new ActionGroup ("StatusbarActions");
		group.Add (toggleActionEntries);
		group.Add (actionEntries);
		
		uim.InsertActionGroup(group, 0);
		uim.AddUiFromString(uiXml);
		
		viewStatusbarShowMenuItem=(CheckMenuItem)uim.GetWidget("/menubar/View/Statusbar/Show");
		viewStatusbarOffsetMenuItem=(CheckMenuItem)uim.GetWidget("/menubar/View/Statusbar/Offset");
		viewStatusbarSelectionMenuItem=(CheckMenuItem)uim.GetWidget("/menubar/View/Statusbar/Selection");
		viewStatusbarOverwriteMenuItem=(CheckMenuItem)uim.GetWidget("/menubar/View/Statusbar/Overwrite");
		
		uim.EnsureUpdate();
	}
	
	void OnDataViewAdded(DataView dv)
	{
		dv.Buffer.Changed += new ByteBuffer.ChangedHandler(OnBufferContentsChanged);
		dv.BufferChanged += new DataView.DataViewEventHandler(OnBufferChanged);
		dv.CursorChanged += new DataView.DataViewEventHandler(OnCursorChanged);
	}
	
	void OnDataViewRemoved(object o, RemovedArgs args)
	{
		DataView dv=((DataViewDisplay)args.Widget).View;
		dv.Buffer.Changed -= new ByteBuffer.ChangedHandler(OnBufferContentsChanged);
		dv.BufferChanged -= new DataView.DataViewEventHandler(OnBufferChanged);
		dv.CursorChanged -= new DataView.DataViewEventHandler(OnCursorChanged);
	}
	
	void OnBufferChanged(DataView dv)
	{
		UpdateInfobar(dv);
	}
	
	void OnBufferContentsChanged(ByteBuffer bb)
	{
		DataView dv=null;
		
		// find DataView that owns bb
		foreach (DataViewDisplay dvtemp in dataBook.Children) {
			if (dvtemp.View.Buffer==bb) {
				dv=dvtemp.View;
				break;	
			}
		}
		
		UpdateInfobar(dv);
	}
	
	void OnSwitchPage(object o, SwitchPageArgs args)
	{
		DataView dv=((DataViewDisplay)dataBook.GetNthPage((int)args.PageNum)).View;
		
		UpdateInfobar(dv);
	}
	
	void OnCursorChanged(DataView dv)
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
			
		DataView curdv=((DataViewDisplay)dataBook.CurrentPageWidget).View;
		if (curdv!=dv)
			return;
		
		widget.Update(dv);
	}
	
	void OnPreferencesChanged(Preferences prefs)
	{
		if (prefs["View.Statusbar.Show"]=="True") {
			viewStatusbarShowMenuItem.Active=true;
			viewStatusbarOffsetMenuItem.Sensitive=true;
			viewStatusbarSelectionMenuItem.Sensitive=true;
			viewStatusbarOverwriteMenuItem.Sensitive=true;
		}
		else {
			viewStatusbarShowMenuItem.Active=false;
			viewStatusbarOffsetMenuItem.Sensitive=false;
			viewStatusbarSelectionMenuItem.Sensitive=false;
			viewStatusbarOverwriteMenuItem.Sensitive=false;
		}
		
		if (Preferences.Instance["View.Statusbar.Offset"]=="True")
			viewStatusbarOffsetMenuItem.Active=true;
		else
			viewStatusbarOffsetMenuItem.Active=false;
		
		if (Preferences.Instance["View.Statusbar.Selection"]=="True")
			viewStatusbarSelectionMenuItem.Active=true;
		else
			viewStatusbarSelectionMenuItem.Active=false;
		
		if (Preferences.Instance["View.Statusbar.Overwrite"]=="True")
			viewStatusbarOverwriteMenuItem.Active=true;
		else
			viewStatusbarOverwriteMenuItem.Active=false;
	}
}

///<summary>
/// An advanced statusbar for Bless
///</summary>
public class Infobar : Gtk.HPaned, IInfoDisplay
{
	Statusbar MessageStatusbar;
	Statusbar DummyStatusbar;
	Statusbar OffsetStatusbar;
	Statusbar SelectionStatusbar;
	Statusbar OverwriteStatusbar;
	Tooltips tips;
	
	DataBook dataBook;
	
	int numberBase;
	
	///<summary>
	///Get or set the visibility of the Offset statusbar
	///</summary>
	public bool OffsetVisible {
		get { return OffsetStatusbar.Visible; }
		set { OffsetStatusbar.Visible=value; AssignResizeGrip(); }
	}
	
	///<summary>
	///Get or set the visibility of the Selection statusbar
	///</summary>
	public bool SelectionVisible {
		get { return SelectionStatusbar.Visible; }
		set { SelectionStatusbar.Visible=value; AssignResizeGrip(); }
	}
	
	///<summary>
	///Get or set the visibility of the Overwrite statusbar
	///</summary>
	public bool OverwriteVisible {
		get { return OverwriteStatusbar.Visible; }
		set { OverwriteStatusbar.Visible=value; AssignResizeGrip(); }
	}
	
	public int NumberBase {
		set { 
			numberBase=value;
			
			if (dataBook.NPages>0) {
				DataView curdv=((DataViewDisplay)dataBook.CurrentPageWidget).View;
			
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
		dataBook=db;
		
		// create bars
		MessageStatusbar=new Statusbar();
		
		OffsetStatusbar=new Statusbar();
		OffsetStatusbar.WidthRequest=270;
		
		SelectionStatusbar=new Statusbar();
		SelectionStatusbar.WidthRequest=270;
		
		OverwriteStatusbar=new Statusbar();
		OverwriteStatusbar.WidthRequest=60;
		
		DummyStatusbar=new Statusbar();
		DummyStatusbar.Visible=false;
		
		EventBox OffsetEB=new EventBox();
		OffsetEB.Add(OffsetStatusbar);
		OffsetEB.ButtonPressEvent+=ChangeNumberBase;
		
		EventBox SelectionEB=new EventBox();
		SelectionEB.Add(SelectionStatusbar);
		SelectionEB.ButtonPressEvent+=ChangeNumberBase;
		
		EventBox OverwriteEB=new EventBox();
		OverwriteEB.Add(OverwriteStatusbar);
		OverwriteEB.ButtonPressEvent+=OnOverwriteStatusbarPressed;
		
		// create hbox to put bars in
		HBox StatusHBox=new HBox();
		StatusHBox.PackStart(DummyStatusbar, false, true, 0);
		StatusHBox.PackStart(OffsetEB, false, true, 0);
		StatusHBox.PackStart(SelectionEB, false, true, 0);
		StatusHBox.PackStart(OverwriteEB, false, true, 0);
		
		// align the hbox
		Alignment StatusAlignment=new Alignment(1.0f, 0.5f, 0.0f, 1.0f);
		StatusAlignment.Add(StatusHBox);
		
		this.Pack1(MessageStatusbar, true, true);
		this.Pack2(StatusAlignment, false, true);
		
		this.NumberBase=16;
		
		PreferencesChangedHandler handler=new PreferencesChangedHandler(OnPreferencesChanged);
		Preferences.Proxy.Subscribe("View.Statusbar.Show", "ib2", handler);
		Preferences.Proxy.Subscribe("View.Statusbar.Selection", "ib2", handler);
		Preferences.Proxy.Subscribe("View.Statusbar.Overwrite", "ib2", handler);
		Preferences.Proxy.Subscribe("View.Statusbar.Offset", "ib2", handler);
		
		this.MapEvent+=OnMapEvent;
		this.ShowAll();
	}

	void OnMapEvent(object o, EventArgs args)
	{
		AssignResizeGrip();
	}
	
	///<summary>Displays a message in the infobar (for 5 sec)</summary>
	public void DisplayMessage(string message)
	{
		MessageStatusbar.Pop(0);	
		MessageStatusbar.Push(0, message);
		GLib.Timeout.Add(5000, ClearStatusMessage);
	}

	///<summary>Clears the status bar message (callback)</summary>
	bool ClearStatusMessage()
	{
		MessageStatusbar.Pop(0);
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
		
		DataView curdv=((DataViewDisplay)dataBook.CurrentPageWidget).View;
		if (curdv!=dv)
			return;
	
		long coffset=dv.CursorOffset;
		OffsetStatusbar.Pop(0);
		
		string coffsetString=BaseConverter.ConvertToString(coffset, numberBase, true, false); 
		string sizeString=BaseConverter.ConvertToString(dv.Buffer.Size-1, numberBase, true, false);
		
		string str=string.Format("Offset: {0} / {1}", coffsetString, sizeString);
		OffsetStatusbar.Push(0, str);
	}
	
	///<summary>
	/// Updates the selection status bar with data from the specified DataView,
	/// if the specified DataView is active
	///</summary>
	public void UpdateSelection(DataView dv)
	{
		if (dataBook.NPages == 0)
			return;

		DataView curdv=((DataViewDisplay)dataBook.CurrentPageWidget).View;
		if (curdv!=dv)
			return;
	
		Bless.Util.Range sel=dv.Selection;
		sel.Sort();
		SelectionStatusbar.Pop(0);
		string str;
	
		if (sel.IsEmpty()==true)
			str="Selection: None";
		else {
			string startString=BaseConverter.ConvertToString(sel.Start, numberBase, true, false);
			string endString=BaseConverter.ConvertToString(sel.End, numberBase, true, false);
			string sizeString=BaseConverter.ConvertToString(sel.Size, numberBase, true, false);
			
			str=string.Format("Selection: {0} to {1} ({2} bytes)", startString, endString, sizeString);
		}
		
		SelectionStatusbar.Push(0, str);
	}
	
	///<summary>
	/// Updates the overwrite status bar with data from the specified DataView,
	/// if the specified DataView is active
	///</summary>
	public void UpdateOverwrite(DataView dv)
	{	
		if (dataBook.NPages == 0)
			return;
		
		DataView curdv=((DataViewDisplay)dataBook.CurrentPageWidget).View;
		if (curdv!=dv)
			return;
	
		OverwriteStatusbar.Pop(0);
		if (dv.Overwrite==true)
			OverwriteStatusbar.Push(0, " OVR");
		else
			OverwriteStatusbar.Push(0, " INS");
	}
	
	///<summary>
	/// Clears the information displayed in the infobar
	///</summary>
	public void ClearMessage()
	{
		OffsetStatusbar.Pop(0);
		OffsetStatusbar.Push(0, "Offset: -");
			
		SelectionStatusbar.Pop(0);
		SelectionStatusbar.Push(0, "Selection: None");
	}
	
	///<summary>Handle button press on Ins/Ovr statusbar</summary>
	void OnOverwriteStatusbarPressed (object o, ButtonPressEventArgs args) 
	{
		if (dataBook.NPages > 0) {
			Gdk.EventButton e=args.Event;
			// ignore double and triple-clicks
			if (e.Type!=Gdk.EventType.ButtonPress)
				return;
			
			// set edit mode
			DataView dv=((DataViewDisplay)dataBook.CurrentPageWidget).View;
			dv.Overwrite=!dv.Overwrite;
		}
	}
	
	void ChangeNumberBase(object o, ButtonPressEventArgs args)
	{
		if (dataBook.NPages == 0)
			return;
			
		Gdk.EventButton e=args.Event;
		// ignore double and triple-clicks
		if (e.Type!=Gdk.EventType.ButtonPress)
			return;
		
		// cycle 8, 10 and 16 number bases
		if (this.NumberBase==8)
			this.NumberBase=10;
		else if (this.NumberBase==10)
			this.NumberBase=16;
		else if (this.NumberBase==16)
			this.NumberBase=8;
	}
	
	///<summary>Assign the resize grip to a visible statusbar</summary>
	void AssignResizeGrip()
	{
		// clear all grips
		OffsetStatusbar.HasResizeGrip=false;
		SelectionStatusbar.HasResizeGrip=false;
		MessageStatusbar.HasResizeGrip=false;
		OverwriteStatusbar.HasResizeGrip=false;
		DummyStatusbar.Visible=false;
		
		// give resize grip to a statusbar
		if (OverwriteStatusbar.Visible==false){
			if (SelectionStatusbar.Visible==false) {
				if (OffsetStatusbar.Visible==false)
					DummyStatusbar.Visible=true; 
				else
					OffsetStatusbar.HasResizeGrip=true;
			}
			else
				SelectionStatusbar.HasResizeGrip=true;	
		}
		else
			OverwriteStatusbar.HasResizeGrip=true;
	}
	
	void OnPreferencesChanged(Preferences prefs)
	{
		if (prefs["View.Statusbar.Show"]=="True")
			this.Visible=true;
		else 
			this.Visible=false;
		
		if (Preferences.Instance["View.Statusbar.Offset"]=="True")
			this.OffsetVisible=true;
		else
			this.OffsetVisible=false;
		
		if (Preferences.Instance["View.Statusbar.Selection"]=="True")
			this.SelectionVisible=true;
		else
			this.SelectionVisible=false;
		
		if (Preferences.Instance["View.Statusbar.Overwrite"]=="True")
			this.OverwriteVisible=true;
		else
			this.OverwriteVisible=false;
	}
}

} // end namespace
