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
public class ByteArrayTests
{
	[Test]
	public void ByteArrayHexTest1()
	{
		byte[] ba=ByteArray.FromString("0305ffFa66", 16);
		
		Assert.AreEqual(5, ba.Length,"#Length");
		Assert.AreEqual(0x03, ba[0],"#0");
		Assert.AreEqual(0x05, ba[1],"#1");
		Assert.AreEqual(0xff, ba[2],"#2");
		Assert.AreEqual(0xfa, ba[3],"#3");
		Assert.AreEqual(0x66, ba[4],"#4");
			
	}
	
	[Test]
	public void ByteArrayHexTest2()
	{
		byte[] ba=ByteArray.FromString(" 63a5 fe DD Cb  ", 16);
		
		Assert.AreEqual(5, ba.Length,"#Length");
		Assert.AreEqual(0x63, ba[0],"#0");
		Assert.AreEqual(0xa5, ba[1],"#1");
		Assert.AreEqual(0xfe, ba[2],"#2");
		Assert.AreEqual(0xdd, ba[3],"#3");
		Assert.AreEqual(0xcb, ba[4],"#4");
			
	}
	
	[Test] 
	public void ByteArrayHexTest3()
	{
		byte[] ba=ByteArray.FromString(" 63a fe DDCb  ", 16);
		
		Assert.AreEqual(5, ba.Length,"#Length");
		Assert.AreEqual(0x63, ba[0],"#0");
		Assert.AreEqual(0x0a, ba[1],"#1");
		Assert.AreEqual(0xfe, ba[2],"#2");
		Assert.AreEqual(0xdd, ba[3],"#3");
		Assert.AreEqual(0xcb, ba[4],"#4");
	}
	
	[Test]
	[ExpectedException(typeof(FormatException))]
	public void ByteArrayHexTest4()
	{
		ByteArray.FromString(" 63 a5 fg DD Cb  ", 16);	
	}
	
	[Test]
	public void ByteArrayHexTest5()
	{
		string s=ByteArray.ToString(new byte[]{0x63, 0xff,0x12,0x00,0xca}, 16);
		
		Assert.AreEqual(14, s.Length,"#Length");
		Assert.AreEqual(s, "63 ff 12 00 ca","#0");	
	}
	
}

} // end namespace
