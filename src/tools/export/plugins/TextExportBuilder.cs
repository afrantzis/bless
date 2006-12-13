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
/// A builder that exports bytes to text
///</summary>
public class TextExportBuilder : IExportBuilder
{
	protected StreamWriter writer;
	protected int alignment;
	
	public TextExportBuilder(Stream stream) 
	{ 
		writer = new StreamWriter(stream);
		alignment = 1;
	}
	
	public virtual void BuildPrologue()
	{
	}
	
	public virtual void BuildEpilogue()
	{
	}
	
	public virtual int BuildBytes(IBuffer buffer, long offset, BuildBytesInfo info)
	{
		int nwritten = 0;
		int count = info.Count;
		
		count -= MatchAlignment(offset, info);
		
		// emit the bytes (eg "__ __ 00 00")
		for(int i = 0; i < count; i++) {
			if (i != 0)
				BuildSeparator(info.Separator);
			
			nwritten += BuildByte(buffer, offset + i, info.Prefix, info.Suffix, info.Empty, false);
			
		}
		
		writer.Flush();
		return nwritten;
	}
	
	public virtual void BuildString(string str)
	{
		writer.Write(str);
		writer.Flush();
	}
	
	public virtual void BuildCharacter(char c)
	{
		writer.Write(c);
		writer.Flush();
	}
	
	public virtual void BuildAlignment(int align)
	{
		alignment = align;
	}
	
	public virtual void BuildOffset(long offset, int length) 
	{
		long lowAlign = (offset / alignment) * alignment;
		
		BuildByteData(BaseConverter.ConvertToString(lowAlign, 16, false, length));
	}
	
	public Stream OutputStream {
		get {
			return writer.BaseStream;
		}
	}
	
	
	protected virtual int MatchAlignment(long offset, BuildBytesInfo info)
	{
		int count = 0;
		// match alignment
		// find lower alignment offset
		long lowAlign = (offset / alignment) * alignment;
		
		// fill with blanks (eg "__ __")
		for(long i = lowAlign; i < offset; i++) {
			if (i != lowAlign)
				BuildSeparator(info.Separator);
			
			BuildByte(null, 0,
				info.Prefix, info.Suffix, info.Empty, true);
			
			count++;
		}
		
		// if alignment adjustments had to be made
		// emit one more separator (eg "__ __ " <-- notice last space)
		if (lowAlign != offset)
			writer.Write(info.Separator);
		
		return count;
	}
	
	protected virtual int BuildByte(IBuffer buffer, long offset, string prefix, string suffix, string empty, bool dummy)
	{
		string str;
		int nwritten = 0;
		
		if (dummy == true || offset >= buffer.Size) {
			dummy = true;
			str = BaseConverter.ConvertToString(0, 16);
		}
		else {
			nwritten++;
			str = BaseConverter.ConvertToString(buffer[offset], 16);
		}
			
		if (!dummy && prefix != null)
			BuildPrefix(prefix);
		else
			BuildEmpty(prefix, " ");
			
		if (!dummy && str != null) {
			BuildByteData(str);
		}
		else if (empty != null)
			BuildEmpty(str, empty);
		else 
			BuildEmpty(str, " ");
		
		if (!dummy && suffix != null)
			BuildSuffix(suffix);
		else
			BuildEmpty(suffix, " ");

		return nwritten;
	}
	
	protected virtual void BuildSeparator(string separator)
	{
		writer.Write(separator);
	}
	
	protected virtual void BuildEmpty(string str, string empty)
	{
		if (str == null)
			return;
			
		for (int i=str.Length; i > 0;) {
			if (empty.Length <= i) {
				writer.Write(empty);
				i -= empty.Length;
			}
			else {
				writer.Write(empty.Substring(0, i));
				i = 0;
			}
		}
	}
	
	protected virtual void BuildPrefix(string str)
	{
		writer.Write(str);
	}
	
	protected virtual void BuildSuffix(string str)
	{
		writer.Write(str);
	}
	
	protected virtual void BuildByteData(string str)
	{
		writer.Write(str);
	}
	
	
	
}

} //end namespace