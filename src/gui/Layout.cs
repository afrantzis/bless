// Configured on 6/26/2004 at 1:42 PM
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
using System.Collections;
using System;
using System.Xml;
using System.IO;
using Gtk;
using Bless.Gui.Areas;
using Bless.Gui.Drawers;
using Bless.Buffers;

namespace Bless.Gui {

///<summary>
/// A class that keeps information about how the data will be
/// placed and shown on screen.
///</summary>
public class Layout {

	ArrayList areas;
	XmlDocument layoutDoc;
	Drawer font;
	string filePath;
	DateTime timeStamp;
	
	public ArrayList Areas {
		get {return areas;}
	}
	
	public string FilePath {
		get { return filePath; }
	}
	
	public DateTime TimeStamp {
		get { return timeStamp; }
	}
	
	public Layout() 
	{
		areas=new ArrayList();
		layoutDoc=new XmlDocument();
		filePath=null;
	}
	
	public Layout(string name): this() 
	{
		Load(name);
	}

	///<summary>Loads the layout from a file containing xml</summary>
	public void Load(string name) 
	{
		layoutDoc.Load(name);
		Configure();
		filePath=name;
		timeStamp=File.GetLastWriteTime(name);
	}
	
	///<summary>Loads the layout from string containing xml</summary>
	public void LoadXml(string xml)
	{
		layoutDoc.LoadXml(xml);
	}
	
	///<summary>
	/// Create and configure the areas, as defined in the layout file.
	///</summary>
	void Configure()
	{
		XmlNodeList areaList = layoutDoc.GetElementsByTagName("area");
		
		foreach(XmlNode areaNode in areaList) {
			XmlAttributeCollection attrColl =  areaNode.Attributes;
			string type=attrColl["type"].Value;
			
			Area area=Area.Factory(type);
			if (area==null)
				continue;
			
			areas.Add(area);
			
			switch(type) {
				case "hexadecimal":
					ConfigureHex(areaNode, (area as HexArea));
					break;
				case "decimal":
					ConfigureDecimal(areaNode, (area as DecimalArea));
					break;
				case "octal":
					ConfigureOctal(areaNode, (area as OctalArea));
					break;
				case "binary":
					ConfigureBinary(areaNode, (area as BinaryArea));
					break;
				case "ascii":
					ConfigureAscii(areaNode, (area as AsciiArea));
					break;
				case "separator":
					ConfigureSeparator(areaNode, (area as SeparatorArea));
					break;
				case "offset":
					ConfigureOffset(areaNode, (area as OffsetArea));
					break;					
				default:
					break;	
			}
		
		} // end foreach
		
		// give the focus to the first applicable area
		foreach(Area a in areas) {
			if (a.Type!="offset" && a.Type !="separator") {
				a.HasCursorFocus=true;
				break;
			}
		}
		
		// reset cursor
		foreach(Area a in areas) {
			a.CursorOffset=0;
			a.CursorDigit=0;		
		}
	
	}
	
	///<summary>
	/// Realizes the areas. If sharedLayout is not null, use its 
	/// resources instead of creating new ones.
	///</summary>
	public void Realize(Gtk.DrawingArea da)
	{	
		foreach(Area area in areas) {
			Drawer d=null;
			
			switch(area.Type) {
				case "hexadecimal":
					d=new HexDrawer(da, area.DrawerInformation);
					break;
				case "decimal":
					d=new DecimalDrawer(da, area.DrawerInformation);
					break;
				case "octal":
					d=new OctalDrawer(da, area.DrawerInformation);
					break;
				case "binary":
					d=new BinaryDrawer(da, area.DrawerInformation);
					break;
				case "ascii":
					d=new AsciiDrawer(da, area.DrawerInformation);
					break;
				case "separator":
					d=new DummyDrawer(da, area.DrawerInformation);
					break;
				case "offset":
					d=new HexDrawer(da, area.DrawerInformation);
					break;					
				default:
					break;	
			}
			
			if (d!=null)
				area.Realize(da, d); 
		} // end foreach
		
	}
	
	///<summary>Configures a grouped area with the properties in the node</summary>
	void ConfigureGrouped(XmlNode parentNode, GroupedArea area) 
	{
		XmlNodeList childNodes=parentNode.ChildNodes;
		foreach(XmlNode node in childNodes) {
			if (node.Name=="bpr")
				area.FixedBytesPerRow=Convert.ToInt32(node.InnerText);
			if (node.Name=="grouping")
				area.Grouping=Convert.ToInt32(node.InnerText);
		}							
	}
	
	///<summary>Configures a hex area with the properties in the node</summary>
	void ConfigureHex(XmlNode parentNode, HexArea area) 
	{
		ConfigureGrouped(parentNode, area);
		
		Drawer.Information info=new Drawer.Information();
		
		XmlNodeList childNodes=parentNode.ChildNodes;
		foreach(XmlNode node in childNodes) {
			if (node.Name=="case")
				info.Uppercase=(node.InnerText=="upper");	
			if (node.Name=="display")
				ParseDisplay(node, info);
		}							
		
		area.DrawerInformation=info;
	}

	///<summary>Configure a decimal area with the properties in the node</summary>	
	void ConfigureDecimal(XmlNode parentNode, DecimalArea area)
	{
		ConfigureGrouped(parentNode, area);
		
		Drawer.Information info=new Drawer.Information();
		
		XmlNodeList childNodes=parentNode.ChildNodes;
		foreach(XmlNode node in childNodes) {
			if (node.Name=="display")
				ParseDisplay(node, info);
		}
		
		area.DrawerInformation=info;
	}

