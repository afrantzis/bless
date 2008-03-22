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
using System.IO;
using System.Text;

namespace Bless.Util
{
/// <summary>
/// A (left-leaning) red black tree (see [Sedgewick 2008]).
/// Duplicates keys are all stored in the same node. 
/// </summary>
public class RedBlackTree<K, V> where K:IComparable<K>
{
	protected enum Color {Red, Black}
	
	private INodeFactory nodeFactory;
	protected INode root;
	protected object lockObj = new object();
	private IList<V> tmpValues;
	
	protected interface INode
	{
		K Key { get; set; }
		IList<V> Values { get; set;}	
		INode Left { get; set;}
		INode Right { get; set;}
		Color Color { get; set;}
	}
	
	protected interface INodeFactory
	{
		INode CreateNode(K key, V val, Color color);
	}
	
	protected class NodeFactory : INodeFactory
	{
		public INode CreateNode(K key, V val, Color color)
		{
			return new Node(key, val, color);
		}
	}
	
	protected class Node : INode
	{
		private K key;
		private IList<V> values;	
		private INode left, right;
		private Color color;
		
		public K Key {
			get { return key; }
			set { key = value; }
		}
		
		public IList<V> Values {
			get { return values; }
			set { values = value; }
		}
		
		public INode Left {
			get { return left; }
			set { left = value; }
		}
		
		public INode Right {
			get { return right; }
			set { right = value; }
		}
		
		public Color Color {
			get { return color; }
			set { color = value; }
		}
	    
		public Node(K key, V val, Color color)
		{
			this.key = key;
			this.values = new System.Collections.Generic.List<V>(); 
			this.values.Add(val);
			this.color = color;
		}
	}

	private bool IsRed(INode n)
	{
		if (n == null) return false;
		return (n.Color == Color.Red);
	}

	protected virtual INode RotL(INode h)
	{
		INode x = h.Right;
		h.Right = x.Left;
		x.Left = h;
		return x;
	}

	protected virtual INode RotR(INode h)
	{
		INode x = h.Left;
		h.Left = x.Right;
		x.Right = h;
		return x;
	}
	
	protected virtual INode SplitFourNode(INode h)
	{
		INode x = RotR(h);
		x.Left.Color = Color.Black;
		return x;
	}
	
	protected virtual INode LeanLeft(INode h)
	{
		INode x = RotL(h);
		x.Color = x.Left.Color;
		x.Left.Color = Color.Red;
		return x;
	}
	
	protected virtual INode LeanRight(INode h)
	{
		INode x = RotR(h);
		x.Color = x.Right.Color;
		x.Right.Color = Color.Red;
		return x;
	}
	
	protected INode MoveRedLeft(INode h)
	{
		h.Color = Color.Black;
		h.Left.Color = Color.Red;
		
 		if (IsRed(h.Right.Left)) {
 			h.Right = RotR(h.Right);
			h = RotL(h);
		}
		else
			h.Right.Color = Color.Red;
			
		return h;
	}
	
	protected INode MoveRedRight(INode h)
	{
		h.Color = Color.Black;
		h.Right.Color = Color.Red;
		
 		if (IsRed(h.Left.Left)) {
 			h = RotR(h);
 			h.Color = Color.Red;
 			h.Left.Color = Color.Black;
		}
		else
			h.Left.Color = Color.Red;
			
		return h;
	}
	
	private INode DeleteMin(INode h)
	{
		// remove node on bottom level (h must be RED by invariant)
		if (h.Left == null)
			return null;
		
		// push red link down if necessary
		if (!IsRed(h.Left) && !IsRed(h.Left.Left))
			h = MoveRedLeft(h);
			
		// move down one level
		h.Left = DeleteMin(h.Left);
		
		// fix right-leaning red links on the way up the tree
		if (IsRed(h.Right))
			h = LeanLeft(h);
		
		return h;
	}
	
	private INode FindMinNode(INode h)
	{
		while (h.Left != null)
			h = h.Left;
		
		return h;
	}

	protected virtual void AddToValues(INode n, V val)
	{
		n.Values.Add(val);
	}
	
	public RedBlackTree()
	{
		nodeFactory = new NodeFactory();
	}

	protected RedBlackTree(INodeFactory nf)
	{
		nodeFactory = nf;
	}

	private INode Insert(INode h, K key, V val)
	{
		if (h == null)
			return nodeFactory.CreateNode(key, val, Color.Red);
		
		// split 4-nodes on the way down
		if (IsRed(h.Left) && IsRed(h.Left.Left))
			h = SplitFourNode(h);
		
		// standard BST insert code
		int cmp = key.CompareTo(h.Key);
		
		if (cmp == 0)
			AddToValues(h, val);
		else if (cmp < 0)
			h.Left = Insert(h.Left, key, val);
		else
			h.Right = Insert(h.Right, key, val);
		
		// fix right-leaning reds on the way up
		if (IsRed(h.Right))
			h = LeanLeft(h);
		
		return h;
	}

	/// <summary>
	/// Insert a key and its associated value into the tree.
	/// </summary>
	/// <param name="key">
	/// A <see cref="K"/>
	/// </param>
	/// <param name="val">
	/// A <see cref="V"/>
	/// </param>
	public virtual void Insert(K key, V val)
	{
		lock(lockObj) {
			root = Insert(root, key, val);
			root.Color = Color.Black;
		}
	}
	
