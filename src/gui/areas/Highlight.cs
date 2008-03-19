// created on 19/3/2008 at 6:14 PM
/*
 *   Copyright (c) 2008, Alexandros Frantzis (alf82 [at] freemail [dot] gr)
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
using System.Collections.Generic;
using Bless.Util;
using Bless.Gui.Drawers;

namespace Bless.Gui.Areas
{

internal class Highlight : Util.Range, IEquatable<Highlight>
{
	Drawer.HighlightType type;
	
	public Drawer.HighlightType Type {
		get { return type; }
	}
	
	public Highlight() : base()
	{
		type = Drawer.HighlightType.Normal;
	}
	
	public Highlight(Drawer.HighlightType ht) : base()
	{
		type = ht;	
	}
	
	public Highlight(IRange r, Drawer.HighlightType ht) : base(r)
	{
		type = ht;	
	}
	
	public Highlight(long start, long end, Drawer.HighlightType ht) : base(start, end)
	{
		type = ht;
	}
	
	public bool Equals(Highlight h)
	{
		return this.Start == h.Start && this.End == h.End && this.Type == h.Type; 
	}
	
	public override bool Equals(object obj)
	{
		Highlight h = obj as Highlight;
		if (h != null)
			return Equals(h);
		else
			return false;
	}
	
	public override int GetHashCode()
	{
		return (start.GetHashCode() ^ end.GetHashCode() ^ type.GetHashCode());
	}
	
	public override string ToString()
	{
		return base.ToString() + " Type: " + type;
	}
	
}

} // end namespace