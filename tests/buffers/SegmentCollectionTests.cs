// created on 3/8/2005 at 5:15 PM
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
using Bless.Buffers;

namespace BlessTests.Buffers {

[TestFixture]
public class SegmentCollectionTests {

	Bless.Buffers.Buffer buf1;
	Bless.Buffers.Buffer buf2;

class MockBuffer: Bless.Buffers.Buffer {

		public override void Put(long pos, byte[] data) { }
		public override int Get(byte[] buf, long pos, int len)  { return 0;}
		public override void Append(byte[] data) { }
		public override void Append(byte data) { }
		public override byte this[long index] {
			set { }
			get { return 0;}
		}
		public override long Size {
			get { return 0; }
		}

	}

	[SetUp]
	public void Init() {
		buf1 = new MockBuffer();
		buf2 = new MockBuffer();
	}

	[Test]
	public void AppendSegmentsTest1() {
		SegmentCollection segs = new SegmentCollection();
		Segment s1, s2;
		s1 = new Segment(buf1, 0, 5);
		s2 = new Segment(buf2, 6, 5);
		segs.Append(s1);
		segs.Append(s2);
		Assert.AreSame(s1, segs.List[0]);
		Assert.AreSame(s2, segs.List[1]);
	}

	[Test]
	public void AppendSegmentsTest2() {
		SegmentCollection segs = new SegmentCollection();
		Segment s1, s2;
		s1 = new Segment(buf1, 0, 5);
		s2 = new Segment(buf1, 6, 8);
		segs.Append(s1);
		segs.Append(s2);
		Assert.AreSame(s1, segs.List[0]);
		Assert.IsTrue(s1.Start == 0);
		Assert.IsTrue(s1.End == 8);
		//Assert.IsNull(segs[1]);
	}

	[Test]
	public void FindSegmentTest() {
		SegmentCollection segs1 = new SegmentCollection();
		Segment s1, s2, s3, s4;
		s1 = new Segment(buf1, 0, 5);
		s2 = new Segment(buf2, 6, 8);
		s3 = new Segment(buf2, 0, 5);
		s4 = new Segment(buf1, 6, 8);
		segs1.Append(s1);
		segs1.Append(s2);
		segs1.Append(s3);
		segs1.Append(s4);

		long map;
		List.Node n;

		Assert.AreSame(s1, segs1.FindSegment(4, out map, out n));
		Assert.AreEqual(0, map);
		Assert.AreSame(s3, segs1.FindSegment(10,  out map, out n));
		Assert.AreEqual(9, map);
	}


	[Test]
	public void InsertSegmentCollectionTest1() {
		SegmentCollection segs1 = new SegmentCollection();
		SegmentCollection segs2 = new SegmentCollection();
		Segment s1, s2, s3, s4;
		s1 = new Segment(buf1, 0, 5);
		s2 = new Segment(buf2, 6, 8);
		s3 = new Segment(buf2, 0, 5);
		s4 = new Segment(buf1, 6, 8);

		segs1.Append(s1);
		segs1.Append(s2);
		segs2.Append(s3);
		segs2.Append(s4);

		segs1.Insert(segs2, 6);

		Assert.AreSame(s1, segs1.List[0]);
		Assert.AreSame(s3, segs1.List[1]);
		Assert.AreSame(s4, segs1.List[2]);
		Assert.AreSame(s2, segs1.List[3]);
		//Assert.IsNull(segs[1]);
	}


	[Test]
	public void InsertSegmentCollectionTest2() {
		SegmentCollection segs1 = new SegmentCollection();
		SegmentCollection segs2 = new SegmentCollection();
		Segment s1, s2, s3, s4;
		s1 = new Segment(buf1, 0, 5);
		s2 = new Segment(buf2, 6, 8);
		s3 = new Segment(buf2, 0, 5);
		s4 = new Segment(buf1, 6, 8);

		segs1.Append(s1);
		segs1.Append(s2);
		segs2.Append(s3);
		segs2.Append(s4);

		segs1.Insert(segs2, 4);

		Assert.IsTrue(s1.End == 3);
		Assert.AreSame(s1, segs1.List[0]);
		Assert.AreSame(s3, segs1.List[1]);
		Assert.AreSame(s4, segs1.List[2]);
		Assert.AreSame(s2, segs1.List[4]);
		Assert.IsTrue(((Segment)segs1.List[3]).Start == 4);
		Assert.IsTrue(((Segment)segs1.List[3]).Buffer == buf1);
		Assert.IsTrue(((Segment)segs1.List[3]).End == 5);
		//Assert.IsNull(segs[1]);
	}

