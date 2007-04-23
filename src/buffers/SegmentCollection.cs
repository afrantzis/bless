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

namespace Bless.Buffers {

///<summary>A collection of buffer segments</summary>
public class SegmentCollection {

#region private data
	Util.List<Segment> list;
	
	// cache
	Util.List<Segment>.Node cachedNode;
	long cachedMapping;
#endregion
	
#region Properties	
	public Util.List<Segment> List {
		get { return list; }
	}
	
#endregion
	
	public SegmentCollection() {
		list= new Util.List<Segment>();
	}
	
	
	private void InvalidateCache() 
	{
		cachedNode=null;
		cachedMapping=0;
	}
	
	private void SetCache(Util.List<Segment>.Node n, long map) 
	{
		cachedNode=n;
		cachedMapping=map;
	}
	
	/*
	public byte this[int index] {
		
	}*/
	///
	///<summary>Append a segment to the collection</summary>
	///
	public void Append(Segment s) 
	{
		Util.List<Segment>.Node n=list.Last;
		if (n!=null && n.data!=null) {
			Segment ls=(Segment)n.data;
			if ((s.Buffer==ls.Buffer) && (s.Start==ls.End+1)) {
				ls.End=s.End;
				return;
			}
		}
		list.Append(s);
	}

	///<summary>Inserts a segment after a node 
	///and merges them if possible</summary>
	private Util.List<Segment>.Node InsertAfter(Util.List<Segment>.Node n, Segment s) 
	{
		if (n!=null && n.data!=null) {
			Segment ls=(Segment)n.data;
			if ((s.Buffer==ls.Buffer) && (s.Start==ls.End+1)) {
				ls.End=s.End;
				return n;
			}
		}
		return list.InsertAfter(n, s);
	}
	
	///<summary>Inserts a segment before a node 
	///and merges them if possible</summary>
	private Util.List<Segment>.Node InsertBefore(Util.List<Segment>.Node n, Segment s) 
	{
		if (n!=null && n.data!=null) {
			Segment ls=(Segment)n.data;
			if ((s.Buffer==ls.Buffer) && (s.End+1==ls.Start)) {
				ls.Start=s.Start;
				return n;
			}
		}
		return list.InsertBefore(n, s);
	}
	
	///
	///<summary>Find the segment that the given offset is mapped into</summary>
	///
	public Segment FindSegment(long offset, out long OutMapping, out Util.List<Segment>.Node OutNode) 
	{
		OutMapping=0;
		OutNode=null;
		
		// if this is the first search
		// set up things
		if (cachedNode==null) {
			if (list.First==null)
				return null;
			SetCache(list.First, 0);
		}	
		
		Segment s=(Segment)cachedNode.data;
		long curMapping=cachedMapping;	
		Util.List<Segment>.Node curNode=cachedNode;
		
		// is the cached node the one we want?
		if (s.Contains(offset, curMapping)==true) {
			OutMapping=curMapping;
			OutNode=curNode;
			return s;
		}
		
		// search towards the beginning
		if (offset < curMapping ) {
			while (curNode.prev!=null) {
				curNode=curNode.prev;
				s=(Segment)curNode.data;
				curMapping -= s.Size ;
				if (s.Contains(offset, curMapping)==true) {
					SetCache(curNode,curMapping); 
					OutMapping=curMapping;
					OutNode=curNode;
					return s;
				}
			}
		}
		else { // search towards the end
			while (curNode.next!=null) {
				curMapping += s.Size;
				curNode=curNode.next;
				s=(Segment)curNode.data;
				if (s.Contains(offset, curMapping)==true) {
					SetCache(curNode,curMapping);
					OutMapping=curMapping;
					OutNode=curNode;
					return s;
				}
			}
		}
		
		// offset not found but return last accessed node info
		OutMapping=curMapping;
		OutNode=curNode;
		return null;
	}
	
	///
	///<summary>Insert (or Append) a segment collection at a given offset</summary>
	/// 
	public void Insert(SegmentCollection sc, long offset) 
	{
		long mapping;
		Util.List<Segment>.Node node;
		
		Segment s=FindSegment(offset, out mapping, out node);
		
		// offset not found, check if we have to append
		if (s==null) {
			if ( (node==null && offset==0 ) || (node !=null && offset == mapping + (node.data as Segment).Size)) {
				Util.List<Segment> lst = sc.List;
				int N=lst.Count;
				Util.List<Segment>.Node n=lst.First;
				for (int i=0;i<N;i++) {
					Append((Segment)n.data);
					n=n.next;
				}
			}
			return;
		}
		
		if (mapping==offset) { // start of segment?
			// insert data from the current node backwards
			Util.List<Segment> lst = sc.List;
			int N=lst.Count;
			Util.List<Segment>.Node n=lst.Last;
			for (int i=0;i<N;i++) { 
				node = InsertBefore(node, (Segment)n.data);
				n=n.prev;
			}
			// update cache
			SetCache(node, mapping);
		}
		else { //middle of segment	
			Segment s1=s.SplitAt(offset-mapping);
			list.InsertAfter(node, s1);
			
			Util.List<Segment> lst = sc.List;
			int N=lst.Count;
			Util.List<Segment>.Node n=lst.First;
			for (int i=0;i<N;i++) {
				node = InsertAfter(node, (Segment)n.data);
				n=n.next;
			}
			
		}	
	}
	
