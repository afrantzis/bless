// created on 4/30/2006 at 12:59 PM
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
using Bless.Buffers;
using Bless.Gui;
using Bless.Plugins;
using Bless.Gui.Dialogs;
using Bless.Tools;
using System;
using System.IO;
using Gtk;

namespace Bless.Gui.Plugins {
	
public class EditOperationsPlugin : GuiPlugin
{
	Gtk.Action UndoAction;
	Gtk.Action RedoAction;
	Gtk.Action CutAction;
	Gtk.Action CopyAction;
	Gtk.Action PasteAction;
	Gtk.Action DeleteAction;
		
	const string uiXml=
	"<menubar>"+
	"	<menu action=\"Edit\">"+
	"		<menuitem name=\"Undo\" action=\"UndoAction\" />"+
	"		<menuitem name=\"Redo\" action=\"RedoAction\" />"+
	"		<separator/>"+
	"		<menuitem name=\"Cut\" action=\"CutAction\" />"+
	"		<menuitem name=\"Copy\" action=\"CopyAction\" />"+
	"		<menuitem name=\"Paste\" action=\"PasteAction\" />"+
	"		<menuitem name=\"Delete\" action=\"DeleteAction\" />"+
	"		<separator/>"+
	"		<placeholder name=\"Extra\" />"+
	"		<separator/>"+
	"		<menuitem name=\"Preferences\" action=\"PreferencesAction\" />"+
	"	</menu>"+
	"</menubar>"+
	"<toolbar>"+
	"	<placeholder name=\"EditItems\">"+
	"		<toolitem name=\"Undo\" action=\"UndoAction\"/>"+
	"		<toolitem name=\"Redo\" action=\"RedoAction\"/>"+
	"		<separator/>"+
	"		<toolitem name=\"Cut\" action=\"CutAction\" />"+
	"		<toolitem name=\"Copy\" action=\"CopyAction\" />"+
	"		<toolitem name=\"Paste\" action=\"PasteAction\" />"+
	"	</placeholder>"+
	"</toolbar>";
	
	DataBook dataBook;
	Window mainWindow;
	UIManager uiManager;
	ActionGroup editActionGroup;
	int editAccelCount=0;
	
	public EditOperationsPlugin(Window mw, UIManager uim)
	{
		mainWindow=mw;
		uiManager=uim;
		
		name="EditOperations";
		author="Alexandros Frantzis";
		description="Provides access to basic edit operations";
	}
	
	public override bool Load()
	{
		dataBook=(DataBook)GetDataBook(mainWindow);
		
		AddActions(uiManager);
		
		dataBook.PageAdded += new DataView.DataViewEventHandler(OnDataViewAdded);
		dataBook.PageRemoved += new DataView.DataViewEventHandler(OnDataViewRemoved);
		dataBook.SwitchPage += new SwitchPageHandler(OnSwitchPage);
		
		loaded=true;
		return true;
	}
	
	
	void OnDataViewAdded(DataView dv)
	{
		dv.Buffer.Changed += new ByteBuffer.ChangedHandler(OnBufferContentsChanged);
		dv.BufferChanged += new DataView.DataViewEventHandler(OnBufferChanged);
		dv.FocusChanged += new DataView.DataViewEventHandler(OnDataViewFocusChanged);
	}
	
	void OnDataViewRemoved(DataView dv)
	{
		dv.Buffer.Changed -= new ByteBuffer.ChangedHandler(OnBufferContentsChanged);
		dv.BufferChanged -= new DataView.DataViewEventHandler(OnBufferChanged);
		dv.FocusChanged -= new DataView.DataViewEventHandler(OnDataViewFocusChanged);
		// if there are no pages left update the menu
		if (dataBook.NPages==0)
			UpdateActions(null);
	}
	
	void OnBufferChanged(DataView dv)
	{
		UpdateActions(dv);
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
		
		UpdateActions(dv);
	}
	
