// created on 2/12/2005 at 12:33 PM
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
  
public class ReplaceDialog :  Gtk.Window
{
	IFinder finder;
	
	bool searchPatternChanged;
	bool replacePatternChanged;
	
	byte[] replacePattern;
	
	
	[Glade.Widget] Gtk.VBox ReplaceVBox;
	[Glade.Widget] Gtk.Button FindButton;
	[Glade.Widget] Gtk.Button ReplaceButton;
	[Glade.Widget] Gtk.Button ReplaceAllButton;
	
	[Glade.Widget] Gtk.Entry SearchPatternEntry;
	[Glade.Widget] Gtk.Entry SearchInterpretEntry;
	[Glade.Widget] Gtk.Entry ReplacePatternEntry;
	[Glade.Widget] Gtk.Entry ReplaceInterpretEntry;
	
	
	public ReplaceDialog(IFinder finder, Gtk.Window main): base("Replace")
	{
		Glade.XML gxml = new Glade.XML (FileResourcePath.GetSystemPath("..","data","bless.glade"), "ReplaceVBox", null);
		gxml.Autoconnect (this);
		
		// setup window
		this.SkipTaskbarHint=true;
		this.TypeHint=Gdk.WindowTypeHint.Dialog;
		this.TransientFor=main;
		this.Default=ReplaceButton;
		
		this.Shown+=OnDialogShown;
		
		this.finder=finder;
		
		FindButton.Sensitive=false;
		ReplaceButton.Sensitive=false;
		ReplaceAllButton.Sensitive=false;
		
		replacePattern=new byte[0];
		
		this.Add(ReplaceVBox);
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
							SearchInterpretEntry.Text="Text";
							SearchPatternEntry.Text=result;
							decodingOk=true;
						}
						break;				
					case "decimal":
						SearchInterpretEntry.Text="Decimal";
						SearchPatternEntry.Text=ByteArray.ToString(ba, 10);
						decodingOk=true;
						break;
					case "octal":
						SearchInterpretEntry.Text="Octal";
						SearchPatternEntry.Text=ByteArray.ToString(ba, 8);
						decodingOk=true;
						break;
					case "binary":
						SearchInterpretEntry.Text="Binary";
						SearchPatternEntry.Text=ByteArray.ToString(ba, 2);
						decodingOk=true;
						break;
				}// end switch
			}
			
			if (!decodingOk) {
				SearchInterpretEntry.Text="Hexadecimal";
				SearchPatternEntry.Text=ByteArray.ToString(ba, 16);
			}
			
			finder.LastFound=sel;
		}
	}
	
	//
	// Search related methods
	//
	
	///<summary>Update the search pattern from the text entry</summary>
	void UpdateSearchPattern()
	{
		if (!searchPatternChanged)
			return;
			
		try {
			byte[] ba;
			switch(SearchInterpretEntry.Text) {
				case "Hexadecimal":
					ba=ByteArray.FromString(SearchPatternEntry.Text, 16);
					break;
				case "Decimal":
					ba=ByteArray.FromString(SearchPatternEntry.Text, 10);
					break;
				case "Octal":
					ba=ByteArray.FromString(SearchPatternEntry.Text, 8);
					break;
				case "Binary":
					ba=ByteArray.FromString(SearchPatternEntry.Text, 2);
					break;
				case "Text":
					ba=Encoding.ASCII.GetBytes(SearchPatternEntry.Text);
					break;
				default:
					ba=new byte[0];
					break;
			} //end switch
			
			// if there is something in the text entry but nothing is parsed
			// it means it is full of spaces
			if (ba.Length==0 && SearchPatternEntry.Text.Length!=0)
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
		searchPatternChanged=false;
	}
	
	void OnSearchPatternEntryChanged(object o, EventArgs args)
	{
		string pat=SearchPatternEntry.Text;
		if (pat.Length==0) {
			FindButton.Sensitive=false;
			ReplaceButton.Sensitive=false;
			ReplaceAllButton.Sensitive=false;
		}
		else {
			FindButton.Sensitive=true;
			ReplaceButton.Sensitive=true;
			ReplaceAllButton.Sensitive=true;
		}
		searchPatternChanged=true;
	}
	
	void OnSearchInterpretEntryChanged(object o, EventArgs args)
	{
		searchPatternChanged=true;
	}
	
	void OnFindClicked(object o, EventArgs args)
	{
		try {
			UpdateSearchPattern();
				 
			finder.FindNext(FindAsyncCallback);
		}
		catch(FormatException) { }	
	}
	
	void FindAsyncCallback(IAsyncResult ar)
	{
		ThreadedAsyncResult tar=(ThreadedAsyncResult) ar;
		
		FindNextOperation state=(FindNextOperation)tar.AsyncState;
		if (state.Result==FindNextOperation.OperationResult.Finished && state.Match==null) {
			InformationAlert ia=new InformationAlert("The pattern you requested was not found.", "End of file reached.", this);
			ia.Run();
			ia.Destroy();
		}
	}
	
	//
	// Replace related methods
	//
	
	///<summary>Update the replace pattern from the text entry</summary>
	void UpdateReplacePattern()
	{
		if (!replacePatternChanged)
			return;
			
		try {
			switch(ReplaceInterpretEntry.Text) {
				case "Hexadecimal":
					replacePattern=ByteArray.FromString(ReplacePatternEntry.Text, 16);
					break;
				case "Decimal":
					replacePattern=ByteArray.FromString(ReplacePatternEntry.Text, 10);
					break;
				case "Octal":
					replacePattern=ByteArray.FromString(ReplacePatternEntry.Text, 8);
					break;
				case "Binary":
					replacePattern=ByteArray.FromString(ReplacePatternEntry.Text, 2);
					break;
				case "Text":
					replacePattern=Encoding.ASCII.GetBytes(ReplacePatternEntry.Text);
					break;
				default:
					break;
			} //end switch
			
			// if there is something in the text entry but nothing is parsed
			// it means it is full of spaces
			if (replacePattern.Length==0 && ReplacePatternEntry.Text.Length!=0)
				throw new FormatException("Strings representing numbers cannot consist of only whitespace characters. Leave the text entry completely blank for deletion of matched pattern(s).");
		}	
		catch (FormatException e) {
			ErrorAlert ea=new ErrorAlert("Invalid Replace Pattern", e.Message, this);
			ea.Run();
			ea.Destroy();
			throw;
		}
		replacePatternChanged=false;
	}
	
	void OnReplacePatternEntryChanged(object o, EventArgs args)
	{
		replacePatternChanged=true;
	}
	
	void OnReplaceInterpretEntryChanged(object o, EventArgs args)
	{
		replacePatternChanged=true;
	}
	
	
	void OnReplaceClicked(object o, EventArgs args)
	{
		try {
			UpdateSearchPattern();
			UpdateReplacePattern();
			
			finder.Replace(replacePattern);
			finder.FindNext(FindAsyncCallback);
		}
		catch(FormatException) { }	
	}
	
	void OnReplaceAllClicked(object o, EventArgs args)
	{
		try {
			UpdateSearchPattern();
			UpdateReplacePattern();
			
			finder.ReplaceAll(replacePattern, ReplaceAllAsyncCallback);
		}
		catch(FormatException) { }	
	}
	
	void ReplaceAllAsyncCallback(IAsyncResult ar)
	{
		ThreadedAsyncResult tar=(ThreadedAsyncResult) ar;
		
		ReplaceAllOperation state=(ReplaceAllOperation)tar.AsyncState;
		
		if (state.Result==ReplaceAllOperation.OperationResult.Finished) {
			InformationAlert ia=new InformationAlert("Found and replaced "+state.NumReplaced+" occurences.", "", this);
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
		SearchPatternEntry.SelectRegion(0, SearchPatternEntry.Text.Length);
		SearchPatternEntry.GrabFocus();
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
