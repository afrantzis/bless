// created on 4/29/2006 at 2:39 PM
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
using System;
using System.IO;
using Gtk;

namespace Bless.Gui.Plugins {
	
public class FileOperationsPlugin : GuiPlugin
{
	Gtk.Action SaveAction;
	Gtk.Action SaveAsAction;
	Gtk.Action CloseAction;
	Gtk.Action QuitAction;
	Gtk.Action RevertAction;
	
	const string uiXml=
	"<menubar>"+
	"	<menu action=\"File\">"+
	"		<menuitem name=\"New\" action=\"NewAction\" />"+
	"		<menuitem name=\"Open\" action=\"OpenAction\" />"+
	"		<separator/>"+
	"		<menuitem name=\"Save\" action=\"SaveAction\" />"+
	"		<menuitem name=\"SaveAs\" action=\"SaveAsAction\" />"+
	"		<menuitem name=\"Revert\" action=\"RevertAction\" />"+
	"		<separator/>"+
	"		<placeholder name=\"HistoryItems\"/>"+
	"		<separator/>"+
	"		<menuitem name=\"Close\" action=\"CloseAction\" />"+
	"		<menuitem name=\"Quit\" action=\"QuitAction\" />"+
	"	</menu>"+
	"</menubar>"+
	"<toolbar>"+
	"	<placeholder name=\"FileItems\">"+
	"		<toolitem name=\"New\" action=\"NewAction\"/>"+
	"		<toolitem name=\"Open\" action=\"OpenAction\"/>"+
	"		<toolitem name=\"Save\" action=\"SaveAction\"/>"+
	"	</placeholder>"+
	"</toolbar>";
	
	DataBook dataBook;
	Window mainWindow;
	UIManager uiManager;
	
	public FileOperationsPlugin(Window mw, UIManager uim)
	{
		mainWindow=mw;
		uiManager=uim;
				
		name="FileOperation";
		author="Alexandros Frantzis";
		description="Provides access to basic file operations";
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
		dv.Buffer.PermissionsChanged += new ByteBuffer.ChangedHandler(OnBufferPermissionsChanged);
		dv.BufferChanged += new DataView.DataViewEventHandler(OnBufferChanged);
	}
	
	void OnDataViewRemoved(DataView dv)
	{
		dv.Buffer.PermissionsChanged -= new ByteBuffer.ChangedHandler(OnBufferPermissionsChanged);
		dv.BufferChanged -= new DataView.DataViewEventHandler(OnBufferChanged);
		// if there are no pages left update the menu
		if (dataBook.NPages==0)
			UpdateActions(null);
	}
	
	void OnBufferChanged(DataView dv)
	{
		UpdateActions(dv);
	}
	
	void OnBufferPermissionsChanged(ByteBuffer bb)
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
	
	private void AddActions(UIManager uim)
	{
		ActionEntry[] actionEntries = new ActionEntry[] {
			new ActionEntry ("NewAction", Stock.New, "_New", "<control>N", "New File",
			                    new EventHandler(OnNewActivated)),
			new ActionEntry ("OpenAction", Stock.Open, "_Open", "<control>O", "Open File",
			                    new EventHandler(OnOpenActivated)),
			new ActionEntry ("SaveAction", Stock.Save, "_Save", "<control>S", "Save File",
			                    new EventHandler(OnSaveActivated)),
			new ActionEntry ("SaveAsAction", Stock.SaveAs, "Save _As", "<shift><control>S", "Save File As",
			                    new EventHandler(OnSaveAsActivated)),
			new ActionEntry ("RevertAction", Stock.RevertToSaved, "_Revert", null, "Revert File",
			                    new EventHandler(OnRevertActivated)),
			new ActionEntry ("CloseAction", Stock.Close, "_Close", "<control>W", "Close File",
			                    new EventHandler(OnCloseActivated)),
			new ActionEntry ("QuitAction", Stock.Quit, "_Quit", "<control>Q", "Quit Application",
			                    new EventHandler(OnQuitActivated))
		};
		
		ActionGroup group = new ActionGroup ("FileActions");
		group.Add (actionEntries);
		
		uim.InsertActionGroup(group, 0);
		uim.AddUiFromString(uiXml);
		SaveAction=(Action)uim.GetAction("/menubar/File/Save");
		SaveAsAction=(Action)uim.GetAction("/menubar/File/SaveAs");
		CloseAction=(Action)uim.GetAction("/menubar/File/Close");
		QuitAction=(Action)uim.GetAction("/menubar/File/Quit");
		RevertAction=(Action)uim.GetAction("/menubar/File/Revert");
		
		uim.EnsureUpdate();
		
	}
	