	void OnSwitchPage(object o, SwitchPageArgs args)
	{
		DataView dv=((DataViewDisplay)dataBook.GetNthPage((int)args.PageNum)).View;
		
		UpdateActions(dv);
	}
	
	void ConnectEditAccelerators(bool v)
	{
		if (editAccelCount==0 && v==true) {	
			foreach(Action a in editActionGroup.ListActions())
				a.ConnectAccelerator();
			editAccelCount=1;
		}
		else if (editAccelCount==1 && v==false) {
			foreach(Action a in editActionGroup.ListActions())
				a.DisconnectAccelerator();
			editAccelCount=0;
		}
		
	}
	
	void OnDataViewFocusChanged(DataView dv)
	{
		// if a DataView gained the focus
		// enable Edit Menu Accelerators
		if (dv.Display.HasFocus) {
			ConnectEditAccelerators(true);
			
		}
		else { // disable them, so that key combos in other
		// widgets behave correctly (eg Ctrl+C in Text Entries)
			ConnectEditAccelerators(false);
		}
	}
	
	private void AddActions(UIManager uim)
	{
		ActionEntry[] actionEntries = new ActionEntry[] {
			new ActionEntry ("CutAction", Stock.Cut, "Cu_t", "<control>X", "Cut",
			                    new EventHandler(OnCutActivated)),
			new ActionEntry ("CopyAction", Stock.Copy, "_Copy", "<control>C", "Copy",
			                    new EventHandler(OnCopyActivated)),
			new ActionEntry ("PasteAction", Stock.Paste, "_Paste", "<control>V", "Paste",
			                    new EventHandler(OnPasteActivated)),
			new ActionEntry ("DeleteAction", Stock.Delete, "_Delete", "Delete", "Delete",
			                    new EventHandler(OnDeleteActivated))
		};		
		ActionEntry[] miscActionEntries = new ActionEntry[] {
			new ActionEntry ("UndoAction", Stock.Undo, "_Undo", "<control>Z", "Undo",
			                    new EventHandler(OnUndoActivated)),
			new ActionEntry ("RedoAction", Stock.Redo, "_Redo", "<shift><control>Z", "Redo",
			                    new EventHandler(OnRedoActivated)),
			new ActionEntry ("PreferencesAction", Stock.Preferences, "_Preferences...", null, "Preferences",
			                    new EventHandler(OnPreferencesActivated))
		};
		
		editActionGroup=new ActionGroup("EditActions");
		editActionGroup.Add (actionEntries);
		ActionGroup miscActionGroup = new ActionGroup ("MiscEditActions");
		miscActionGroup.Add (miscActionEntries);
		
		uim.InsertActionGroup(editActionGroup, 0);
		uim.InsertActionGroup(miscActionGroup, 0);

		uim.AddUiFromString(uiXml);
		UndoAction=(Action)uim.GetAction("/menubar/Edit/Undo");
		RedoAction=(Action)uim.GetAction("/menubar/Edit/Redo");
		CutAction=(Action)uim.GetAction("/menubar/Edit/Cut");
		CopyAction=(Action)uim.GetAction("/menubar/Edit/Copy");
		PasteAction=(Action)uim.GetAction("/menubar/Edit/Paste");
		DeleteAction=(Action)uim.GetAction("/menubar/Edit/Delete");
		
		foreach (Action a in editActionGroup.ListActions()) {
				a.DisconnectAccelerator();	
		}
		editAccelCount=0;
		uim.EnsureUpdate();
		
	}
	///<summary>Handle edit->undo command from menu</summary>
	public void OnUndoActivated(object o, EventArgs args) 
	{
		if (dataBook.NPages == 0)
			return;
		DataView dv=((DataViewDisplay)dataBook.CurrentPageWidget).View;
		dv.Undo();
	}
	
