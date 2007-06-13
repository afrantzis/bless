// created on 3/8/2005 at 5:02 PM
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
using System.Threading;

using Bless.Buffers;

namespace BlessTests.Buffers {

[TestFixture]
public class ByteBufferTests {

	[Test]
	public void AppendDoTest() {
		ByteBuffer bb = new ByteBuffer();
		byte[] ba = {1, 3, 5, 7, 13, 17};
		bb.Append(ba);

		Assert.AreEqual(6, bb.Size);
		Assert.AreEqual(1, bb[0]);
		Assert.AreEqual(3, bb[1]);
		Assert.AreEqual(5, bb[2]);
		Assert.AreEqual(7, bb[3]);
		Assert.AreEqual(13, bb[4]);
		Assert.AreEqual(17, bb[5]);
	}

	[Test]
	public void AppendUndoTest() {
		ByteBuffer bb = new ByteBuffer();
		byte[] ba = {1, 3, 5, 7};
		byte[] ba1 = {13, 17};
		bb.Append(ba);
		bb.Append(ba1);

		Assert.AreEqual(6, bb.Size, "#1");
		Assert.AreEqual(3, bb[1], "#2");
		Assert.AreEqual(17, bb[5], "#3");

		bb.Undo();

		Assert.AreEqual(4, bb.Size, "#4");
		Assert.AreEqual(1, bb[0], "#5");
		Assert.AreEqual(3, bb[1], "#6");
		Assert.AreEqual(5, bb[2], "#7");
		Assert.AreEqual(7, bb[3], "#8");
	}

	[Test]
	public void InsertDoTest() {
		ByteBuffer bb = new ByteBuffer();
		byte[] ba = {1, 3, 17};
		byte[] ba1 = {5, 7, 13};
		bb.Append(ba);
		//bb.Display("D1");
		bb.Insert(2, ba1);
		//bb.Display("D2");

		Assert.AreEqual(6, bb.Size, "#1");
		Assert.AreEqual(1, bb[0], "#2");
		Assert.AreEqual(3, bb[1], "#3");
		Assert.AreEqual(5, bb[2], "#4");
		Assert.AreEqual(7, bb[3], "#5");
		Assert.AreEqual(13, bb[4], "#6");
		Assert.AreEqual(17, bb[5], "#7");
	}

	[Test]
	public void InsertUndoTest() {
		ByteBuffer bb = new ByteBuffer();
		byte[] ba = {1, 3, 17};
		byte[] ba1 = {5, 7, 13};
		bb.Append(ba);
		//bb.Display("D1");
		bb.Insert(2, ba1);
		//bb.Display("D2");

		Assert.AreEqual(6, bb.Size, "#1");
		Assert.AreEqual(3, bb[1], "#2");
		Assert.AreEqual(17, bb[5], "#3");

		bb.Undo();
		//bb.Display("D3");
		Assert.AreEqual(3, bb.Size, "#4");
		Assert.AreEqual(1, bb[0], "#5");
		Assert.AreEqual(3, bb[1], "#6");
		Assert.AreEqual(17, bb[2], "#7");
	}

	[Test]
	public void DeleteDoTest() {
		ByteBuffer bb = new ByteBuffer();
		byte[] ba = {1, 3, 17};
		byte[] ba1 = {5, 7, 13};
		bb.Append(ba);
		//bb.Display("D1");
		bb.Insert(2, ba1);
		//bb.Display("D2");
		bb.Delete(1, 3);

		Assert.AreEqual(3, bb.Size, "#1");
		Assert.AreEqual(1, bb[0], "#2");
		Assert.AreEqual(13, bb[1], "#3");
		Assert.AreEqual(17, bb[2], "#4");
	}

