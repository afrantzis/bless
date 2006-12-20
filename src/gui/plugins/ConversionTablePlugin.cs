// created on 2/18/2005 at 3:24 PM
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
using System.Text;
using Gtk;
using Bless.Util;
using Bless.Tools;
using Bless.Gui;
using Bless.Buffers;
using Bless.Plugins;
using Mono.Unix;

namespace Bless.Gui.Plugins {
	
public class ConversionTablePlugin : GuiPlugin
{	
	DataBook dataBook;
	ConversionTable widget;
	Window mainWindow;
	ToggleAction conversionTableAction;
	UIManager uiManager;
	
	const string uiXml=
	"<menubar>"+
	"	<menu action=\"Tools\">"+
	"		<menuitem name=\"ConversionTable\" action=\"ConversionTableAction\" />"+
	"	</menu>"+
	"</menubar>";
	
	public ConversionTablePlugin(Window mw, UIManager uim)
	{
		mainWindow=mw;
		uiManager=uim;
		
		name="ConversionTable";
		author="Alexandros Frantzis";
		description="Convert";
	}
	
	public override bool Load()
	{
		dataBook=(DataBook)GetDataBook(mainWindow);
		
		widget=new ConversionTable(dataBook);
		
		WidgetGroup wg1=(WidgetGroup)GetWidgetGroup(mainWindow, 1);
		wg1.Add(widget);
		
		AddMenuItems(uiManager);
		
		
		
		Preferences.Proxy.Subscribe("Tools.ConversionTable.Show", "ct1", new PreferencesChangedHandler(OnPreferencesChanged));
		
		loaded=true;
		return true;
	}
	
	
	
	private void AddMenuItems(UIManager uim)
	{
		ToggleActionEntry[] toggleActionEntries = new ToggleActionEntry[] {
			new ToggleActionEntry ("ConversionTableAction", null, Catalog.GetString("Conversion Table"), null, null,
			                    new EventHandler(OnViewConversionTableToggled), false),
		};
		
		ActionGroup group = new ActionGroup ("ConversionTableActions");
		group.Add (toggleActionEntries);
		
		uim.InsertActionGroup(group, 0);
		uim.AddUiFromString(uiXml);
		conversionTableAction=(ToggleAction)uim.GetAction("/menubar/Tools/ConversionTable");
		
		uim.EnsureUpdate();
	}
	
	///<summary>Handle the View->Conversion Table command</summary>
	public void OnViewConversionTableToggled(object o, EventArgs args)
	{
		Preferences.Proxy.Change("Tools.ConversionTable.Show", conversionTableAction.Active.ToString(), "ct1");
	}
	

	void OnPreferencesChanged(Preferences prefs)
	{	
		if (prefs["Tools.ConversionTable.Show"]=="True")
			conversionTableAction.Active=true;
		else
			conversionTableAction.Active=false;
	}
}


///<summary> A widget to convert the data at the current offset to various types</summary>
public class ConversionTable: Gtk.HBox
{

	[Glade.Widget] Gtk.Table ConversionTableWidget;
	
	[Glade.Widget] Gtk.Entry Signed8bitEntry;
	[Glade.Widget] Gtk.Entry Unsigned8bitEntry;
	[Glade.Widget] Gtk.Entry Signed16bitEntry;
	[Glade.Widget] Gtk.Entry Unsigned16bitEntry;
	[Glade.Widget] Gtk.Entry Signed32bitEntry;
	[Glade.Widget] Gtk.Entry Unsigned32bitEntry;
	[Glade.Widget] Gtk.Entry Float32bitEntry;
	[Glade.Widget] Gtk.Entry Float64bitEntry;
	[Glade.Widget] Gtk.Entry HexadecimalEntry;
	[Glade.Widget] Gtk.Entry DecimalEntry;
	[Glade.Widget] Gtk.Entry OctalEntry;
	[Glade.Widget] Gtk.Entry BinaryEntry;
	[Glade.Widget] Gtk.Entry AsciiEntry;
	
	[Glade.Widget] Gtk.CheckButton LittleEndianCheckButton;
	[Glade.Widget] Gtk.CheckButton UnsignedAsHexCheckButton;
	
	DataBook dataBook;
	
	bool littleEndian;
	bool unsignedAsHex;
	
	public ConversionTable(DataBook db)
	{
		Glade.XML gxml = new Glade.XML (FileResourcePath.GetSystemPath("..","data","bless.glade"), "ConversionTableWidget", "bless");
		gxml.Autoconnect (this);
	
		littleEndian=true;
		unsignedAsHex=false;
		dataBook=db;
		
		foreach(DataViewDisplay dvd in dataBook.Children) {
			OnDataViewAdded(dvd.View);
		}
		
		dataBook.PageAdded += new DataView.DataViewEventHandler(OnDataViewAdded);
		dataBook.Removed += new RemovedHandler(OnDataViewRemoved);
		dataBook.SwitchPage += new SwitchPageHandler(OnSwitchPage);
		
		Preferences.Proxy.Subscribe("Tools.ConversionTable.Show", "ct2", new PreferencesChangedHandler(OnPreferencesChanged));

		this.Add(ConversionTableWidget);
		this.ShowAll();
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
		
		Update();
	}
	
