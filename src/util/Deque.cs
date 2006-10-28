// created on 7/4/2005 at 12:24 PM
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

namespace Bless.Util {

///<summary>
/// Double-ended queue data structure 
///</summary>
public class Deque
{
	Bless.Util.List list;
	
	public Deque()
	{
		list=new Bless.Util.List();
	}
	
	///<summary>
	/// Add an object to the front of the queue 
	///</summary>
	public void AddFront(object o)
	{
		list.InsertBefore(list.First, o);
	}
	
	public void AddEnd(object o)
	{
		list.InsertAfter(list.Last, o);
	}
	
	///<summary>
	/// Remove an object from the front of the queue (LIFO) 
	///</summary>
	public object RemoveFront()
	{
		object o=null;
		
		if (list.First!=null) {
			o=list.First.data;
			list.First.data=null;
			list.Remove(list.First);
		}
		
		return o;
	}
	
	///<summary>
	/// Remove an object from the end of the queue (FIFO) 
	///</summary>
	public object RemoveEnd()
	{
		object o=null;
		
		if (list.Last!=null) {
			o=list.Last.data;
			list.Last.data=null;
			list.Remove(list.Last);
		}
		
		return o;
	}
	
	///<summary>
	/// Peek the object at the front of the queue 
	///</summary>
	public object PeekFront()
	{
		object o=null;
		
		if (list.First!=null)
			o=list.First.data;
		
		return o;
	}
	
	///<summary>
	/// Peek the object at the end of the queue 
	///</summary>
	public object PeekEnd()
	{
		object o=null;
		
		if (list.Last!=null)
			o=list.Last.data;
		
		return o;
	}
	
	public void Clear()
	{
		while (Count > 0)
			RemoveFront();
	}
	
	public int Count {
		get { return list.Count; }
	}
}


} // end namespace