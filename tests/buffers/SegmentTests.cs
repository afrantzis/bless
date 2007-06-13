// created on 3/8/2005 at 5:14 PM
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

using Bless.Buffers;

namespace BlessTests.Buffers {

[TestFixture]
public class SegmentTests {

	[Test]
	public void SplitTest1() {
		Segment	s = new Segment(null, 3, 8);
		Segment s1 = s.SplitAt(3);
		Assert.AreEqual(3, s.Start);
		Assert.AreEqual(5, s.End);
		Assert.AreEqual(6, s1.Start);
		Assert.AreEqual(8, s1.End);
	}

	[Test]
	public void SplitTest2() {
		Segment	s = new Segment(null, 3, 8);
		Segment s1 = s.SplitAt(0);
		Assert.IsNull(s1);
	}

	[Test]
	public void ToStringTest() {
		Segment	s = new Segment(null, 3, 8);
		Assert.AreEqual("(3->8)", s.ToString());
	}


}

} // end namespace