	[Test]
	public void DeleteUndoTest() {
		ByteBuffer bb = new ByteBuffer();
		byte[] ba = {1, 3, 17};
		byte[] ba1 = {5, 7, 13};
		bb.Append(ba);
		//bb.Display("D1");
		bb.Insert(2, ba1);
		//bb.Display("D2");
		bb.Delete(1, 3);

		Assert.AreEqual(3, bb.Size, "#1");
		Assert.AreEqual(1, bb[0], "#2");
		Assert.AreEqual(13, bb[1], "#3");
		Assert.AreEqual(17, bb[2], "#4");

		bb.Undo();

		Assert.AreEqual(6, bb.Size, "#5");
		Assert.AreEqual(1, bb[0], "#6");
		Assert.AreEqual(3, bb[1], "#7");
		Assert.AreEqual(5, bb[2], "#8");
		Assert.AreEqual(7, bb[3], "#9");
		Assert.AreEqual(13, bb[4], "#10");
		Assert.AreEqual(17, bb[5], "#11");
	}

	[Test]
	public void FileLoadTest1() {
		ByteBuffer bb = ByteBuffer.FromFile("test1.bin");

		long size = bb.Size;
		int sum = 0;

		for (int i = 0; i < size; i++)
			sum ^= bb[i];

		Assert.AreEqual( 0x88, sum, "XOR Result");
	}

	[Test]
	public void FileLoadTest2() {
		ByteBuffer bb = ByteBuffer.FromFile("test1.bin");
		byte[] ba = {0x41, 0x61};
		bb.Append(ba);

		long size = bb.Size;
		int sum = 0;

		for (int i = 0; i < size; i++)
			sum ^= bb[i];

		Assert.AreEqual( 0x88 ^ 0x20, sum);
	}

	[Test]
	public void FileLoadTest3() {
		ByteBuffer bb = ByteBuffer.FromFile("test1.bin");
		byte[] ba = {0x41, 0x61};
		bb.Insert(768, ba);

		long size = bb.Size;
		int sum = 0;

		for (int i = 0; i < size; i++)
			sum ^= bb[i];

		Assert.AreEqual( 0x88 ^ 0x20, sum);
	}

	[Test]
	[Ignore("It fails, perhaps because of io-layer problems in mono >= 1.1.7")]
	public void FileSaveTest1() {
		ByteBuffer bb = ByteBuffer.FromFile("test1.bin");
		byte[] ba = {0x41, 0x61};
		long size1 = bb.Size;

		bb.Insert(768, ba);
		Assert.AreEqual(size1 + 2, bb.Size, "#1");

		IAsyncResult ar = bb.BeginSaveAs("test2.bin", null, null);
		ar.AsyncWaitHandle.WaitOne();
		bb.CloseFile();

		ByteBuffer bb1 = ByteBuffer.FromFile("test2.bin");

		Assert.AreEqual(bb.Size, bb1.Size, "#2");
		long size = bb1.Size;

		for (int i = 0; i < size; i++) {
			if (bb1[i] != bb[i])
				Assert.Fail(string.Format("#3 Difference at {0} ({1}!={2})", i, bb1[i], bb[i]));
		}
	}

	[Test]
	public void HasChangedTest()
	{
		ByteBuffer bb = new ByteBuffer();
		byte[] ba = {0x41, 0x61};
		Assert.AreEqual(false, bb.HasChanged, "#0");
		bb.Append(ba);
		Assert.AreEqual(true, bb.HasChanged, "#1");
		bb.Undo();
		Assert.AreEqual(false, bb.HasChanged, "#2");
	}

	[Test]
	public void InsertEmptyUndoTest()
	{
		ByteBuffer bb = new ByteBuffer();
		byte[] ba = {0x01, 0x02, 0xfe, 0xff};
		byte[] baEmpty = new byte[0];

		bb.Append(ba);
		bb.Insert(2, baEmpty);

		Assert.AreEqual(4, bb.Size, "#Size before undo");
		Assert.AreEqual(0x01, bb[0], "#bb[0] before undo");
		Assert.AreEqual(0x02, bb[1], "#bb[1] before undo");
		Assert.AreEqual(0xfe, bb[2], "#bb[2] before undo");
		Assert.AreEqual(0xff, bb[3], "#bb[3] before undo");

		bb.Undo();
		Assert.AreEqual(4, bb.Size, "#Size after undo");
		Assert.AreEqual(0x01, bb[0], "#bb[0] after undo");
		Assert.AreEqual(0x02, bb[1], "#bb[1] after undo");
		Assert.AreEqual(0xfe, bb[2], "#bb[2] after undo");
		Assert.AreEqual(0xff, bb[3], "#bb[3] after undo");
	}