	[Test]
	public void InsertSegmentCollectionAtEndTest() {
		SegmentCollection segs1 = new SegmentCollection();
		SegmentCollection segs2 = new SegmentCollection();
		Segment s1, s2, s3, s4;
		s1 = new Segment(buf1, 0, 5);
		s2 = new Segment(buf2, 6, 8);
		s3 = new Segment(buf2, 0, 5);
		s4 = new Segment(buf1, 6, 8);

		segs1.Append(s1);
		segs1.Append(s2);
		segs2.Append(s3);
		segs2.Append(s4);

		segs1.Insert(segs2, 9);

		Assert.AreSame(s1, segs1.List[0]);
		Assert.AreSame(s2, segs1.List[1]);
		Assert.AreSame(s3, segs1.List[2]);
		Assert.AreSame(s4, segs1.List[3]);
		//Assert.IsNull(segs[1]);
	}

	[Test]
	public void InsertSegmentCollectionThenFindTest() {
		SegmentCollection segs1 = new SegmentCollection();
		SegmentCollection segs2 = new SegmentCollection();
		Segment s1, s2, s3, s4;
		s1 = new Segment(buf1, 0, 5);
		s2 = new Segment(buf2, 6, 8);
		s3 = new Segment(buf2, 0, 5);
		s4 = new Segment(buf1, 6, 8);

		segs1.Append(s1);
		segs1.Append(s2);
		segs2.Append(s3);
		segs2.Append(s4);

		segs1.Insert(segs2, 4);

		long map;
		List.Node n;
		//segs1.List.Display();
		Assert.AreSame(s1, segs1.FindSegment(3, out map, out n), "s1");
		Assert.AreEqual(0, map);
		Assert.AreSame(s3, segs1.FindSegment(7,  out map, out n), "s3");
		Assert.AreEqual(4, map);
		Assert.AreSame(s4, segs1.FindSegment(12,  out map, out n), "s4");
		Assert.AreEqual(10, map);
		Segment s5 = segs1.FindSegment(13,  out map, out n);
		Assert.AreEqual(13, map);
		Assert.IsTrue(s5.End == s5.Start + 1, "s1'");
		Assert.AreSame(s2, segs1.FindSegment(15,  out map, out n), "s2");
		Assert.AreEqual(15, map);

	}


	[Test]
	public void DeleteFromSegmentCollectionTest1() {
		SegmentCollection segs1 = new SegmentCollection();
		Segment s1, s2, s3, s4;
		s1 = new Segment(buf1, 0, 5);
		s2 = new Segment(buf2, 6, 8);
		s3 = new Segment(buf2, 0, 5);
		s4 = new Segment(buf1, 6, 8);
		segs1.Append(s1);
		segs1.Append(s2);
		segs1.Append(s3);
		segs1.Append(s4);

		segs1.DeleteRange(7, 15);
		Assert.AreEqual(3, segs1.List.Count, "#1");
		Assert.AreEqual(6, ((Segment)segs1.List[0]).Size, "#2");
		Assert.AreEqual(6, ((Segment)segs1.List[1]).Start, "#3");
		Assert.AreEqual(6, ((Segment)segs1.List[1]).End, "#4");
		Assert.AreEqual(7, ((Segment)segs1.List[2]).Start, "#5");
		Assert.AreEqual(8, ((Segment)segs1.List[2]).End, "#6");
	}

	[Test]
	public void DeleteFromSegmentCollectionTest2() {
		SegmentCollection segs1 = new SegmentCollection();
		Segment s1;
		s1 = new Segment(buf1, 0, 8);
		segs1.Append(s1);
		SegmentCollection del = segs1.DeleteRange(3, 6);

		Assert.AreEqual(2, segs1.List.Count, "#0");
		Assert.AreEqual(0, ((Segment)segs1.List[0]).Start, "#1");
		Assert.AreEqual(2, ((Segment)segs1.List[0]).End, "#2");
		Assert.AreEqual(7, ((Segment)segs1.List[1]).Start, "#3");
		Assert.AreEqual(8, ((Segment)segs1.List[1]).End, "#4");

		Assert.AreEqual(1, del.List.Count, "#5");
		Assert.AreEqual(3, ((Segment)del.List[0]).Start, "#6");
		Assert.AreEqual(6, ((Segment)del.List[0]).End, "#7");

	}