	void OnBufferChanged(DataView dv)
	{
		// if changed dataview is not the current one just ignore
		DataViewDisplay dvd=(DataViewDisplay)dataBook.CurrentPageWidget;
		if (dvd==null || dvd.View!=dv)
			return;
		
		Update();
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
		
		DataViewDisplay dvd=(DataViewDisplay)dataBook.CurrentPageWidget;
		if (dvd==null || dvd.View!=dv)
			return;
		
		Update();
	}
	
	void OnSwitchPage(object o, SwitchPageArgs args)
	{	
		Update();
	}
	
	void OnCursorChanged(DataView dv)
	{
		DataView dvcur=((DataViewDisplay)dataBook.CurrentPageWidget).View;
		if (dvcur!=dv)
			return;
		
		Update();
	}
	
	// redefine Hide() method
	// to properly handle hiding
	protected override void  OnHidden()
	{
		// if the focus is in the table
		// give it to the active dataview
		if (IsFocusInTable()) {
			DataViewDisplay dvd=(DataViewDisplay)dataBook.CurrentPageWidget;
			if (dvd!=null)
				dvd.GrabKeyboardFocus();
		}
		
		Console.WriteLine("CT Hide");
		Preferences.Proxy.Change("Tools.ConversionTable.Show", "False", "ct2");
		base.OnHidden();
	}
	
	protected override void OnShown()
	{
		Preferences.Proxy.Change("Tools.ConversionTable.Show", "True", "ct2");
		base.OnShown();
		
		Update();
	}
	
	
	// whether a widget in the table has the focus
	bool IsFocusInTable()
	{
		foreach (Gtk.Widget child in  ConversionTableWidget.Children) {
			Widget realChild=child;
			
			if (child.GetType()==typeof(Gtk.Alignment))
				realChild=(child as Gtk.Alignment).Child;
				
			if (realChild.HasFocus)
				return true;
		}
		
		return false;
	}
	
	void OnLittleEndianToggled(object o, EventArgs args)
	{
		littleEndian=LittleEndianCheckButton.Active;
		
		Preferences.Proxy.Change("Tools.ConversionTable.LEDecoding", littleEndian.ToString(), "ct2");
		
		Update();
	}
	
	void OnUnsignedAsHexToggled(object o, EventArgs args)
	{
		unsignedAsHex=UnsignedAsHexCheckButton.Active;
		
		Preferences.Proxy.Change("Tools.ConversionTable.UnsignedAsHex", unsignedAsHex.ToString(), "ct2");
		
		Update();
	}
	
	void OnPreferencesChanged(Preferences prefs)
	{
		Console.WriteLine("Prefch CT2");
		if (prefs["Tools.ConversionTable.LEDecoding"]=="True")
			LittleEndianCheckButton.Active=true;
		else	
			LittleEndianCheckButton.Active=false;
			
		if (prefs["Tools.ConversionTable.UnsignedAsHex"]=="True")
			UnsignedAsHexCheckButton.Active=true;
		else	
			UnsignedAsHexCheckButton.Active=false;
		
		if (prefs["Tools.ConversionTable.Show"]=="True")
			this.Visible=true;
		else	
			this.Visible=false;
	}
	
	void OnCloseButtonClicked(object o, EventArgs args)
	{
		Preferences.Instance["Tools.ConversionTable.Show"]="False";
	}
	
	void Clear8bit()
	{
		Signed8bitEntry.Text="---";
		Unsigned8bitEntry.Text="---";
	}
	
	void Clear16bit()
	{
		Signed16bitEntry.Text="---";
		Unsigned16bitEntry.Text="---";
	}
	
	void Clear32bit()
	{
		Signed32bitEntry.Text="---";
		Unsigned32bitEntry.Text="---";
	}
	
	void ClearFloat()
	{
		Float32bitEntry.Text="---";
		Float64bitEntry.Text="---";
	}
	
	void ClearBases()
	{
		HexadecimalEntry.Text="---";
		DecimalEntry.Text="---";
		OctalEntry.Text="---";
		BinaryEntry.Text="---";
		AsciiEntry.Text="---";
	}
			
	///<summary>Update the 8bit entries</summary>
	void Update8bit(DataView dv)
	{
		long offset=dv.CursorOffset;
		
		// make sure offset is valid
		if (offset < dv.Buffer.Size && offset >= 0) {
			byte uval=dv.Buffer[offset];
			sbyte val=(sbyte)uval;
			
			// set signed
			Signed8bitEntry.Text=val.ToString();
			
			// set unsigned
			if (unsignedAsHex)
				Unsigned8bitEntry.Text=string.Format("0x{0:x}", uval);
			else
				Unsigned8bitEntry.Text=uval.ToString();
		}
		else {
			Clear8bit();
		}	
	}
	
