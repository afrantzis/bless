// created on 4/29/2006 at 3:37 PM
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
using Gtk;
using Bless.Buffers;
using Bless.Gui.Dialogs;
using Bless.Tools;
using Bless.Util;
using System;
using System.IO;
using System.Collections;
using Mono.Unix;
 
namespace Bless.Gui
{

public class FileService
{
	DataBook dataBook;
	Window mainWindow;
	History history;
	
	
	public FileService(DataBook db, Window mw, History h)
	{
		dataBook=db;
		mainWindow=mw;
		history=h;
	}
	
	///<summary>
	/// Create and setup a DataView 
	///</summary>
	public DataView CreateDataView(ByteBuffer bb)
	{
		DataView dv=new DataView();
		
		string layoutFile=string.Empty;
		
		// try to load default (from user preferences) layout file
		try {
			layoutFile=Preferences.Instance["Default.Layout.File"];
			string useCurrent=Preferences.Instance["Default.Layout.UseCurrent"];
			if (useCurrent=="True" && dataBook.NPages > 0) {
				DataViewDisplay dvd=(DataViewDisplay)dataBook.CurrentPageWidget;
				layoutFile=dvd.Layout.FilePath;
			}
			
			if (layoutFile!=string.Empty)
				dv.Display.Layout=new Bless.Gui.Layout(layoutFile);
		}
		catch(Exception ex) {
			string msg = string.Format(Catalog.GetString("Error loading layout '{0}'. Loading default layout instead."), layoutFile); 
			ErrorAlert ea=new ErrorAlert(msg, ex.Message , mainWindow);
			ea.Run();
			ea.Destroy();
		}
		
		if (Preferences.Instance["Default.EditMode"]=="Insert")
			dv.Overwrite=false;
		else if (Preferences.Instance["Default.EditMode"]=="Overwrite")
			dv.Overwrite=true;
		
		dv.Buffer=bb;
		
		return dv;
	}
	
	///<summary>If file has changed ask whether to save it.
	/// Returns true if file saved or the user doesn't want to save it.</summary>
	private bool AskForSaveIfFileChanged(DataView dv)
	{
		ByteBuffer bb=dv.Buffer;
	
		if (bb == null)
			return true;
	
		if (bb.HasChanged) {
			SaveConfirmationAlert sca=new SaveConfirmationAlert(bb.Filename, mainWindow);
			ResponseType res=(ResponseType)sca.Run();
			sca.Destroy();
	
			if (res== ResponseType.Ok) {
				return SaveFile(dv, null, false, true);
			}
			else if (res==ResponseType.No){
				return true;
			}
			else /*if (res==ResponseType.Cancel)*/
				return false;
		}
	
		return true;
	}
	
	///<summary>If any file has changed ask whether to save it.
	/// Returns true if all changed files are saved or the user doesn't want to save them.</summary>
	private bool AskForSaveIfFilesChanged()
	{
		ArrayList list=new ArrayList();
	
		// create a list with the changed files
		int i=0;
		foreach (DataViewDisplay dv in dataBook.Children) {
			ByteBuffer bb=dv.View.Buffer;
			if (bb.HasChanged)
			list.Add(new SaveFileItem(true, bb.Filename, i));
			i++;
		}
	
		if (list.Count == 0)
			return true;
	
		// special handling if only one file changed
		if (list.Count == 1) {
			int page=((SaveFileItem)list[0]).Page;
			// make the page active 
			dataBook.Page=page;
		
			DataView dv=((DataViewDisplay)dataBook.GetNthPage(page)).View;
			return AskForSaveIfFileChanged(dv);
		}
	
		// show the confirmation alert 
		SaveFileItem[] array=(SaveFileItem[])list.ToArray(typeof(SaveFileItem));
	
		SaveConfirmationMultiAlert sca=new SaveConfirmationMultiAlert(array, mainWindow);
		ResponseType res=(ResponseType)sca.Run();
		sca.Destroy();
	
		// handle responses	
		if (res == ResponseType.Ok) {
			bool allSaved=true;
			
			// save the files the user specified
			foreach(SaveFileItem item in array) {
				if (item.Save==true) {
					DataView dv=((DataViewDisplay)dataBook.GetNthPage(item.Page)).View;
					// make page active
					dataBook.Page=item.Page;
					// try to save the file
					if (SaveFile(dv, null, false, true)==false)
						allSaved=false;
					else {
						//UpdateTabLabel(dv);
						//UpdateWindowTitle(dv);	
					}	
				}
			} 
	
			return allSaved;
		}
		else if (res==ResponseType.No){
			return true;
		}
		else /*if (res==ResponseType.Cancel)*/
			return false;
	}
	
	
	///<summary>
	/// Create new file  
	///</summary>
	public ByteBuffer NewFile()
	{
		ByteBuffer bb = new ByteBuffer();
		bb.UseGLibIdle = true;
		
		if (Preferences.Instance["ByteBuffer.TempDir"] != "") 
			bb.TempDir = Preferences.Instance["ByteBuffer.TempDir"];
			
		return bb;
	}
	
