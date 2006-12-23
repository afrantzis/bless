// created on 12/23/2006 at 3:50 PM
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

using System.Collections.Generic;
using Gdk;

namespace Bless.Gui.Drawers {

///<summary>
/// Handles drawer pixmaps in an memory efficient manner
///</summary>
class PixmapManager
{
	static private PixmapManager manager;
	
	static public PixmapManager Instance {
		get { 
			if (manager == null)
				manager = new PixmapManager();
			
			return manager;
		}
	}

	Dictionary<string, Gdk.Pixmap> pixmaps;
	Dictionary<string, int> references;

	private PixmapManager()
	{
		pixmaps = new Dictionary<string, Gdk.Pixmap>();
		references = new Dictionary<string, int>();
	}
	
	///<summary>
	/// Get the id of the pixmap with the specified properties 
	///</summary>
	public string GetPixmapId(System.Type type, Drawer.Information info, Gdk.Color fg, Gdk.Color bg)
	{
		return string.Format("{0}{1}{2}{3}{4}{5}", type, info.FontName, info.FontLanguage, info.Uppercase, fg.ToString(), bg.ToString());
	}
	
	///<summary>
	/// Get the pixmap with the specified id.
	/// Returns null if the pixmap doesn't exist
	///</summary>
	public Gdk.Pixmap GetPixmap(string id)
	{	
		Gdk.Pixmap pix = null;
		if (pixmaps.ContainsKey(id))
		 	pix = pixmaps[id];
		 	
		return pix;
	}
	
	///<summary>
	/// Add the pixmap to the collection 
	///</summary>
	public void AddPixmap(string id, Gdk.Pixmap pix)
	{
		pixmaps[id] = pix;
		references[id] = 0;
	}
	
	///<summary>
	/// Mark that we are using the pixmap  
	///</summary>
	public void ReferencePixmap(string id)
	{
		++references[id];
	}
	
	///<summary>
	/// Mark that we aren't using the pixmap anymore.
	/// If nobody uses it, dispose of it
	///</summary>
	public void DereferencePixmap(string id)
	{
		--references[id];
		if (references[id] <= 0) {
			pixmaps[id].Dispose();
			pixmaps.Remove(id);
			references.Remove(id);
		}
	}
	
}



}