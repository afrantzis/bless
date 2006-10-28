// created on 4/14/2005 at 12:20 PM
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

using System.IO;
using System.Collections;
using System.Security.Cryptography;
using Bless.Buffers;
using Bless.Util;


namespace Bless.Gui {

///<summary>
/// Handles the creation and disposing of layouts
/// in a memory efficient manner.
///</summary
public class LayoutManager
{
	static Hashtable layoutInfo=new Hashtable();
	static object lockObj=new object();
	
	private LayoutManager()
	{
	}
	
	///<summary>
	/// Realizes a layout. If the layout resources are already
	/// loaded, re-use them.
	///</summary>
	static public void RealizeLayout(Layout layout, Gtk.DrawingArea da)
	{
		lock (lockObj) {
		
		if (layout==null)
			return;
			
		// uniquely describe a layout file
		string id=layout.FilePath+layout.TimeStamp.ToString();
			
		LayoutInfo li=(LayoutInfo)layoutInfo[id];
		
		// first time we are using it
		if (li==null) {
			li=new LayoutInfo(layout);
			layoutInfo[id]=li;
			layout.Realize(da, null);
		}
		// else if layout is already loaded
		else if (li!=null) {
			++li.Count;
			layout.Realize(da, li.Layout);
		}
		
		}//lock
	}
	
	///<summary>
	/// Disposes the resources of a layout. If the layout resources are
	/// being used by another layout, just decrement a usage counter.
	///</summary>
	static public void DisposeLayout(Layout layout)
	{
		lock (lockObj) {
		
		if (layout==null)
			return;
			
		// uniquely describe a layout file
		string id=layout.FilePath+layout.TimeStamp.ToString();
		
		LayoutInfo li=(LayoutInfo)layoutInfo[id];
		
		// if layout is loaded
		if (li!=null) {
			--li.Count; // decrease usage count
			if (li.Count==0) { // if it is not being used anymore
				layoutInfo.Remove(id);
				layout.DisposePixmaps();
			}
		}
		else if (li==null) {
			layout.DisposePixmaps();
		}
		
		}//lock
	}	

}

class LayoutInfo
{
	public Layout Layout;
	public int Count;
	public LayoutInfo(Layout l) { Layout=l; Count=1;}
}

} // end namespace