	[Test]
	public void AppendEmptyUndoTest()
	{
		ByteBuffer bb = new ByteBuffer();
		byte[] ba = {0x01, 0x02, 0xfe, 0xff};
		byte[] baEmpty = new byte[0];

		bb.Append(ba);
		bb.Append(baEmpty);

		Assert.AreEqual(4, bb.Size, "#Size before undo");
		Assert.AreEqual(0x01, bb[0], "#bb[0] before undo");
		Assert.AreEqual(0x02, bb[1], "#bb[1] before undo");
		Assert.AreEqual(0xfe, bb[2], "#bb[2] before undo");
		Assert.AreEqual(0xff, bb[3], "#bb[3] before undo");

		bb.Undo();
		Assert.AreEqual(4, bb.Size, "#Size after undo");
		Assert.AreEqual(0x01, bb[0], "#bb[0] after undo");
		Assert.AreEqual(0x02, bb[1], "#bb[1] after undo");
		Assert.AreEqual(0xfe, bb[2], "#bb[2] after undo");
		Assert.AreEqual(0xff, bb[3], "#bb[3] after undo");
	}

	[Test]
	public void ActionChainingTest()
	{
		ByteBuffer bb = new ByteBuffer();
		byte[] ba1 = {0x01, 0x04};
		byte[] ba2 = {0x02, 0x03};
		byte[] ba3 = {0x05, 0x06};
		byte[] ba4 = {0x07, 0x08};

		bb.BeginActionChaining();
		bb.Append(ba1);
		bb.Insert(1, ba2);
		bb.EndActionChaining();

		Assert.AreEqual(4, bb.Size, "#1");
		Assert.IsTrue(bb.CanUndo, "#2");
		bb.Undo();
		Assert.AreEqual(0, bb.Size, "#3");
		Assert.IsFalse(bb.CanUndo, "#4");

		bb.Redo();

		bb.BeginActionChaining();
		bb.Append(ba4);
		bb.Insert(4, ba3);
		bb.EndActionChaining();

		Assert.AreEqual(8, bb.Size, "#5");
		Assert.IsTrue(bb.CanUndo, "#6");

		bb.Undo();

		Assert.AreEqual(4, bb.Size, "#7");
		Assert.IsTrue(bb.CanUndo, "#8");
	}

	[Test]
	public void LimitedUndoTest()
	{
		ByteBuffer bb = new ByteBuffer();
		bb.MaxUndoActions = 3;

		byte[] ba1 = {0x01, 0x04};
		byte[] ba2 = {0x02, 0x03};
		byte[] ba3 = {0x05, 0x06};
		byte[] ba4 = {0x07, 0x08};


		bb.Append(ba1);
		bb.Insert(1, ba2);
		bb.Append(ba4);
		bb.Insert(4, ba3);

		Assert.AreEqual(true, bb.CanUndo, "#1");
		bb.Undo();
		Assert.AreEqual(true, bb.CanUndo, "#2");
		bb.Undo();
		Assert.AreEqual(true, bb.CanUndo, "#3");
		bb.Undo();
		Assert.AreEqual(false, bb.CanUndo, "#4");
		Assert.AreEqual(2, bb.Size, "#5");
		Assert.AreEqual(0x01, bb[0], "#6");
		Assert.AreEqual(0x04, bb[1], "#7");

		bb.Redo();
		bb.Redo();
		bb.Redo();
		Assert.AreEqual(8, bb.Size, "#8");
		bb.MaxUndoActions = 2;

		bb.Undo();
		Assert.AreEqual(6, bb.Size, "#8.5");
		Assert.AreEqual(true, bb.CanUndo, "#9");
		bb.Undo();
		Assert.AreEqual(false, bb.CanUndo, "#10");
		Assert.AreEqual(4, bb.Size, "#11");
	}
}

} // end namespace