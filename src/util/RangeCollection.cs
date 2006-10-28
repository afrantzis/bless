// created on 8/25/2005 at 11:21 AM
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
using System.Collections;

namespace Bless.Util {

public class RangeCollection: ArrayList
{

	public Range LastAdded {
		get { return (Range)this[Count-1]; }
	}
	
	public RangeCollection()
	{	
		Add(new Range());
	}

	public void UpdateRange(Range oldRange, Range newRange)
	{
		int index=IndexOf(oldRange);
		RemoveAt(index);
		Insert(index, newRange);
	}
	
	public new void Clear()
	{
		base.Clear();
		Add(new Range());
	}
		
}


} // end namespace
