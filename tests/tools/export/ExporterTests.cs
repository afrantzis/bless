/*
 *   Copyright (c) 2006, Alexandros Frantzis (alf82 [at] freemail [dot] gr)
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
using System.IO;
using System.Text;

using Bless.Tools.Export;
using Bless.Tools.Export.Plugins;
using Bless.Buffers;

namespace BlessTests.Export {

[TestFixture]
public class ExporterTests {

	[Test]
	public void AppendTest() {
		string correct = "0000:   __|  __|0x02|0x03 -       \n" +
						"0004: 0x04|0x05|0x06|0x07 - 04*05*\n" +
						"0008: 0x08|0x09|0x0A|0x0B - 08*09*\n" +
						"000C: 0x0C|0x0D|0x0E|0x10 - 0C*0D*\n" +
						"0010: 0x10|0x11|0x12|0x13 - 10*11*\n" +
						"0014: 0x14|  __|  __|  __ - 14*   \n";

		MemoryStream ms = new MemoryStream();
		TextExportBuilder teb = new TextExportBuilder(ms);

		InterpretedPatternExporter ipe = new InterpretedPatternExporter(teb);

		IBuffer buffer = new SimpleBuffer();
		buffer.Append(new byte[]{0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 16, 16, 17, 18, 19, 20}, 0, 21);

		ipe.Pattern = "%A\"4\"%[%O\"4\"%: %E\"4\"e\"_\"p\"0x\"x\"|\"%] - %E\"2\"s\"*\"%%I\"2\"%\n";
		bool cancelled = false;
		ipe.Export(buffer, 2, buffer.Size - 1, ref cancelled);

		Assert.AreEqual(correct, ASCIIEncoding.ASCII.GetString(ms.ToArray()), "#1"); 
	}

}

} // end namespace
