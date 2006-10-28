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
public class ChangeBufferTests {

	[Test]
	public void AppendTest() {
		ChangeBuffer cb=new ChangeBuffer();
		byte[] ba={2,3};
		cb.Append(1);
		cb.Append(ba);
		Assert.AreEqual(1, cb[0]);
		Assert.AreEqual(2, cb[1]);
		Assert.AreEqual(3, cb[2]);
		Assert.AreEqual(0, cb[3]);
	}


	[Test]
	public void PutTest() {
		ChangeBuffer cb=new ChangeBuffer();
		byte[] ba={1,2,3,4,5};
		byte[] ba2={11,22,33};
		cb.Append(ba);
		try {
		cb.Put(1, ba2);
		cb.Put(3, ba2);
		}
		catch (Exception e) {
		}
		Assert.AreEqual(1, cb[0]);
		Assert.AreEqual(11, cb[1]);
		Assert.AreEqual(22, cb[2]);
		Assert.AreEqual(33, cb[3]);
		Assert.AreEqual(5, cb[4]);
	}

	[Test]
	public void GetTest() {
		ChangeBuffer cb=new ChangeBuffer();
		byte[] ba={1,2,3,4,5};
		
		cb.Append(ba);
		
		byte[] ba1=new byte[3];
		cb.Get(ba1, 1,3);
		Assert.AreEqual(3, ba1.Length);
		Assert.AreEqual(2, ba1[0]);
		Assert.AreEqual(3, ba1[1]);
		Assert.AreEqual(4, ba1[2]);	
	}

}

} // end namespace
