// created on 3/8/2005 at 5:20 PM
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
public class RangeTests
{

	[Test]
	public void IntersectTest()
	{
		Range r1 = new Range(5, 10);
		Range r2 = new Range(6, 9);

		r1.Intersect(r2);
		Assert.AreEqual(6, r1.Start);
		Assert.AreEqual(9, r1.End);

		Range r3 = new Range(5, 10);
		Range r4 = new Range(4, 8);

		r3.Intersect(r4);
		Assert.AreEqual(5, r3.Start);
		Assert.AreEqual(8, r4.End);

		Range r5 = new Range(5, 10);
		Range r6 = new Range(10, 14);

		r5.Intersect(r6);
		Assert.AreEqual(10, r5.Start);
		Assert.AreEqual(10, r5.End);

	}

	[Test]
	public void DifferenceTest()
	{
		Range r1 = new Range(5, 10);
		Range r2 = new Range(6, 9);
		Range r3 = new Range();

		r1.Difference(r2, r3);
		Assert.AreEqual(5, r1.Start, "#1");
		Assert.AreEqual(5, r1.End, "#2");

		Assert.AreEqual(10, r3.Start, "#3");
		Assert.AreEqual(10, r3.End, "#4");

		Range r4 = new Range(5, 10);
		Range r5 = new Range(5, 8);

		r4.Difference(r5, r3);
		Assert.AreEqual(9, r4.Start, "#5");
		Assert.AreEqual(10, r4.End, "#6");
		Assert.AreEqual(true, r3.IsEmpty(), "#7");

	}
	
	[Test]
	public void SplitAtomicTest()
	{
		Range r = new Range(0, 10);
		Range s = new Range(4, 6);
		Range[] ra = new Range[]{new Range(), new Range(), new Range()};
		
		Assert.IsTrue(r.Equals(new Range(0, 10)), "#0");
		
		Range.SplitAtomic(ra, r, s);
		Assert.AreEqual(new Range(0, 3), ra[0], "#1.0");
		Assert.AreEqual(new Range(4, 6), ra[1], "#1.1");
		Assert.AreEqual(new Range(7, 10), ra[2], "#1.2");
		
		s = new Range(0, 4);
		Range.SplitAtomic(ra, r, s);
		Assert.AreEqual(new Range(), ra[0], "#2.0");
		Assert.AreEqual(new Range(0, 4), ra[1], "#2.1");
		Assert.AreEqual(new Range(5, 10), ra[2], "#2.2");
		
		s = new Range(6, 10);
		Range.SplitAtomic(ra, r, s);
		Assert.AreEqual(new Range(0, 5), ra[0], "#3.0");
		Assert.AreEqual(new Range(6, 10), ra[1], "#3.1");
		Assert.AreEqual(new Range(), ra[2], "#3.2");
		
		s = new Range(0, 10);
		Range.SplitAtomic(ra, r, s);
		Assert.AreEqual(new Range(), ra[0], "#4.0");
		Assert.AreEqual(new Range(0, 10), ra[1], "#4.1");
		Assert.AreEqual(new Range(), ra[2], "#4.2");
	}
	
	[Test]
	public void SplitAtomic1Test()
	{
		Range r = new Range(10, 20);
		Range s = new Range(8, 13);
		
		Range[] ra = new Range[]{new Range(), new Range(), new Range()};
		
		Range.SplitAtomic(ra, r, s);
		Assert.AreEqual(new Range(), ra[0], "#1.0");
		Assert.AreEqual(new Range(8, 13), ra[1], "#1.1");
		Assert.AreEqual(new Range(14, 20), ra[2], "#1.2");
		
		s = new Range(16, 26);
		Range.SplitAtomic(ra, r, s);
		Assert.AreEqual(new Range(10, 15), ra[0], "#2.0");
		Assert.AreEqual(new Range(16, 26), ra[1], "#2.1");
		Assert.AreEqual(new Range(), ra[2], "#2.2");
	}
	
}

} // end namespace
