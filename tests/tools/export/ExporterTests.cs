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

using Bless.Tools.Export;
using Bless.Buffers;

namespace BlessTests.Export {

[TestFixture]
public class ExporterTests {

	[Test]
	public void AppendTest() {
		TextExportBuilder teb = new TextExportBuilder(new FileStream("skata", FileMode.Create, FileAccess.Write));

		InterpretedPatternExporter ipe = new InterpretedPatternExporter(teb);

		IBuffer buffer = new SimpleBuffer();
		buffer.Append(new byte[]{0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 16, 16, 17, 18, 19, 20});

		ipe.Pattern = "%A\"4\"%[%O\"4\"%: %E\"4\"e\"_\"p\"0x\"x\"|\"%] - %E\"2\"s\"*\"%%I\"2\"%\n";
		ipe.Export(buffer, 2, buffer.Size - 1);

	}

}

} // end namespace