	///<summary>
	/// Try to open the file as a ByteBuffer 
	///</summary>
	public ByteBuffer OpenFile(string filename)
	{
		Uri uri;
		
		// handle normal paths, as well as uris
		try {
			// first try filename as a uri
			uri=new Uri(filename);
		}
		catch(Exception e) {
			try {
				// filename is not a uri...
				// try to expand it as a local path
				uri=new Uri(Path.Combine("file://", Path.GetFullPath(filename)));
			}
			catch(Exception ex) {
				string msg = string.Format(Catalog.GetString("Error opening file '{0}'"), filename);
				ErrorAlert ea=new ErrorAlert(msg, ex.Message, mainWindow);
				ea.Run();
				ea.Destroy();
				return null;
			}
		}
		
		// get an unescaped representation of the uri
		string fullPath=uri.LocalPath;
		
		try {	
			ByteBuffer bb=ByteBuffer.FromFile(fullPath);
			bb.UseGLibIdle=true;
			if (Preferences.Instance["ByteBuffer.TempDir"] != "") 
				bb.TempDir = Preferences.Instance["ByteBuffer.TempDir"];
			string msg = string.Format(Catalog.GetString("Loaded file '{0}'"), fullPath);
			Services.Info.DisplayMessage(msg);
			//history.Add(fullPath);
			return bb;
		}
		catch(UnauthorizedAccessException ex) {
			string msg = string.Format(Catalog.GetString("Error opening file '{0}'"), fullPath);
			ErrorAlert ea=new ErrorAlert(msg, Catalog.GetString("You do not have read permissions for the file you requested."), mainWindow);
			ea.Run();
			ea.Destroy();
		}
		catch(System.IO.FileNotFoundException ex) {
			string msg = string.Format(Catalog.GetString("Error opening file '{0}'"), fullPath);
			ErrorAlert ea=new ErrorAlert(msg, Catalog.GetString("The file you requested does not exist."), mainWindow);
			ea.Run();
			ea.Destroy();
		}
		catch(System.IO.IOException ex) {
			string msg = string.Format(Catalog.GetString("Error opening file '{0}'"), fullPath);
			ErrorAlert ea=new ErrorAlert(msg, ex.Message, mainWindow);
			ea.Run();
			ea.Destroy();
		}
		catch(System.ArgumentException ex) {
			string msg = string.Format(Catalog.GetString("Error opening file '{0}'"), fullPath);
			ErrorAlert ea=new ErrorAlert(msg, ex.Message, mainWindow);
			ea.Run();
			ea.Destroy();
		}
		
		return null;
	}
	