	///<summary>Configure a octal area with the properties in the node</summary>		
	void ConfigureOctal(XmlNode parentNode, OctalArea area)
	{
		ConfigureGrouped(parentNode, area);
		
		Drawer.Information info=new Drawer.Information();
		
		XmlNodeList childNodes=parentNode.ChildNodes;
		foreach(XmlNode node in childNodes) {
			if (node.Name=="display")
				ParseDisplay(node, info);
		}
		
		area.DrawerInformation=info;
	}
	
	///<summary>Configure a binary area with the properties in the node</summary>
	void ConfigureBinary(XmlNode parentNode, BinaryArea area)
	{
		ConfigureGrouped(parentNode, area);
		
		Drawer.Information info=new Drawer.Information();
		
		XmlNodeList childNodes=parentNode.ChildNodes;
		foreach(XmlNode node in childNodes) {
			if (node.Name=="display")
				ParseDisplay(node, info);
		}
		
		area.DrawerInformation=info;
	}
	
	///<summary>Configure an ascii area with the properties in the node</summary>
	void ConfigureAscii(XmlNode parentNode, AsciiArea area)
	{
		Drawer.Information info=new Drawer.Information();

		XmlNodeList childNodes=parentNode.ChildNodes;
		foreach(XmlNode node in childNodes) {
			if (node.Name=="bpr")
				area.FixedBytesPerRow=Convert.ToInt32(node.InnerText);
			if (node.Name=="display")
				ParseDisplay(node, info);
		}
		
		area.DrawerInformation=info;
	}
	
	///<summary>Configure a separator area with the properties in the node</summary>
	void ConfigureSeparator(XmlNode parentNode, SeparatorArea area)
	{
		Drawer.Information info=new Drawer.Information();
		
		XmlNodeList childNodes=parentNode.ChildNodes;
		foreach(XmlNode node in childNodes) {
			if (node.Name=="display")
				ParseDisplay(node, info);
		}
		
		area.DrawerInformation=info;
	}
	
	///<summary>Configure an offset area with the properties in the node</summary>
	void ConfigureOffset(XmlNode parentNode, OffsetArea area)
	{
		Drawer.Information info=new Drawer.Information();
		
		XmlNodeList childNodes=parentNode.ChildNodes;
		foreach(XmlNode node in childNodes) {
			if (node.Name=="case")
				info.Uppercase=(node.InnerText=="upper");
			if (node.Name=="display")
				ParseDisplay(node, info);
			if (node.Name=="bytes")
				area.Bytes=Convert.ToInt32(node.InnerText);
		}
		
		area.DrawerInformation=info;
	}
	///<summary>Parse the <display> tag in layout files</summary>
	void ParseDisplay(XmlNode parentNode, Drawer.Information info) 
	{
		XmlNodeList childNodes=parentNode.ChildNodes;
		foreach(XmlNode node in childNodes) {
			if (node.Name=="evenrow")
				ParseDisplayRow(node, info, Drawer.RowType.Even);
			else if (node.Name=="oddrow")
				ParseDisplayRow(node, info, Drawer.RowType.Odd);
			else if (node.Name=="font")
				info.FontName=node.InnerText;
		}		
	}
	
	void ParseDisplayRow(XmlNode parentNode, Drawer.Information info, Drawer.RowType rowType)
	{
		Gdk.Color fg, bg;
		XmlNodeList childNodes=parentNode.ChildNodes;
		foreach(XmlNode node in childNodes) {
			ParseDisplayType(node, out fg, out bg);
			
			if (node.Name=="evencolumn") {	
				if (!bg.Equal(Gdk.Color.Zero))
					info.bgNormal[(int)rowType, (int)Drawer.ColumnType.Even]=bg;
				if (!fg.Equal(Gdk.Color.Zero))
					info.fgNormal[(int)rowType, (int)Drawer.ColumnType.Even]=fg;
			}
			else if (node.Name=="oddcolumn") {
				if (!bg.Equal(Gdk.Color.Zero))
					info.bgNormal[(int)rowType, (int)Drawer.ColumnType.Odd]=bg;
				if (!fg.Equal(Gdk.Color.Zero))
					info.fgNormal[(int)rowType, (int)Drawer.ColumnType.Odd]=fg;
			}
			else if (node.Name=="selectedcolumn") {
				if (!bg.Equal(Gdk.Color.Zero))
					info.bgHighlight[(int)rowType, (int)Drawer.HighlightType.Selection]=bg;
				if (!fg.Equal(Gdk.Color.Zero))
					info.fgHighlight[(int)rowType, (int)Drawer.HighlightType.Selection]=fg;
			}
			else if (node.Name=="patternmatchcolumn") {
				if (!bg.Equal(Gdk.Color.Zero))
					info.bgHighlight[(int)rowType, (int)Drawer.HighlightType.PatternMatch]=bg;
				if (!fg.Equal(Gdk.Color.Zero))
					info.fgHighlight[(int)rowType, (int)Drawer.HighlightType.PatternMatch]=fg;
			}
		}
	}
	
	///<summary>Parse a font type</summary>
	void ParseDisplayType(XmlNode parentNode, out Gdk.Color fg, out Gdk.Color bg)
	{
		fg=Gdk.Color.Zero;
		bg=Gdk.Color.Zero;
		XmlNodeList childNodes=parentNode.ChildNodes;
		foreach(XmlNode node in childNodes) {
			if (node.Name=="foreground")
				Gdk.Color.Parse(node.InnerText, ref fg);
			if (node.Name=="background")
				Gdk.Color.Parse(node.InnerText, ref bg);
		}
	}
	
	///<summary>Dispose the pixmap resources used by the layout</summary>
	public void DisposePixmaps()
	{
		foreach(Area a in areas) {
			a.DisposePixmaps();
		}
	}
}

}//namespace
