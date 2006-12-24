// created on 12/5/2006 at 2:23 PM
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

using System.IO;
using Bless.Buffers;
using Bless.Util;
using Bless.Tools.Export;

namespace Bless.Tools.Export.Plugins
{

///<summary>
/// A builder that exports bytes to HTML
///</summary>
public class HTMLExportBuilder : TextExportBuilder
{
	
	public HTMLExportBuilder(Stream stream)
	: base(stream)
	{ 
	}
	
	public override void BuildPrologue()
	{
		System.Console.WriteLine("Writing prologue");
		BuildString("<html><body><pre>");
	}
	
	public override void BuildEpilogue()
	{
		System.Console.WriteLine("Writing epilogue");
		BuildString("</pre></body></html>");
	}
	
	protected override int BuildByte(IBuffer buf, long offset, BuildBytesInfo info, bool dummy)
	{
		if (offset % 2 == 0)
			BuildString("<span style=\"color:blue\">");
			
		int n = base.BuildByte(buf, offset, info, dummy); 
	
		if (offset % 2 == 0)
			BuildString("</span>");
		return n;
	}
}

} //end namespace