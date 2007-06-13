// created on 1/9/2005 at 1:18 PM
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

using Bless.Util;
using Bless.Buffers;

namespace Bless.Tools.Find {

/// <summary>A simple, brute-force finder</summary>
class SimpleFindStrategy : IFindStrategy
{
	byte[] pattern;
	ByteBuffer buffer;
	long pos;

	public SimpleFindStrategy()
	{
		pattern = new byte[0];
	}

	public byte[] Pattern {
		set { pattern = (byte[])value.Clone(); }
		get { return pattern; }
	}

	public ByteBuffer Buffer {
		get { return buffer; }
		set { buffer = value; pos = 0; }
	}

	public long Position {
		get { return pos;}
		set { pos = value; }
	}

	public Range FindNext(long limit)
	{
		long len = buffer.Size;
		int plen = pattern.Length;
		int i = 0;

		if (limit > len)
			limit = len;

		while (pos < limit && i < plen) {
			if (buffer[pos] == pattern[i]) {
				i++;
			}
			else {
				if (i > 0)
					pos = pos - i + 1;
				i = 0;
			}
			pos++;
		}

		if (i == plen && plen > 0)
			return new Range(pos -plen, pos - 1);
		else
			return null;
	}

	public Range FindPrevious(long limit)
	{
		int plen = pattern.Length;
		int i = plen - 1;

		if (limit < 0)
			limit = 0;

		pos--;

		while (pos >= limit && i >= 0 ) {
			if (buffer[pos] == pattern[i]) {
				i--;
			}
			else {
				if (i < plen - 1)
					pos = pos + (plen - 1 - i) - 1;
				i = plen - 1;
			}
			pos--;
		}

		if (i == -1 && plen > 0)
			return new Range(pos + 1, pos + plen);
		else
			return null;
	}

	public Range FindNext()
	{
		return FindNext(long.MaxValue);
	}

	public Range FindPrevious()
	{
		return FindPrevious(0);
	}
}

} //end namespace