	///<summary>
	/// Manage high-level file saving procedures (eg confirm file overwrites, get filenames etc) 
	///</summary>
	public bool SaveFile(DataView dv, string filename, bool forceSaveAs, bool synchronous)
	{
		ByteBuffer bb=dv.Buffer;
	
	 	if (!dv.Buffer.FileOperationsAllowed) {
	 		return false;
	 	}
	 	
		// if a filename is given, save the file under the specified filename
		if (!forceSaveAs && filename!=null) 
			return SaveFileInternal(dv, filename, synchronous);
	  
		// if a filename is not given but buffer
		// has a file associated with it, save it under the same filename
		if (!forceSaveAs && filename==null && bb.HasFile==true && bb.Filename.Length!=0)
			return SaveFileInternal(dv, bb.Filename, synchronous);	  
	  
		// otherwise prompt user for a name	  
		Gtk.FileChooserDialog fs=new Gtk.FileChooserDialog(Catalog.GetString("Save File As"), mainWindow, FileChooserAction.Save,
			Gtk.Stock.Cancel, ResponseType.Cancel,
			Gtk.Stock.Save, ResponseType.Accept);
		
		bool done=false;
		bool fileSaved=true;
	
		do {
			ResponseType response = (ResponseType)fs.Run();
			fs.Hide();
			if (response == ResponseType.Accept) {
				// check to see whether file exists and prompt user to confirm
				if (File.Exists(fs.Filename)) {
					FileOverwriteAlert ea=new FileOverwriteAlert(fs.Filename, mainWindow);
					ResponseType response1 = (ResponseType)ea.Run();
					ea.Destroy();
					if (response1 == ResponseType.Ok) {
						fileSaved=SaveFileInternal(dv, fs.Filename, synchronous);
						done=true;
					}
					else
						done=false;
				}
				else{ // !File.Exists(fs.Filename)
					fileSaved=SaveFileInternal(dv, fs.Filename, synchronous);
					done=true;
				}
			}
			else { // response!=ResponseType.OK
				done=true;
				fileSaved=false;
			}
		} while(!done);
	
		fs.Destroy();	
		
		//UpdateRevert(dv);
		
		return fileSaved; 
	}
	
	
	///<summary>
	/// Manage low-level file saving procedures 
	///</summary>
	private bool SaveFileInternal(DataView dv, string filename, bool synchronous)
	{
		ByteBuffer bb=dv.Buffer;
	
		string fullPath=null;
	
		// get the full path
		try {
			fullPath=Path.GetFullPath(filename);	
		}
		catch(Exception ex) {
			string msg = string.Format(Catalog.GetString("Error saving file '{0}'"), filename);
			ErrorAlert ea=new ErrorAlert(msg, ex.Message, mainWindow);
			ea.Run();
			ea.Destroy();
		}
	
		// if we can't get full path, return
		if (fullPath==null)
			return false;
		
		try {
			string msg;
			if (fullPath!=bb.Filename)
				msg	= string.Format(Catalog.GetString("Saving file '{0}' as '{1}'"), bb.Filename, fullPath);
			else
				msg = string.Format(Catalog.GetString("Saving file '{0}'"), bb.Filename);
						
			Services.Info.DisplayMessage(msg + "...");

			
			// create a ProgressDialog
			// if save is synchronous, make it modal
			ProgressDialog pd=new ProgressDialog(msg, mainWindow);
			pd.Modal=synchronous;
			
			IAsyncResult ar;
			
			// Decide whether to save or save as
			if (fullPath!=bb.Filename)
				ar=bb.BeginSaveAs(fullPath, new ProgressCallback(pd.Update), new AsyncCallback(SaveFileAsyncCallback));
			else
				ar=bb.BeginSave(new ProgressCallback(pd.Update), new AsyncCallback(SaveFileAsyncCallback));
				
			// if save is synchronous wait for save to finish
			if (synchronous) {
				// while waiting update the gui
				while (ar.AsyncWaitHandle.WaitOne(50, true)==false) {
					while( Application.EventsPending() ) {
      					Application.RunIteration();
      				}
				}
				// find out if save succeeded
				SaveOperation bbs=(SaveOperation)ar.AsyncState;
				if (bbs.Result!=SaveOperation.OperationResult.Finished)
					return false;
				// add to history
				//history.Add(bbs.SavePath);
				return true;
			}
		}
		catch(IOException ex) {
			string file;
			if (fullPath!=bb.Filename)
				file=fullPath;
			else
				file=bb.Filename;
			string msg = string.Format(Catalog.GetString("Error saving file '{0}'"), file);	
			ErrorAlert ea=new ErrorAlert(msg, ex.Message, mainWindow);
			ea.Run();
			ea.Destroy();
			
			msg = string.Format(Catalog.GetString("The file '{0}' has NOT been saved"), file);
			Services.Info.DisplayMessage(msg);
		}
		
		return false;
	}
	
