// created on 6/28/2004 at 12:58 PM
/*
 *   Copyright (c) 2004, Alexandros Frantzis (alf82 [at] freemail [dot] gr)
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
using System.IO;
using Gtk;
using Glade;
using Bless.Gui.Areas;
using Bless.Buffers;
using Bless.Util;
using Mono.Unix;
 
namespace Bless.Gui.Dialogs { 

///<summary>
/// A dialog that lets the user select one of the available layouts
///</summary>
public class LayoutSelectionDialog : Dialog {
	
	DataBook dataBook;
	DataView dataPreview;
	[Glade.Widget] Gtk.TreeView LayoutList;
	[Glade.Widget] Gtk.Frame PreviewFrame;
	[Glade.Widget] Gtk.Paned LayoutSelectionPaned;
	TreeIter selectedLayoutIter;
	string selectedLayout;
	string layoutDir;
	
	public string SelectedLayout {
		get { return selectedLayout; } 
	}
	
	enum LayoutColumn { Filename, Path } 
	
	public LayoutSelectionDialog(DataBook db)
	: base(Catalog.GetString("Select Layout"), null, 0) 
	{
		Glade.XML gxml = new Glade.XML (FileResourcePath.GetSystemPath("..","data","bless.glade"), "LayoutSelectionPaned", "bless");
		gxml.Autoconnect (this);
		
		dataBook=db;
		
		// create the preview area
		dataPreview=new DataView();
		ByteBuffer bb=new ByteBuffer();
		bb.Append(new byte[]{0,1,2,3,4,5,6,7,8,9,10,11,12,13,14,15});
		bb.Append(new byte[]{16,17,18,19,20,21,22,23,24,25,26,27,28,29,30,31});
		dataPreview.Buffer=bb;
		
		PreviewFrame.Add(dataPreview.Display);
		PreviewFrame.ShowAll();
		
		layoutDir=System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "bless");
		layoutDir=System.IO.Path.Combine(layoutDir, "layouts");
		
		// Initialize list
		PopulateLayoutList();
		
		this.DefaultWidth=600;
		this.DefaultHeight=300;
		this.Modal=false;
		this.BorderWidth=6;
		this.HasSeparator=false;
		this.AddButton(Gtk.Stock.Close, ResponseType.Close);
		this.AddButton(Gtk.Stock.Ok, ResponseType.Ok);
		this.Response+=new ResponseHandler(OnDialogResponse);
		this.VBox.Add(LayoutSelectionPaned);
	}
	
	///<summary>Populate the layout list</summary>
	void PopulateLayoutList()
	{
		// specify the column types
		TreeStore ts=new TreeStore(typeof(string), typeof(string));
		
		TreeIter ti=ts.AppendValues(Catalog.GetString("System-wide Layouts"), string.Empty);
		
		// fill list from bless data dir
		string dataDir=FileResourcePath.GetSystemPath("..", "data");
		if (Directory.Exists(dataDir)) {
			string[] files=Directory.GetFiles(dataDir, "*.layout");	
			foreach (string s in files) {
				ts.AppendValues(ti, System.IO.Path.GetFileName(s), s);
			}
		}
		
		ti=ts.AppendValues(Catalog.GetString("User Layouts"), string.Empty);
		
		// fill list from user layout dir
		if (Directory.Exists(layoutDir)) {
			string[] files=Directory.GetFiles(layoutDir, "*.layout");	
			foreach (string s in files) {
				ts.AppendValues(ti, System.IO.Path.GetFileName(s), s);
			}
		}
		
		// Create the treeview
		LayoutList.Model=ts;
		LayoutList.AppendColumn("Layout",new CellRendererText (), "text", (int)LayoutColumn.Filename);
		LayoutList.ExpandAll();
		LayoutList.Selection.Changed +=OnLayoutListSelectionChanged;
	}
	
	///<summary>Handle list selection changed event</summary>
	public void OnLayoutListSelectionChanged (object o, EventArgs args) 
	{
		TreeSelection sel=(TreeSelection)o;
		TreeModel tm;
		TreeIter ti;
		
		if (sel.GetSelected(out tm, out ti)) {
			string val = (string) tm.GetValue (ti, (int)LayoutColumn.Path);
			
			// if there is not path, it means user clicked on a header row
			if (val==string.Empty) {
				if (selectedLayout != null)
					sel.SelectIter(selectedLayoutIter);
				else
					sel.UnselectIter(ti);
				return;
			}
			
			// try to load layout from file
			try {
				dataPreview.Display.Layout=new Layout(val);
				selectedLayoutIter=ti;
				selectedLayout=val;
			}
			catch(System.Xml.XmlException ex) {
				string msg = string.Format(Catalog.GetString("Error parsing layout file '{0}'"), val);
				ErrorAlert ea=new ErrorAlert(msg, ex.Message, this);
				ea.Run();
				ea.Destroy();
				if (selectedLayout != null)
					sel.SelectIter(selectedLayoutIter);
				else
					sel.UnselectIter(ti);
			}
			finally {
				dataPreview.Display.Redraw();
			}
			
		}
	}
	
	void OnDialogResponse(object o, Gtk.ResponseArgs args)
	{
		if (args.ResponseId==ResponseType.Ok && selectedLayout!=null) {
			// get current dataview
			if (dataBook!=null && dataBook.NPages > 0) {
				DataView dv=((DataViewDisplay)dataBook.CurrentPageWidget).View;
				dv.Display.Layout=new Layout(selectedLayout);
			}
		}
		
		// dispose preview Area pixmaps
		dataPreview.Cleanup();
				
		this.Destroy();
	}
}


}//namespace