	[Test]
	public void DeleteFromSegmentCollectionTest3() {
		SegmentCollection segs1 = new SegmentCollection();
		Segment s1;
		s1 = new Segment(buf1, 0, 8);
		segs1.Append(s1);
		SegmentCollection del = segs1.DeleteRange(0, 6);

		Assert.AreEqual(1, segs1.List.Count, "#0");
		Assert.AreEqual(7, ((Segment)segs1.List[0]).Start, "#1");
		Assert.AreEqual(8, ((Segment)segs1.List[0]).End, "#2");

		Assert.AreEqual(1, del.List.Count, "#5");
		Assert.AreEqual(0, ((Segment)del.List[0]).Start, "#6");
		Assert.AreEqual(6, ((Segment)del.List[0]).End, "#7");

	}

	[Test]
	public void DeleteFromSegmentCollectionTest4() {
		SegmentCollection segs1 = new SegmentCollection();
		Segment s1;
		s1 = new Segment(buf1, 0, 8);
		segs1.Append(s1);
		SegmentCollection del = segs1.DeleteRange(0, 8);

		Assert.AreEqual(0, segs1.List.Count, "#0");

		Assert.AreEqual(1, del.List.Count, "#5");
		Assert.AreEqual(0, ((Segment)del.List[0]).Start, "#6");
		Assert.AreEqual(8, ((Segment)del.List[0]).End, "#7");

	}

	[Test]
	public void DeleteFromSegmentCollectionTest5() {
		SegmentCollection segs1 = new SegmentCollection();
		Segment s1;
		s1 = new Segment(buf1, 0, 8);
		segs1.Append(s1);
		SegmentCollection del = segs1.DeleteRange(6, 8);

		Assert.AreEqual(1, segs1.List.Count, "#0");
		Assert.AreEqual(0, ((Segment)segs1.List[0]).Start, "#1");
		Assert.AreEqual(5, ((Segment)segs1.List[0]).End, "#2");

		Assert.AreEqual(1, del.List.Count, "#5");
		Assert.AreEqual(6, ((Segment)del.List[0]).Start, "#6");
		Assert.AreEqual(8, ((Segment)del.List[0]).End, "#7");
	}

	[Test]
	public void InsertToSegmentCollectionThenDeleteTest() {
		SegmentCollection segs1 = new SegmentCollection();
		Segment s1, s2;
		s1 = new Segment(buf1, 0, 2);
		s2 = new Segment(buf1, 3, 5);
		segs1.Append(s1);
		SegmentCollection segs2 = new SegmentCollection();
		segs2.Append(s2);
		segs1.Insert(segs2, 2);

		segs1.DeleteRange(2, 4);

		long map;
		List.Node n;
		Segment s = segs1.FindSegment(0, out map, out n);
		Assert.IsNotNull(s, "#1");
		Assert.AreEqual(0, map, "#2");
		Assert.AreEqual(0, s.Start, "#3");



	}

	[Test]
	public void ReplaceAtEnd() {
		SegmentCollection segs1 = new SegmentCollection();
		Segment s1, s2, s3;
		s1 = new Segment(buf1, 0, 4);
		s2 = new Segment(buf2, 7, 7);
		s3 = new Segment(buf2, 8, 8);
		segs1.Append(s1);
		//segs1.List.Display();
		segs1.DeleteRange(4, 4);
		//segs1.List.Display();
		segs1.Append(s2);
		//segs1.List.Display();
		segs1.DeleteRange(4, 4);
		//segs1.List.Display();
		segs1.Append(s3);
		//segs1.List.Display();

		Assert.AreEqual(2, segs1.List.Count, "#1");
		Assert.AreEqual(3, (segs1.List[0] as Segment).End, "#2");
		Assert.AreEqual(8, (segs1.List[1] as Segment).Start, "#3");
		Assert.AreEqual(8, (segs1.List[1] as Segment).End, "#4");

	}

	[Test]
	public void InsertInEmptyCollection() {
		SegmentCollection segs1 = new SegmentCollection();
		SegmentCollection segs2 = new SegmentCollection();
		Segment s1 = new Segment(buf1, 0, 4);
		Segment s2 = new Segment(buf2, 7, 9);

		segs2.Append(s1);
		segs2.Append(s2);

		segs1.Insert(segs2, 0);

		Assert.AreEqual(2, segs1.List.Count, "#1");
		Assert.AreEqual(4, (segs1.List[0] as Segment).End, "#2");
		Assert.AreEqual(7, (segs1.List[1] as Segment).Start, "#3");
		Assert.AreEqual(9, (segs1.List[1] as Segment).End, "#4");
	}
}

} // end namespace