	///<summary>
	/// Callback to call when a save operation has finished
	///</summary>
	void SaveFileAsyncCallback(IAsyncResult ar)
	{
		SaveAsOperation bbs=(SaveAsOperation)ar.AsyncState;
		
		DataViewDisplay dvDisplay=null;
		
		// find out to which dataview the saved buffer belongs
		foreach (DataViewDisplay dd in dataBook.Children) {
			if (dd.View.Buffer==bbs.Buffer) {
				dvDisplay=dd;
				break;
			}
		}
		
		if (bbs.Result==SaveAsOperation.OperationResult.Finished) { // save went ok
			string msg;
			if (bbs.SavePath!=bbs.Buffer.Filename)
				msg = string.Format(Catalog.GetString("The file has been saved as '{0}'"), bbs.SavePath);
			else
				msg = string.Format(Catalog.GetString("The file '{0}' has been saved"), bbs.SavePath);
			
			Services.Info.DisplayMessage(msg);
			// add to history
			//history.Add(bbs.SavePath);
			
			return;
		}
		else if (bbs.Result==SaveAsOperation.OperationResult.Cancelled) { // save cancelled
		
		}
		else if (bbs.Result==SaveAsOperation.OperationResult.CaughtException) {
			// * UnauthorizedAccessException
			// * System.ArgumentException
			// * System.IO.IOException
			string msg = string.Format(Catalog.GetString("Error saving file '{0}'"), bbs.SavePath);	
			ErrorAlert ea=new ErrorAlert(msg, bbs.ThreadException.Message, mainWindow);
			ea.Run();
			ea.Destroy();
		}
		
		{
		string msg = string.Format(Catalog.GetString("The file '{0}' has NOT been saved"), bbs.SavePath);
		Services.Info.DisplayMessage(msg);
		}
	}
	
	public void CloseFile(DataView dv)
	{
		if (!dv.Buffer.FileOperationsAllowed)
	 		return;
	 	
		// if user decides not to close after all
		if (AskForSaveIfFileChanged(dv)==false)
			return;
	
		dv.Buffer.CloseFile();
		dataBook.RemoveView(dv);
	
		// promptly free resources memory (eg pixmaps)
		dv.Cleanup();
		dv=null;
		
		// Make sure the new current DataView has the focus
		DataViewDisplay dvd=(DataViewDisplay)dataBook.CurrentPageWidget;
		if (dvd!=null)
			dvd.GrabKeyboardFocus();
		else { 
			// there is no dataview left,
			// update title and statusbars
			mainWindow.Title="Bless - Gtk# Hex Editor";
			Services.Info.ClearMessage();
		}
	}
	
	///<summary>
	/// Try to quit. Ask user to save files before quitting and
	/// save the current session.
	///</summary>
	public void TryQuit()
	{
	 	if (AskForSaveIfFilesChanged()) {
	 		string blessConfDir=Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "bless");
			try {
				Services.Session.Save(Path.Combine(blessConfDir,"last.session"));
				SaveHistory(Path.Combine(blessConfDir,"history.xml"));
			}
			catch (Exception ex) { }
			Application.Quit ();
		}
	}
	
	///<summary>
	/// Load the files specified
	///</summary>
	public void LoadFiles(string[] files)
	{
		// should we replace the current page or create a new one?
		bool replaceCurrentPage=dataBook.CanReplacePage(dataBook.CurrentPage);
		
		foreach(string file in files) {
			// try to open the file
			ByteBuffer bb=OpenFile(file);

			// if open was successful
			if (bb!=null) {
				if (replaceCurrentPage) { // replace current page
					DataView dv=((DataViewDisplay)dataBook.CurrentPageWidget).View;
					dv.Buffer.CloseFile();
					dv.Buffer=bb;
					replaceCurrentPage=false;
				}
				else { // create new page
					// create and setup a  DataView
					DataView dv=CreateDataView(bb);	
					dataBook.AppendView(dv, new CloseViewDelegate(CloseFile), Path.GetFileName(bb.Filename));	
				}
				
			}
		}  
	}
	
	///<summary>
	/// Save the history to the specified file
	///</summary>
	public void SaveHistory(string path)
	{
		try {
			history.Save(path);
		}
		catch (Exception ex) {
		
		}
	}
	
	///<summary>
	/// Load the history from the specified file
	///</summary>
	public void LoadHistory(string path)
	{
		try {
			history.Load(path);
		}
		catch (Exception ex) {
		
		}
	}
}
	
	
	
}
 
