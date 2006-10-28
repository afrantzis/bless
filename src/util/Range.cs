// created on 10/25/2004 at 9:32 AM
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
 
namespace Bless.Util {

///<summary>Represents a range of values</summary>
public class Range {
	long start;
	long end;
	
	public long Start {
		get { return start; }
		set { start=value; }	
	}
	
	public long End {
		get { return end; }
		set { end=value; }	
	}
 
	public long Size {
		get { 
				if (IsEmpty())
					return 0;
				else if (end>=start) 
					return end-start+1;
				else
					return start-end+1;
			}
	}
	
	public Range()
	{
		Clear();
	}

	public Range(long s, long e)
	{
		start=s;
		end=e;
	} 
 
	///<summary>copy constructor</summary> 
	public Range(Range r): this(r.Start, r.End)
	{
		
	} 
 
	///<summary>value equality test</summary>
	public override bool Equals(object obj)
	{
		//Check for null and compare run-time types.
		if (obj == null || GetType() != obj.GetType()) 
			return false;
		Range r = (Range)obj;
	
		return ((start == r.Start) && (end == r.End) || (end == r.Start) && (start == r.End));
	} 
 	
 	// we should override GetHashCode() because we
 	// are overriding Equals()
 	public override int GetHashCode()
 	{
 		return (start.GetHashCode()^end.GetHashCode());
 	}
 	
	public void Clear()
	{
		start=-1;
		end=-1;
	}
 
	public bool Contains(long n)
	{
		if (start<end)
			return (n>=start && n<=end);
		else
			return (n<=start && n>=end);
	}
	
	///<summary>Evaluates the intersection of two ranges</summary>
	// NOTE: expects both ranges to be sorted 
	public void Intersect(Range sel)
	{	
		if (sel.Start >= start && sel.Start <= end) {
			start=sel.Start;
			if (sel.End < end)
				end=sel.End;
		}
		else if (sel.Start < start && sel.End >= start) {	
			if (sel.End < end)
				end=sel.End;
		}
		else
			Clear();
	}
	
	///<summary>Evaluates the difference of two ranges.</summary>
	// NOTE: expects both ranges to be sorted
	public void Difference(Range r, Range r1) 
	{
		
		if (r.Start > start && r.Start <= end) {
			if (r.End >= end) {
				end=r.Start-1;
				r1.Clear();
			}
			else {
				r1.Start=r.End+1;
				r1.End=end;
				end=r.Start-1;
			}	
		}
		else if (r.Start <= start && r.End >= start) {
			r1.Clear();	
			if (r.End < end)
				start=r.End+1;
			else
				Clear();
		}
			
	}
	
	public bool IsEmpty()
	{
		return ((start == -1) && (end == -1));
	}
	
	///<summary>Make sure range start is less or equal to end</summary>
	public void Sort() 
	{
		if (end<start) {
			long t=start;
			start=end;
			end=t;
		}
	}
}

} // end namespace
