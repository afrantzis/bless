// created on 3/8/2005 at 5:11 PM
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
public class FileBufferTests {
	
	[Test]
	public void LoadFile() {
		FileBuffer fb=new FileBuffer("test1.bin");
		byte[] baExpect={0x3c,0x13,0xe3,0x36,0xcc,0x66,0x21,0xda};
		byte[] ba=new byte[baExpect.Length];
		fb.Get(ba, 0x3f0, baExpect.Length);
		
		Assert.IsNotNull(ba,"#1");
		for(int i=0;i<baExpect.Length;i++) 
			Assert.AreEqual(baExpect[i], ba[i]);
	}
	
	[Test]
	public void IndexerAccessTest() {
		FileBuffer fb=new FileBuffer("test1.bin");
		Assert.IsNotNull(fb,"#1");
		
		long size=fb.Size;
		int sum=0;
		
		for(int i=0; i<size; i++) 
			sum ^=fb[i];
	
		Assert.AreEqual( 0x88, sum);
		
	}
	
}

} // end namespace