	///<summary>Delete a range from the collection</summary>
	public SegmentCollection DeleteRange(long pos1, long pos2) 
	{
		long mapping1, mapping2;
		Util.List<Segment>.Node node1;
		Util.List<Segment>.Node node2;
		
		// Find the segments of the end points.
		// Search for the ending point first so that we won't
		// have to invalidate the cache later
		Segment s2=FindSegment(pos2, out mapping2, out node2);
		Segment s1=FindSegment(pos1, out mapping1, out node1);
		
		if (s1==null || s2==null)
			return null;
		
		// ending segment == starting segment 
		// needs special handling
#region Special Handling if node1==node2		
		if (ReferenceEquals(node1, node2)) {
			bool remove_flag=false;
			
			// try to split segment at pos1
			Segment s_f=s1.SplitAt(pos1-mapping1);
			// if we can't, this means that pos1 is 
			// at the beginning of the segment 
			if (s_f==null) {
				s_f=s1;
				remove_flag=true;
			}
			
			// try to split s_f at pos2+1
			// s_l is the ending part of s1 that we 
			// should keep
			Segment s_l=s_f.SplitAt(pos2-pos1+1);
			
			// if we can't split, this means that pos2 is 
			// at the end of the segment,
			// otherwise add s_l after node1 (which contains s1) 
			if (s_l!=null)
				list.InsertAfter(node1,s_l);
			
			// if we should remove s1
			if (remove_flag) {
				// try to set the cache
				if (node1.next!=null)
					SetCache(node1.next, mapping1);
				else if (node1.prev!=null) {
					Segment s=(Segment)node1.prev.data;
					SetCache(node1.prev, mapping1-s.Size);
				}
				else
					InvalidateCache();
					
				list.Remove(node1);
			}
			//else leave the cache set as is (node1, mapping1)
			
			SegmentCollection s_c= new SegmentCollection();
			s_c.Append(s_f);
			
			return s_c;
		}
#endregion		
		// try to split the ending segment	
		Segment sl=s2.SplitAt(pos2-mapping2+1);
		
		// if we can't, this means that pos2 is the 
		// at the end of the ending segment
		if (sl==null)
			sl=s2;	// set the whole segment for removal
		else
			list.InsertAfter(node2, sl); 
			
			
		Util.List<Segment>.Node n=node1.next;
		
		// try to split the starting segment
		Segment sf=s1.SplitAt(pos1-mapping1);
		
		// if we can't, this means that pos1 is 
		// at the beginning of the starting segment 
		if (sf==null) {
			sf=s1;
			// try to set the cache
			if (node1.prev!=null) {
				Segment s=(Segment)node1.prev.data;
				SetCache(node1.prev, mapping1-s.Size);
			}
			else 
				InvalidateCache();
			
			list.Remove(node1); // remove the whole segment
		}
		
		SegmentCollection sc=new SegmentCollection();
		
		// append the first segment 
		sc.Append(sf);
		
		// append to new and remove from old 
		// all segments up to node2
		while(ReferenceEquals(n, node2)==false) {
			sc.Append((Segment)n.data);
			Util.List<Segment>.Node p=n;
			n=n.next;
			// Remove() must be placed after n.next
			// because it sets n.next=null
			list.Remove(p);	
		}
		// append and remove node2
		list.Remove(n);
		sc.Append((Segment)n.data);
		
		return sc;
		
	}
	
	public SegmentCollection GetRange(long pos1, long pos2)
	{
		long mapping1, mapping2;
		Util.List<Segment>.Node node1;
		Util.List<Segment>.Node node2;
		
		// Find the segments of the end points.
		// Search for the ending point first so that we won't
		// have to invalidate the cache later
		Segment s2=FindSegment(pos2, out mapping2, out node2);
		Segment s1=FindSegment(pos1, out mapping1, out node1);
		
		if (s1==null || s2==null)
			return null;
			
		if (ReferenceEquals(node1, node2)) {
			SegmentCollection scTemp = new SegmentCollection();
			Segment seg = new Segment(s1.Buffer, pos1 - mapping1 + s1.Start, pos2 - mapping1 + s1.Start);
			scTemp.Append(seg);
			return scTemp;
		}
		
		// try to split the ending segment	
		Segment sl = new Segment(s2.Buffer, s2.Start, pos2 - mapping2 + s2.Start );
		
		// try to split the starting segment
		Segment sf = new Segment(s1.Buffer, pos1 - mapping1 + s1.Start, s1.End);
		
				
		SegmentCollection sc = new SegmentCollection();
		
		// append the first segment 
		sc.Append(sf);
		
		Util.List<Segment>.Node n = node1.next;
		
		// append to new and remove from old
		// all segments up to node2
		while(ReferenceEquals(n, node2) == false) {
			sc.Append(new Segment(n.data.Buffer, n.data.Start, n.data.End));
			n = n.next;	
		}
		
		sc.Append(sl);
		
		return sc;
	}
}

} // end namespace
