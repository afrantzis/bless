// created on 1/10/2005 at 7:02 PM
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
using Bless.Tools.Find;
using Bless.Util;
using Bless.Gui;
using Bless.Gui.Areas;
using Bless.Buffers;
using System.Text;
 
namespace Bless.Gui.Dialogs {
  
public class FindDialog :  Gtk.Window
{
	IFinder finder;
	bool patternChanged;
	
	[Glade.Widget]
	Gtk.Entry PatternEntry;
	[Glade.Widget]
	Gtk.VBox FindVBox;
	[Glade.Widget]
	Gtk.Button FindNextButton;
	[Glade.Widget]
	Gtk.Button FindPreviousButton;
	[Glade.Widget]
	Gtk.Entry InterpretEntry;
	
	public FindDialog(IFinder finder, Gtk.Window main): base("Find")
	{
		Glade.XML gxml = new Glade.XML (FileResourcePath.GetSystemPath("..","data","bless.glade"), "FindVBox", null);
		gxml.Autoconnect (this);
		
		// setup window
		this.SkipTaskbarHint=false;
		this.TypeHint=Gdk.WindowTypeHint.Dialog;
		this.TransientFor=main;
		this.Default=FindNextButton;
		
		this.Shown+=OnDialogShown;
		
		// create finder
		this.finder=finder;
		
		FindNextButton.Sensitive=false;
		FindPreviousButton.Sensitive=false;
		
		this.Add(FindVBox);
		this.Hide();
	}
	
	///<summary>Load the dialog with data from the DataView's selection</summary>
	public void LoadWithSelection(DataView dv)
	{
		ByteBuffer bb=dv.Buffer;
		
		// load selection only if it isn't very large 
		if (dv.Selection.Size <= 1024 && dv.Selection.Size > 0) {
			// make sure selection is sorted
			Bless.Util.Range sel=new Bless.Util.Range(dv.Selection);
			sel.Sort();
			
			byte[] ba=new byte[sel.Size];
			for (int i=0;i<sel.Size;i++) {
				ba[i]=bb[sel.Start+i];
			}
		
			bool decodingOk=false;
			Area focusedArea=dv.FocusedArea;
			
			if (focusedArea!=null) { 
				switch (focusedArea.Type) {
					case "ascii": 
						string result=Encoding.ASCII.GetString(ba);
						// if the byte sequence cannot be displayed correctly 
						// as ascii, eg it contains a zero byte, use hexadecimal
						if (result.IndexOf('\x00')!=-1)
							decodingOk=false;
						else {	
							InterpretEntry.Text="Text";
							PatternEntry.Text=result;
							decodingOk=true;
						}
						break;				
					case "decimal":
						InterpretEntry.Text="Decimal";
						PatternEntry.Text=ByteArray.ToString(ba, 10);
						decodingOk=true;
						break;
					case "octal":
						InterpretEntry.Text="Octal";
						PatternEntry.Text=ByteArray.ToString(ba, 8);
						decodingOk=true;
						break;
					case "binary":
						InterpretEntry.Text="Binary";
						PatternEntry.Text=ByteArray.ToString(ba, 2);
						decodingOk=true;
						break;
				}// end switch
			}
			
			if (!decodingOk) {
				InterpretEntry.Text="Hexadecimal";
				PatternEntry.Text=ByteArray.ToString(ba, 16);
			}
		}
	}
	
	///<summary>Update the search pattern from the text entry</summary>
	void UpdatePattern()
	{
		if (!patternChanged)
			return;
		
		try {
			byte[] ba;
			switch(InterpretEntry.Text) {
				case "Hexadecimal":
					ba=ByteArray.FromString(PatternEntry.Text, 16);
					break;
				case "Decimal":
					ba=ByteArray.FromString(PatternEntry.Text, 10);
					break;
				case "Octal":
					ba=ByteArray.FromString(PatternEntry.Text, 8);
					break;
				case "Binary":
					ba=ByteArray.FromString(PatternEntry.Text, 2);
					break;
				case "Text":
					ba=Encoding.ASCII.GetBytes(PatternEntry.Text);
					break;
				default:
					ba=new byte[0];
					break;
			} //end switch
			
			// if there is something in the text entry but nothing is parsed
			// it means it is full of spaces
			if (ba.Length==0 && PatternEntry.Text.Length!=0)
				throw new FormatException("Strings representing numbers cannot consist of whitespace characters only.");
			else
				finder.Strategy.Pattern=ba;
		}	
		catch (FormatException e) {
			ErrorAlert ea=new ErrorAlert("Invalid Search Pattern", e.Message, this);
			ea.Run();
			ea.Destroy();
			throw;
		}
		patternChanged=false;
	}
	
	void OnPatternEntryChanged(object o, EventArgs args)
	{
		string pat=PatternEntry.Text;
		if (pat.Length==0) {
			FindNextButton.Sensitive=false;
			FindPreviousButton.Sensitive=false;
		}
		else {
			FindNextButton.Sensitive=true;
			FindPreviousButton.Sensitive=true;
		}
		patternChanged=true;
	}
	
	void OnInterpretEntryChanged(object o, EventArgs args)
	{
		patternChanged=true;
	}
	
	void OnFindNextClicked(object o, EventArgs args)
	{
		try {
			UpdatePattern();
			
			finder.FindNext(FindNextAsyncCallback);
		}
		catch(FormatException) { }
	}
	
	void FindNextAsyncCallback(IAsyncResult ar)
	{
		ThreadedAsyncResult tar=(ThreadedAsyncResult) ar;
		
		FindNextOperation state=(FindNextOperation)tar.AsyncState;
		if (state.Result==FindNextOperation.OperationResult.Finished && state.Match==null) {
			InformationAlert ia=new InformationAlert("The pattern you requested was not found.", "End of file reached.", this);
			ia.Run();
			ia.Destroy();
		}
	}
	
	void OnFindPreviousClicked(object o, EventArgs args)
	{
		try {
			UpdatePattern();
			finder.FindPrevious(FindPreviousAsyncCallback);
		}
		catch(FormatException) { }
	}
	
	void FindPreviousAsyncCallback(IAsyncResult ar)
	{
		ThreadedAsyncResult tar=(ThreadedAsyncResult) ar;
		
		FindPreviousOperation state=(FindPreviousOperation)tar.AsyncState;
		if (state.Result==FindPreviousOperation.OperationResult.Finished && state.Match==null) {
			InformationAlert ia=new InformationAlert("The pattern you requested was not found.", "Beginning of file reached.", this);
			ia.Run();
			ia.Destroy();
		}
	}
	
	void OnCloseClicked(object o, EventArgs args)
	{
		this.Hide();
	}
	
	void OnDialogShown(object o, EventArgs args)
	{
		// when the dialog is shown, select and give the focus 
		// to the search pattern entry
		PatternEntry.SelectRegion(0, PatternEntry.Text.Length);
		PatternEntry.GrabFocus();
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
	
	protected override bool OnDeleteEvent(Gdk.Event e)
	{
		this.Hide();
		return true;
	}
}
 
} //end namespace
