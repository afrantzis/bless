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
using System.Collections.Generic;
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

	List<Area> areas;
	XmlDocument layoutDoc;
	Drawer font;
	string filePath;
	DateTime timeStamp;
	
	public List<Area> Areas {
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
		areas=new List<Area>();
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
			area.Configure(areaNode);
		}
		
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
	/// Realizes the areas.
	///</summary>
	public void Realize(Gtk.DrawingArea da)
	{	
		foreach(Area a in areas)
			a.Realize(da);		
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
