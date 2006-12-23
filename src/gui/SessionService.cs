// created on 4/30/2006 at 11:57 AM
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
using Bless.Tools;
using Bless.Buffers;
using Bless.Gui.Dialogs;
using System.IO;

namespace Bless.Gui
{

public class SessionService
{
	DataBook dataBook;
	Window mainWindow;
	
	public SessionService(DataBook db, Window mw)
	{
		dataBook=db;
		mainWindow=mw;
	}
	
	///<summary>
	/// Save the current session to the specified file.
	///</summary>
	public void Save(string path)
	{
		Session session=new Session();
		Gdk.Rectangle alloc=mainWindow.Allocation;
		
		// save the window size
		session.WindowHeight=alloc.Height;
		session.WindowWidth=alloc.Width;
		
		// save information about the currently open files
		foreach(DataViewDisplay dvd in dataBook.Children) {
			DataView dv=dvd.View;
			// add to session only if buffer is related to a file
			if (dv.Buffer.HasFile) {
				session.AddFile(dv.Buffer.Filename, dv.Offset, dv.CursorOffset, dv.CursorDigit, dvd.Layout.FilePath, 0);
				if (dv == ((DataViewDisplay)dataBook.CurrentPageWidget).View) {
					session.ActiveFile=dv.Buffer.Filename;
				}
			}	
		}
		session.Save(path);
	}
	
	///<summary>
	/// Load a session from the specified file.
	///</summary>
	public void Load(string path)
	{
		Session session=new Session();
		
		// try to load session file
		try {
			session.Load(path);
		}
		catch(Exception ex) {
			System.Console.WriteLine(ex.Message);
			return;
		}
		
		// set the size of main window
		if (Preferences.Instance["Session.RememberWindowGeometry"]=="True")
			mainWindow.Resize(session.WindowWidth, session.WindowHeight);
		
		// add files to the DataBook
		foreach(SessionFileInfo sfi in session.Files) {
			ByteBuffer bb=Services.File.OpenFile(sfi.Path);
			// if file was opened successfully
			if (bb!=null) {
				DataView dv=Services.File.CreateDataView(bb);
				// try to load layout file
				try {
					dv.Display.Layout=new Bless.Gui.Layout(sfi.Layout);
				}
				catch(Exception ex) {
					ErrorAlert ea=new ErrorAlert("Error loading layout '"+ sfi.Layout +"' for file '"+sfi.Path+"'. Loading default layout.", ex.Message , mainWindow);
					ea.Run();
					ea.Destroy();
				}
				
				long cursorOffset=sfi.CursorOffset;
				
				// sanity check cursor offset and view offset
				if (cursorOffset > bb.Size) 
					cursorOffset = bb.Size;	
				
				
				long offset=sfi.Offset;
				if (offset >= bb.Size)
					offset = 0;
				
				if (Preferences.Instance["Session.RememberCursorPosition"]=="True") {
					dv.MoveCursor(cursorOffset, sfi.CursorDigit);
					dv.Offset=offset;
				}
				
				dataBook.AppendView(dv, new CloseViewDelegate(Services.File.CloseFile), Path.GetFileName(bb.Filename));
				//OnBufferChanged(bb);
			}
		}
		
		foreach(DataViewDisplay dvd in dataBook.Children) {
			DataView dv=dvd.View;
			if (dv.Buffer.Filename == session.ActiveFile) {
				dataBook.CurrentPage=dataBook.PageNum(dvd);
				break;
			}
		}
	}	
}

}