	///<summary>Update the 16bit entries</summary>
	void Update16bit(DataView dv)
	{
		long offset=dv.CursorOffset;
		
		// make sure offset is valid
		if (offset < dv.Buffer.Size-1 && offset >= 0) {
			short val=0;
			
			// create value according to endianess
			if (littleEndian) {
				val += (short)(dv.Buffer[offset+1]<< 8);
				val += (short)dv.Buffer[offset];
			}
			else {
				val += (short)(dv.Buffer[offset]<< 8);
				val += (short)dv.Buffer[offset+1];
			}
			
			ushort uval=(ushort)val;
			
			// set signed
			Signed16bitEntry.Text=val.ToString();
			
			// set unsigned
			if (unsignedAsHex)
				Unsigned16bitEntry.Text=string.Format("0x{0:x}", uval);
			else
				Unsigned16bitEntry.Text=uval.ToString();
		}
		else {
			Clear16bit();
		}
	}
	
	///<summary>Update the 32bit entries</summary>
	void Update32bit(DataView dv)
	{
		long offset=dv.CursorOffset;
		
		// make sure offset is valid
		if (offset < dv.Buffer.Size-3 && offset >= 0) {
			int val = 0;
			
			// create value according to endianess
			if (littleEndian) {
				val += dv.Buffer[offset+3] << 24;
				val += dv.Buffer[offset+2] << 16;
				val += dv.Buffer[offset+1] << 8;
				val += dv.Buffer[offset];
			}
			else {
				val += dv.Buffer[offset] << 24;
				val += dv.Buffer[offset+1] << 16;
				val += dv.Buffer[offset+2] << 8;
				val += dv.Buffer[offset+3];
			}
				
			uint uval=(uint)val;
			
			// set signed
			Signed32bitEntry.Text=val.ToString();
			
			// set unsigned
			if (unsignedAsHex)
				Unsigned32bitEntry.Text=string.Format("0x{0:x}", uval);
			else
				Unsigned32bitEntry.Text=uval.ToString();
		}
		else {
			Clear32bit();
		}
	}
	
	///<summary>Update the floating point entries</summary>
	void UpdateFloat(DataView dv)
	{
		long offset=dv.CursorOffset;
		
		// make sure offset is valid for 32 bit float (and 64 bit)
		if (offset < dv.Buffer.Size-3 && offset >= 0) {
			// create byte[] with float bytes
			byte[] ba=new byte[8];
			
			// fill byte[] according to endianess
			if (littleEndian)
				for(int i=0; i<4; i++) 
					ba[i]=dv.Buffer[offset+i];
			else
				for(int i=0; i<4; i++) 
					ba[3-i]=dv.Buffer[offset+i];
			
			// set float 32bit	
			float f=BitConverter.ToSingle(ba, 0);	
			Float32bitEntry.Text=f.ToString();
			
			// make sure offset is valid for 64 bit float
			if (offset < dv.Buffer.Size-7) {
				// fill byte[] according to endianess
				if (littleEndian)
					for(int i=4; i<8; i++)
						ba[i]=dv.Buffer[offset+i];
				else
					for(int i=0; i<8; i++)
						ba[7-i]=dv.Buffer[offset+i];
				
				// set float 64bit			
				double d=BitConverter.ToDouble(ba, 0);	
				Float64bitEntry.Text=d.ToString();
			}
			else 
				Float64bitEntry.Text="---";
		}
		else {
			ClearFloat();
		}
		
	}
	
	///<summary>Update the number base entries</summary>
	void UpdateBases(DataView dv)
	{
		long offset=dv.CursorOffset;
		long size;
		
		if (offset < 0 || offset >= dv.Buffer.Size)
			size=0;
		else {
			size=dv.Buffer.Size-offset;
			size=size<4?size:4;
		}
		
		byte[] ba=new byte[(int)size];
		for (int i=0; i<size; i++)
			ba[i]=dv.Buffer[offset+i];
		
		// make sure offset is valid
		if (size>0) {
			HexadecimalEntry.Text=ByteArray.ToString(ba, 16);
			DecimalEntry.Text=ByteArray.ToString(ba, 10);
			OctalEntry.Text=ByteArray.ToString(ba, 8);
			BinaryEntry.Text=ByteArray.ToString(ba, 2);
			AsciiEntry.Text=Encoding.ASCII.GetString(ba);
		}
		else {
			ClearBases();
		}
	}
	
	///<summary>Update all conversion entries</summary>
	public void Update()
	{
		if (!this.Visible)
			return;
		
		DataViewDisplay dvd=(DataViewDisplay)dataBook.CurrentPageWidget;
		if (dvd==null) {
			Clear();
			return;
		}

		DataView dv=dvd.View;

		
		Update8bit(dv);
		Update16bit(dv);
		Update32bit(dv);
		UpdateFloat(dv);
		UpdateBases(dv);
	}
	
	///<summary>Clear all conversion entries</summary>
	public void Clear()
	{
		if (!this.Visible)
			return;
		
		Clear8bit();
		Clear16bit();
		Clear32bit();
		ClearFloat();
		ClearBases();
	}
        
}   

} // end namespace
