// Created 1:14 PM 16/3/2008
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

namespace Bless.Util
{

public class IntervalTree<T> : RedBlackTree<long, T> where T : IRange
{
	protected class ITNode : INode
	{
		private long max;
		private long key;
		private IList<T> values;	
		private INode left, right;
		private Color color;
		
		public long Max { 
			get { return max; }
			set { max = value; }
		}
		
		public long Key {
			get { return key; }
			set { key = value; }
		}
		
		public IList<T> Values {
			get { return values; }
			set { values = value; UpdateMax();}
		}
		
		public INode Left {
			get { return left; }
			set { left = value; UpdateMax();}
		}
		
		public INode Right {
			get { return right; }
			set { right = value; UpdateMax();}
		}
		
		public Color Color {
			get { return color; }
			set { color = value; }
		}
		
		public ITNode(long key, T val, Color color)
		{
			this.key = key;
			this.values = new System.Collections.Generic.List<T>(); 
			this.values.Add(val);
			this.color = color;
			max = val.End; 
		}
		
		private void UpdateMax()
		{
			long tmpMax = -1;
			
			if (this.Left != null)
				tmpMax = Math.Max(tmpMax, (this.Left as ITNode).Max);
		
			if (this.Right != null)
				tmpMax = Math.Max(tmpMax, (this.Right as ITNode).Max);
			
			foreach(T r in this.Values) {
				tmpMax = Math.Max(tmpMax, r.End);
			}
		
			this.max = tmpMax;
		}
		
	}
	
	protected class ITNodeFactory : INodeFactory
	{
		public INode CreateNode(long key, T val, Color color) 
		{
			return new ITNode(key, val, color);
		}
		
	}
	
	private System.Collections.Generic.List<T> searchResults;
	
	public IntervalTree() : base(new ITNodeFactory())
	{
	
	}
		
	private bool RangesOverlap(IRange r1, IRange r2)
	{
		if (r1.Start >= r2.Start && r1.Start <= r2.End)
			return true;
		if (r1.End >= r2.Start && r1.End <= r2.End)
			return true;
		if (r1.Start <= r2.Start && r1.End >= r2.End)
			return true;
		
		return false;
	}
	
	protected override void AddToValues(INode n, T val)
	{
		base.AddToValues(n, val);
		
		(n as ITNode).Max = Math.Max((n as ITNode).Max, val.End);
	}
	
	public void Insert(T r)
	{
		Insert(r.Start, r);
	}
	
	public IList<T> SearchOverlap(T r)
	{
		lock(lockObj) {
			searchResults = new System.Collections.Generic.List<T>();
			SearchOverlap(root as ITNode, r);
			return searchResults;
		}
	}
	
	private void SearchOverlap(ITNode n, T r)
	{
		
		if (n != null) {
			if (n.Key <= r.End && n.Right != null)
				SearchOverlap(n.Right as ITNode, r);
			
			if (n.Max >= r.Start) {
				foreach(T q in n.Values) {
					if (RangesOverlap(q, r))
						searchResults.Add(q);
				}
				if(n.Left != null)
					SearchOverlap(n.Left as ITNode, r);
			}
		}
	}
	
	public void Delete(T r)
	{
		ITNode n = (this.SearchNode(r.Start) as ITNode);
		
		if (n == null)
			return;
		
		n.Values.Remove(r);
		
		if (n.Values.Count == 0)
			this.Delete(r.Start);
	}
}

} //end namespace