	///<summary>Handle edit->redo command from menu</summary>
	public void OnRedoActivated(object o, EventArgs args) 
	{
		if (dataBook.NPages == 0)
			return;
		DataView dv=((DataViewDisplay)dataBook.CurrentPageWidget).View;
		dv.Redo();
	}
	
	///<summary>Handle edit->cut command from menu</summary>
	public void OnCutActivated(object o, EventArgs args) 
	{
		if (dataBook.NPages == 0)
			return;
		DataView dv=((DataViewDisplay)dataBook.CurrentPageWidget).View;
		dv.Cut();
	}
	
	///<summary>Handle edit->copy command from menu</summary>
	public void OnCopyActivated(object o, EventArgs args) 
	{
		if (dataBook.NPages == 0)
			return;
		DataView dv=((DataViewDisplay)dataBook.CurrentPageWidget).View;
		dv.Copy();
	}
	
	///<summary>Handle edit->paste command from menu</summary>
	public void OnPasteActivated(object o, EventArgs args) 
	{
		if (dataBook.NPages == 0)
			return;
		DataView dv=((DataViewDisplay)dataBook.CurrentPageWidget).View;
		dv.Paste();
	}
	
	///<summary>Handle edit->delete command from menu</summary>
	public void OnDeleteActivated(object o, EventArgs args) 
	{
		if (dataBook.NPages == 0)
			return;
		DataView dv=((DataViewDisplay)dataBook.CurrentPageWidget).View;
		dv.Delete();
	}
	
	///<summary>Handle edit->Select Range command from menu</summary>
	public void OnSelectRange(object o, EventArgs args) 
	{
		if (dataBook.NPages == 0)
			return;
		//DataView dv=((DataViewDisplay)dataBook.CurrentPageWidget).View;
		
		//selectRangeWidget.Show();
	}
	
	///<summary>Handle edit->preferences command from menu</summary>
	public void OnPreferencesActivated(object o, EventArgs args) 
	{
		PreferencesDialog pd=new PreferencesDialog(Preferences.Instance, mainWindow);
		pd.Show();
	}
	
	///<summary>
	/// Updates various menu items according to the ModifyAllowed 
	/// permission property of the active ByteBuffer
	///</summary>
	void UpdateActions(DataView dv)
	{
		if (dv==null) {
			CutAction.Sensitive=false;
			PasteAction.Sensitive=false;
			DeleteAction.Sensitive=false;
			UndoAction.Sensitive=false;
			RedoAction.Sensitive=false;
			CopyAction.Sensitive=false;
			return;
		}
		
		DataView curdv=((DataViewDisplay)dataBook.CurrentPageWidget).View;
		
		ByteBuffer curbb=curdv.Buffer;
	
		ByteBuffer bb=dv.Buffer;
	
		// if DataView is active
		if (curbb==bb) {
			// gray various menu items
			// if we are not allowed to modify the buffer 
			if (!bb.ModifyAllowed) {
				CutAction.Sensitive=false;
				PasteAction.Sensitive=false;
				DeleteAction.Sensitive=false;
			}
			else {
				CutAction.Sensitive=true;
				PasteAction.Sensitive=true;
				DeleteAction.Sensitive=true;
			}
			
			// gray Undo if there is no undo action
			// or we aren't allowed to modify the file
			if (bb.CanUndo && bb.ModifyAllowed) {
				UndoAction.Sensitive=true;
			}
			else {
				UndoAction.Sensitive=false;
			}
			
			// gray Redo if there is no redo action
			// or we aren't allowed to modify the file
			if (bb.CanRedo && bb.ModifyAllowed) {
				RedoAction.Sensitive=true;
			}
			else {
				RedoAction.Sensitive=false;
			}
			
			// gray various menu items
			// if we are not allowed to read from the buffer  
			if (!bb.ReadAllowed) {
				CopyAction.Sensitive=false;
			}
			else {
				CopyAction.Sensitive=true;
			}
		}
	}
	
}

} //end namespace
