// created on 1/18/2005 at 1:17 AM
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
using Bless.Gui;
using Bless.Util;

namespace Bless.Gui.Dialogs {

public class GotoOffsetDialog :  Gtk.Window
{
	[Glade.Widget]
	Gtk.Entry OffsetEntry;
	[Glade.Widget]
	Gtk.VBox GotoOffsetVBox;
	[Glade.Widget]
	Gtk.Button GotoOffsetButton;
	[Glade.Widget]
	Gtk.Button CloseButton;
	
	DataBook dataBook;
	
	public GotoOffsetDialog(DataBook db, Gtk.Window main): base("Go to Offset")
	{
		Glade.XML gxml = new Glade.XML (FileResourcePath.GetSystemPath("..","data","bless.glade"), "GotoOffsetVBox", null);		gxml.Autoconnect (this);
	
		dataBook=db;
		
		// setup window
		this.SkipTaskbarHint=false;
		this.TransientFor=main;
		this.TypeHint=Gdk.WindowTypeHint.Dialog;
		this.Default=GotoOffsetButton;
				
		GotoOffsetButton.Sensitive=false;
		

		this.Add(GotoOffsetVBox);
		this.Hide();
	}
	
	void OnOffsetEntryChanged(object o, EventArgs args)
	{
		if (OffsetEntry.Text.Length > 0)
			GotoOffsetButton.Sensitive=true;
		else
			GotoOffsetButton.Sensitive=false;
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
				ErrorAlert ea=new ErrorAlert("Invalid Offset", "The offset you specified is outside the file's limits.", this);
				ea.Run();
				ea.Destroy();
			}
		}
		catch(FormatException e) {
			ErrorAlert ea=new ErrorAlert("Error in Offset Format", e.Message, this);
			ea.Run();
			ea.Destroy();
		}
		
		
	}
	
	protected override bool OnKeyPressEvent(Gdk.EventKey e)
	{
		if (e.Key==Gdk.Key.Escape) {
			this.Hide();
			return true;
		}
		else
			return base.OnKeyPressEvent(e);
	}
	
	void OnCloseClicked(object o, EventArgs args)
	{
		this.Hide();
	}

	protected override bool OnDeleteEvent(Gdk.Event e)
	{
		this.Hide();
		return true;
	}	
}

}// end namespace