	///<summary>Handle file->new command from menu</summary>
	public void OnNewActivated(object o, EventArgs args) 
	{
		ByteBuffer bb=new ByteBuffer();
		bb.UseGLibIdle=true;
		
		//bufferHasChanged[bb]=true;
	
		// create and setup a  DataView
		DataView dv=Services.File.CreateDataView(bb);
	
		dataBook.AppendView(dv, new CloseViewDelegate(Services.File.CloseFile), Path.GetFileName(bb.Filename));
	}
	
	///<summary>Handle file->open command from menu</summary>
	public void OnOpenActivated(object o, EventArgs args) 
	{
		// Get path of file(s) to open	
		Gtk.FileChooserDialog fs=new Gtk.FileChooserDialog("Open File(s)", mainWindow, FileChooserAction.Open,
			Gtk.Stock.Cancel, ResponseType.Cancel,
			Gtk.Stock.Open, ResponseType.Accept);
		
		fs.SelectMultiple=true;
		ResponseType result = (ResponseType)fs.Run();
		fs.Hide();
		
		// if user does not cancel load the file(s)
		if (result == ResponseType.Accept)
			Services.File.LoadFiles(fs.Filenames);
	
		fs.Destroy();
	}
	
	///<summary>Handle file->save command from menu</summary>
	public void OnSaveActivated(object o, EventArgs args)
	{
		if (dataBook.NPages == 0)
			return;
	
		DataView dv=((DataViewDisplay)dataBook.CurrentPageWidget).View;
	
		Services.File.SaveFile(dv, null, false, false);
	}
	
	///<summary>Handle file->save as command from menu</summary>
	public void OnSaveAsActivated(object o, EventArgs args)
	{
		if (dataBook.NPages == 0)
			return;
	
		DataView dv=((DataViewDisplay)dataBook.CurrentPageWidget).View;
	
		Services.File.SaveFile(dv, null, true, false);
	}
	
	///<summary>Handle file->revert command from menu</summary>
	public void OnRevertActivated(object o, EventArgs args)
	{
		if (dataBook.NPages == 0)
			return;
	
		DataView dv=((DataViewDisplay)dataBook.CurrentPageWidget).View;
		
		RevertConfirmationAlert rca=new RevertConfirmationAlert(dv.Buffer.Filename, mainWindow);
		ResponseType response=(ResponseType)rca.Run();
		rca.Destroy();
		
		try {
			if (response==ResponseType.Ok)
				dv.Revert();
		}
		catch (FileNotFoundException ex) {
			ErrorAlert ea=new ErrorAlert("Error reverting file '"+ ex.Message+"'.","The file cannot be found. Perhaps it has been recently deleted." , mainWindow);
			ea.Run();
			ea.Destroy();
		}
	}
	
	///<summary>Handle file->close as command from menu</summary>
	public void OnCloseActivated(object o, EventArgs args)
	{
		if (dataBook.NPages == 0)
			return;
		DataView dv=((DataViewDisplay)dataBook.CurrentPageWidget).View;
	
		Services.File.CloseFile(dv);
	}
	
	
	///<summary>Handle file->quit command from menu</summary>
	public void OnQuitActivated(object o, EventArgs args) 
	{
		Services.File.TryQuit();
	}
	
	///<summary>
	/// Updates various menu items according to the FileOperationsAllowed
	/// permission property of the active ByteBuffer
	///</summary>
	void UpdateActions(DataView dv)
	{
		if (dv==null) {
			SaveAction.Sensitive=false;
			SaveAsAction.Sensitive=false;
			CloseAction.Sensitive=false;
			RevertAction.Sensitive=false;
			return;
		}
	
		DataView curdv=((DataViewDisplay)dataBook.CurrentPageWidget).View;
		ByteBuffer curbb=curdv.Buffer;
	
		ByteBuffer bb=dv.Buffer;
		
		// if DataView is active
		if (curbb==bb) {
			// gray various menu items
			// if FileOperations aren't allowed
			if (!bb.FileOperationsAllowed) {
				SaveAction.Sensitive=false;
				SaveAsAction.Sensitive=false;
				CloseAction.Sensitive=false;
				
				//SaveToolButton.Sensitive=false;
			}
			else {
				SaveAction.Sensitive=true;
				SaveAsAction.Sensitive=true;
				CloseAction.Sensitive=true;
				
				//SaveToolButton.Sensitive=true;
			}
			
			if (dv.Buffer.HasFile && dv.Buffer.ModifyAllowed)
				RevertAction.Sensitive=true;
			else
				RevertAction.Sensitive=false;
		}
		
		
		// If even one ByteBuffer in the dataBook
		// doesn't allow file operations
		// gray the Quit menu item.
		QuitAction.Sensitive=true; // set initially to true
		
		foreach (DataViewDisplay dd in dataBook.Children) {
			if (!dd.View.Buffer.FileOperationsAllowed) {
				QuitAction.Sensitive=false;
				dataBook.SetCloseSensitivity(dd, false);
			}
			else
				dataBook.SetCloseSensitivity(dd, true);
		}
	}
	
}

}
