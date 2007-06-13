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
using System;
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
		for (int i = 0; i < count; i++) {
			if (i != 0)
				BuildSeparator(info.Separator);

			nwritten += BuildByte(buffer, offset + i, info, false);

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

	public virtual void BuildOffset(long offset, int length, char type)
	{
		long lowAlign = (offset / alignment) * alignment;
		bool lowercase;
		int numBase = GetBaseFromArgument(type, out lowercase);
		string str = BaseConverter.ConvertToString(lowAlign, numBase, false, lowercase, length);

		BuildByteData(str);
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
		for (long i = lowAlign; i < offset; i++) {
			if (i != lowAlign)
				BuildSeparator(info.Separator);

			BuildByte(null, 0, info, true);

			count++;
		}

		// if alignment adjustments had to be made
		// emit one more separator (eg "__ __ " <-- notice last space)
		if (lowAlign != offset)
			writer.Write(info.Separator);

		return count;
	}

	protected virtual int BuildByte(IBuffer buffer, long offset, BuildBytesInfo info, bool dummy)
	{
		string str;
		int nwritten = 0;
		byte b = 0;

		if (dummy == true || offset >= buffer.Size) {
			dummy = true;
		}
		else {
			nwritten++;
			b = buffer[offset];
		}

		if (info.Type == 'A')
			str = info.Type.ToString();
		else {
			bool lowercase;
			int numBase = GetBaseFromArgument(info.Type, out lowercase);
			str = BaseConverter.ConvertToString(b, numBase, false, lowercase, 0);
		}

		if (!dummy && info.Prefix != null)
			BuildPrefix(info.Prefix);
		else
			BuildEmpty(info.Prefix, " ");

		if (!dummy && str != null) {
			BuildByteData(str);
		}
		else if (info.Empty != null)
			BuildEmpty(str, info.Empty);
		else
			BuildEmpty(str, " ");

		if (!dummy && info.Suffix != null)
			BuildSuffix(info.Suffix);
		else
			BuildEmpty(info.Suffix, " ");

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

		for (int i = str.Length; i > 0;) {
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

	private int GetBaseFromArgument(char type, out bool lowercase)
	{
		int numBase = 0;
		lowercase = false;

		switch (type) {
			case 'H':
				numBase = 16;
				break;
			case 'h':
				numBase = 16;
				lowercase = true;
				break;
			case 'D':
				numBase = 10;
				break;
			case 'O':
				numBase = 8;
				break;
			case 'B':
				numBase = 2;
				break;
			default:
				break;
		}

		return numBase;
	}

}

} //end namespace