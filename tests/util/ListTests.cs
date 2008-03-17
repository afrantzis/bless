// created on 3/8/2005 at 5:19 PM
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
using NUnit.Framework;
using System;

using Bless.Util;

namespace BlessTests.Util {

[TestFixture]
public class ListTests {

	[Test]
	public void AppendTest() {
		List<int> list = new List<int>();
		list.Append(4);
		list.Append(7);
		List<int>.Node n = list.First;
		Assert.AreEqual(4, n.data);
		Assert.AreEqual(7, n.next.data);
	}


	[Test]
	public void CountTest() {
		List<int> list = new List<int>();
		list.Append(1);
		list.Append(2);
		list.Append(3);

		Assert.AreEqual(3, list.Count);
	}


	[Test]
	public void InsertBeforeTest() {
		List<int> list = new List<int>();
		list.InsertBefore(list.First, 3);
		list.InsertBefore(list.First, 1);
		list.InsertBefore(list.Last, 2);


		List<int>.Node n = list.First;
		Assert.AreEqual(1, n.data);
		Assert.AreEqual(2, n.next.data);
		Assert.AreEqual(3, n.next.next.data);
	}

	[Test]
	public void InsertAfterTest() {
		List<int> list = new List<int>();
		list.InsertAfter(list.First, 1);
		list.InsertAfter(list.Last, 3);
		list.InsertAfter(list.First, 2);


		List<int>.Node n = list.First;
		Assert.AreEqual(1, n.data);
		Assert.AreEqual(2, n.next.data);
		Assert.AreEqual(3, n.next.next.data);
	}


	[Test]
	public void IndexerTest() {
		List<int> list = new List<int>();
		list.Append(1);
		list.Append(2);
		list.Append(3);

		Assert.AreEqual(1, list[0]);
		Assert.AreEqual(2, list[1]);
		Assert.AreEqual(3, list[2]);
		//if (list[3]!=null)
		//Assert.Fail("Expected: null!");
	}

	[Test]
	public void RemoveTest() {
		List<int> list = new List<int>();
		List<int>.Node n1 = list.Append(1);
		list.Append(2);
		List<int>.Node n3 = list.Append(3);
		List<int>.Node n4 = list.Append(4);

		list.Remove(n1);
		Assert.AreEqual(3, list.Count, "#1");
		Assert.AreEqual(2, list[0], "#2");
		Assert.AreEqual(3, list[1], "#3");
		Assert.AreEqual(4, list[2], "#4");

		list.Remove(n3);
		Assert.AreEqual(2, list.Count, "#5");
		Assert.AreEqual(2, list[0], "#6");
		Assert.AreEqual(4, list[1], "#7");

		list.Remove(n4);
		Assert.AreEqual(1, list.Count, "#8");
		Assert.AreEqual(2, list[0], "#9");

		//if (list[3]!=null)
		//Assert.Fail("Expected: null!");
	}

	[Test]
	public void RemoveLastTest() {
		List<int> list = new List<int>();
		list.Append(1);
		List<int>.Node n2 = list.Append(2);

		list.Remove(n2);
		list.Append(3);

		Assert.AreEqual(2, list.Count, "#1");
		Assert.AreEqual(1, list[0], "#2");
		Assert.AreEqual(3, list[1], "#3");

	}


}

} // end namespace
