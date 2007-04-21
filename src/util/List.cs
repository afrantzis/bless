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
using System.Collections;
using System.Collections.Generic;

namespace Bless.Util {

///<summary>A double-linked list</summary>
public class List<T> : IEnumerable<T>
{

	///<summary>A node of the list</summary>
	public class Node
	{
		public T data;
		public Node next;
		public Node prev;
	
		public Node(T d, Node p, Node n) {
			data = d;
			prev = p;
			next = n;
		}
		
	}


	Node m_first;
	Node m_last;
	
	int m_count;
	
	
	public List() 
	{
		m_first=null;
		m_last=null;
	}
	
	
	private Node AddFirst(T o) 
	{
		m_first = new Node(o, null, null);
		m_last = m_first;
		++m_count;
		return m_first;
	}
	
	///<summary>Append an object to the list</summary>
	public Node Append(T o) 
	{
		return InsertAfter(m_last, o);
	}
	
	///<summary>Insert an object before a node in the list</summary>
	public Node InsertBefore(Node n, T o) 
	{
		if (m_last == null) { // first entry?
			return AddFirst(o);
		}	
		else {
			Node tmp = new Node(o, n.prev, n);
			if (n.prev != null) 
				n.prev.next = tmp;
			n.prev = tmp;
			if (ReferenceEquals(n, m_first)) 
				m_first = tmp;
			++m_count;
			return tmp;
		}
	}
	
	///<summary>Insert an object after a node in the list</summary>
	public Node InsertAfter(Node n, T o) 
	{
		if (m_last == null) { // first entry?
			return AddFirst(o);
		}	
		else {
			Node tmp = new Node(o, n, n.next);
			if (n.next != null)
				n.next.prev = tmp;
			n.next = tmp;
			if (ReferenceEquals(n, m_last)) 
				m_last = tmp;
			++m_count;
			return tmp;
		}
	}

	///<summary>Remove a node (unlink it)</summary>
	public void Remove(Node n) 
	{
		if (n.prev != null) 
			n.prev.next = n.next;
		if (n.next != null)
			n.next.prev = n.prev;
		if (ReferenceEquals(n, m_first)) 
			m_first = n.next;
		if (ReferenceEquals(n, m_last)) 
			m_last = n.prev;
		n.next = null;
		n.prev = null;
		--m_count;
	}
	
	public T this[int index] {
		get {
			//Console.WriteLine("Asking for index {0}", index);
			if (index > m_count)
				return default(T); 
			Node n = m_first;
			for(int i = 0; i < index; i++)
				n = n.next;
			return n.data;
		  }
	}

	public IEnumerator<T> GetEnumerator()
	{
		Node currentNode = First;
		
		while (currentNode != null) {
			yield return currentNode.data;
			currentNode = currentNode.next;
		}
	}
	
	IEnumerator IEnumerable.GetEnumerator()
	{
		return GetEnumerator();
	}
	
	public void Display() 
	{
		Node n = m_first;
		while (n != null) {
			Console.Write("{0}<=>", n.data);
			n = n.next;
		}
		Console.WriteLine();
	}
	
	public Node First {
		get { return m_first; }
	}

	public Node Last {
		get { return m_last; }
	}

	public int Count {
		get { return m_count; }
	}


}

} // end namespace
