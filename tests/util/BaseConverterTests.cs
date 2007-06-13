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
public class BaseConverterTests
{
	[Test]
	public void BaseConverterTest1()
	{
		string s2 = BaseConverter.ConvertToString(15464, 2);
		string s8 = BaseConverter.ConvertToString(15464, 8);
		string s10 = BaseConverter.ConvertToString(15464, 10);
		string s16 = BaseConverter.ConvertToString(15464, 16);

		Assert.AreEqual("11110001101000", s2, "#base2");
		Assert.AreEqual("36150", s8, "#base8");
		Assert.AreEqual("15464", s10, "#base10");
		Assert.AreEqual("3c68", s16, "#base16");

	}

	[Test]
	public void BaseConverterTest2()
	{
		long l2 = BaseConverter.ConvertToNum("11110001101000", 2);
		long l8 = BaseConverter.ConvertToNum("36150", 8);
		long l10 = BaseConverter.ConvertToNum("15464", 10);
		long l16 = BaseConverter.ConvertToNum("3c68", 16);

		Assert.AreEqual(15464, l2, "#base2");
		Assert.AreEqual(15464, l8, "#base8");
		Assert.AreEqual(15464, l10, "#base10");
		Assert.AreEqual(15464, l16, "#base16");
	}

	[Test]
	[ExpectedException(typeof(FormatException))]
	public void BaseConverterTest3()
	{
		BaseConverter.ConvertToNum("18032", 8);
	}

	[Test]
	[ExpectedException(typeof(FormatException))]
	public void BaseConverterTest4()
	{
		BaseConverter.ConvertToNum("a04zd", 16);
	}

	[Test]
	public void BaseConverterTest5()
	{
		long l16 = BaseConverter.Parse("  0x66  ");
		long l10 = BaseConverter.Parse("  66  ");
		long l8 = BaseConverter.Parse("  066  ");

		Assert.AreEqual(0x66, l16, "#base16");
		Assert.AreEqual(66, l10, "#base10");
		Assert.AreEqual(54, l8, "#base8");
	}
}

} // end namespace
