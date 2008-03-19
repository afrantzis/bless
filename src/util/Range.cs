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

using System;

namespace Bless.Util {

public interface IRange
{
	long Start { get; set; }
	long End { get; set; }
	long Size { get; }
	bool IsEmpty();
	void Clear();
}

///<summary>Represents a range of values</summary>
public class Range : IRange, IEquatable<Range>
{
	protected long start;
	protected long end;

	public long Start {
		get { return start; }
		set { start = value; }
	}

	public long End {
		get { return end; }
		set { end = value; }
	}

	public long Size {
		get {
			if (IsEmpty())
				return 0;
			else if (end >= start)
				return end -start + 1;
			else
				return start -end + 1;
		}
	}

	public Range()
	{
		Clear();
	}

	public Range(long s, long e)
	{
		start = s;
		end = e;
	}

	///<summary>copy constructor</summary>
	public Range(IRange r): this(r.Start, r.End)
	{

	}

	///<summary>value equality test</summary>
	public bool Equals(Range r)
	{
		return ((start == r.Start) && (end == r.End) || (end == r.Start) && (start == r.End));
	}
	
	///<summary>value equality test (for general objects)</summary>
	public override bool Equals(object obj)
	{
		Range r = obj as Range;
		if (r != null)
			return Equals(r);
		else
			return false;
	}

	// we should override GetHashCode() because we
	// are overriding Equals()
	public override int GetHashCode()
	{
		return (start.GetHashCode() ^ end.GetHashCode());
	}

	public void Clear()
	{
		start = -1;
		end = -1;
	}

	public bool Contains(long n)
	{
		if (start < end)
			return (n >= start && n <= end);
		else
			return (n <= start && n >= end);
	}

	///<summary>Evaluates the intersection of two ranges</summary>
	// NOTE: expects both ranges to be sorted
	public void Intersect(Range sel)
	{
		if (sel.Start >= start && sel.Start <= end) {
			start = sel.Start;
			if (sel.End < end)
				end = sel.End;
		}
		else if (sel.Start < start && sel.End >= start) {
			if (sel.End < end)
				end = sel.End;
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
				end = r.Start - 1;
				r1.Clear();
			}
			else {
				r1.Start = r.End + 1;
				r1.End = end;
				end = r.Start - 1;
			}
		}
		else if (r.Start <= start && r.End >= start) {
			r1.Clear();
			if (r.End < end)
				start = r.End + 1;
			else
				Clear();
		}

	}

	public bool IsEmpty()
	{
		return ((start == -1) && (end == -1));
	}
	
	static public void SplitAtomic(Range[] results, Range r, Range s)
	{
		
		if (r.Contains(s.Start)) {
			results[0].Start = r.Start;
			results[0].End = s.Start - 1;
			
			results[1].Start = s.Start;
			results[1].End = s.End;
			
			if (r.Contains(s.End)) {
				results[2].Start = s.End + 1;
				results[2].End = r.End;
			}
			else {
				results[2].Clear();
			}
		}
		else if (r.Contains(s.End)) {
			results[0].Clear();
			
			results[1].Start = s.Start;
			results[1].End = s.End;
			
			results[2].Start = s.End + 1;
			results[2].End = r.End;
		}
		else if (s.Start < r.Start && s.End > r.End) {
			results[0].Clear();
			
			results[1].Start = s.Start;
			results[1].End = s.End;
			
			results[2].Clear();	
		}
		else {
			if (r.Start < s.Start) {
				results[0].Start = r.Start;
				results[0].End = r.End;
				
				results[2].Clear();
			}
			else {
				results[2].Start = r.Start;
				results[2].End = r.End;
				
				results[1].Clear();
			}
			
			results[1].Start = s.Start;
			results[1].End = s.End;
		}
		
		foreach(Range result in results) {
			if (result.Start > result.End)
				result.Clear();
		}	
		
	}
	
	public bool Overlaps(IRange r)
	{
		if (r.Start >= Start && r.Start <= End)
			return true;
		if (r.End >= Start && r.End <= End)
			return true;
		if (r.Start <= Start && r.End >= End)
			return true;
		
		return false;
	}

	///<summary>Make sure range start is less or equal to end</summary>
	public void Sort()
	{
		if (end < start) {
			long t = start;
			start = end;
			end = t;
		}
	}
	
	public override string ToString()
	{
		return string.Format("{0} -> {1}", start, end);
	}
}

} // end namespace