	/// <summary>
	/// Search for the tree node that contains a key. 
	/// </summary>
	/// <param name="key">
	/// A <see cref="K"/>
	/// </param>
	/// <returns>
	/// A <see cref="INode"/>
	/// </returns>
	protected virtual INode SearchNode(K key)
	{
		INode x = root;
			
		while (x != null) {
			int cmp = key.CompareTo(x.Key);
			
			if (cmp == 0) return x;
			else if (cmp < 0) x = x.Left;
			else if (cmp > 0) x = x.Right;
		}
		   
		return null;
	}
	
	/// <summary>
	/// Search for all the values associated with a key.
	/// </summary>
	/// <param name="key">
	/// A <see cref="K"/>
	/// </param>
	/// <returns>
	/// A <see cref="IList`1"/>
	/// </returns>
	public virtual IList<V> Search(K key)
	{
		lock(lockObj) {
			INode x = SearchNode(key);
			
			if (x != null)
				return x.Values;
			else
				return new System.Collections.Generic.List<V>(0);
		}
	}
	
	private INode Delete(INode h, K key)
	{
		if (h == null)
			return null;
		
		int cmp = key.CompareTo(h.Key);
		
		if (cmp < 0) { // left
			
			// push red right if necessary
			if (!IsRed(h.Left) && h.Left != null && !IsRed(h.Left.Left))
				h = MoveRedLeft(h);                           
			// move down (left)
			h.Left = Delete(h.Left, key);
		}
		else { // right or equal
			// rotate to push red right
			if (IsRed(h.Left)) {
				h = LeanRight(h);
				// LeanRight always rotates and brings (old) h to the right
				cmp = 1;
			}
			
			// equal (at bottom)...  delete node
			// or search reached bottom and we must go right (cmp == 1)
			// but we can't (h.Right == null) -> the key is not in the tree
			if ((cmp == 0 || cmp == 1) && (h.Right == null))
				return null;
				
			// push red right if necessary
			if (!IsRed(h.Right) && !IsRed(h.Right.Left)) {
				h = MoveRedRight(h);
				// MoveRedRight may or may not rotate so we must
				// check new node again
				cmp = key.CompareTo(h.Key);
			}
			
			// equal (not at bottom)
			if (cmp == 0) {
				INode minNode = FindMinNode(h.Right);
				// replace current node with successor key, value
      			h.Key = minNode.Key;                               
				h.Values = minNode.Values; 
				// delete successor
				h.Right = DeleteMin(h.Right);
			}
			else { // move down (right)
				h.Right = Delete(h.Right, key);
			}				
		}
		
		// Fix right-leaning red links on the way up the tree
		if (IsRed(h.Right)) h = LeanLeft(h);
		
		return h;
	}
	
	/// <summary>
	/// Delete a key and all its associated values from the tree.
	/// </summary>
	/// <param name="key">
	/// A <see cref="K"/>
	/// </param>
	public virtual void Delete(K key)
	{
		lock(lockObj) {
			root = Delete(root, key);
			if (root != null)
				root.Color = Color.Black;
		}
	}
	
	/// <summary>
	/// Dump the contents of the tree as a graphviz/dot file
	/// </summary>
	/// <param name="filename">
	/// A <see cref="System.String"/>
	/// </param>
	/// <param name="title">
	/// The title of the produced graph
	/// </param>
	public void DumpToDot(string filename, string title)
	{
		lock(lockObj) {
			StringBuilder sb = new StringBuilder("digraph {\nlabel=\""+title+"\"\n");
			DumpToDotInternal(root, sb);
			sb.Append("}\n");
			
			StreamWriter sw = new StreamWriter(filename);
			sw.Write(sb.ToString());
			sw.Close();
		}
	}
	
	public void Clear()
	{
		lock (lockObj) {
			root = null;
		}
	}
	
	public IList<V> GetValues()
	{
		lock (lockObj){
			tmpValues = new System.Collections.Generic.List<V>();
			GetValues(root);
			
			// clear private var
			IList<V> tv = tmpValues;
			tmpValues = null;
			return tv;
		}
	}
	
	private void GetValues(INode x)
	{
		if (x == null)
			return;
		
		GetValues(x.Left);
		
		foreach(V v in x.Values)
			tmpValues.Add(v);
		
		GetValues(x.Right);
	}
	
	private void DumpToDotInternal(INode h, StringBuilder sb)
	{
		if (h == null)
			return;
		
		DumpToDotInternal(h.Left, sb);
		
		if (h.Left != null) {
			string col = " [label=L, color=" + (h.Left.Color == Color.Red?"red, style=bold]\n":"black]");
			sb.Append(h.Key + " -> " + h.Left.Key + col +"\n");
		}
		
		if (h.Right != null) {
			string col = " [label=R, color=" + (h.Right.Color == Color.Red?"red, style=bold]\n":"black]");
			sb.Append(h.Key + " -> " + h.Right.Key + col + "\n");
		}
		
		DumpToDotInternal(h.Right, sb);
	}

}

} // end namespace