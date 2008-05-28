// created on 3/8/2005 at 5:07 PM
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
public class SimpleBufferTests {

	[Test]
	public void AppendTest() {
		SimpleBuffer sb = new SimpleBuffer();
		byte[] ba = {1, 2, 3};
		sb.Append(ba, 0, ba.Length);
		Assert.AreEqual(1, sb[0]);
		Assert.AreEqual(2, sb[1]);
		Assert.AreEqual(3, sb[2]);
		Assert.AreEqual(0, sb[3]);
	}


	[Test]
	public void ReadTest() {
		SimpleBuffer sb = new SimpleBuffer();
		byte[] ba = {1, 2, 3, 4, 5};

		sb.Append(ba, 0, ba.Length);

		byte[] ba1 = new byte[3];
		sb.Read(ba1, 0, 1, 3);
		Assert.AreEqual(3, ba1.Length);
		Assert.AreEqual(2, ba1[0]);
		Assert.AreEqual(3, ba1[1]);
		Assert.AreEqual(4, ba1[2]);
	}

}

} // end namespace
