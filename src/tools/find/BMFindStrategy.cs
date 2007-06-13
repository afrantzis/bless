// created on 2/15/2005 at 12:32 PM
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

/// <summary>A Boyer-Moore finder utilizing only skip-tables (bad-character shifts)</summary>
public class BMFindStrategy : IFindStrategy
{
	byte[] pattern;
	ByteBuffer buffer;
	long pos;
	bool cancelled;

	int[] skipTable;
	int[] skipTableReverse;

	///<summary>
	/// Create the forward and reverse skip tables
	/// for boyer-moore based searches
	///</summary>
	void UpdateSkipTables()
	{
		int len = skipTable.Length;
		int patLen = pattern.Length;

		for (int i = 0; i < len; i++) {
			skipTable[i] = patLen;
			skipTableReverse[i] = patLen;
		}

		for (int i = 0; i < patLen;i++)
			skipTable[pattern[i]] = patLen - i - 1;

		for (int i = patLen - 1; i >= 0;i--)
			skipTableReverse[pattern[i]] = i;
	}

	public BMFindStrategy()
	{
		pattern = new byte[0];
		skipTable = new int[256];
		skipTableReverse = new int[256];
	}

	public byte[] Pattern {
		set {
			pattern = (byte[])value.Clone();
			UpdateSkipTables();
		}

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

	public bool Cancelled { get { return cancelled; } set { cancelled = value;} }

	///<summary>Simple method to search forwards for a single byte</summary>
	Range FindNextSingle(long limit)
	{
		byte b = pattern[0];

		while ((pos < limit) && buffer[pos] != b && !cancelled) pos++;

		if (pos >= limit || cancelled)
			return null;
		else {
			Range r = new Range(pos, pos);
			pos++; // next time start search from next byte
			return r;
		}
	}

	///<summary>Simple method to search backwards for a single byte</summary>
	Range FindPreviousSingle(long limit)
	{
		byte b = pattern[0];

		// start the search at the previous position
		pos--;

		while ((pos >= limit) && buffer[pos] != b && !cancelled) pos--;

		if (pos < 0 || cancelled) {
			// make sure pos is not negative
			if (pos < 0) pos = 0;
			return null;
		}
		else {
			Range r = new Range(pos, pos);
			return r;
		}
	}

	public Range FindNext(long limit)
	{
		int patLen = pattern.Length;
		long bufLen = buffer.Size;
		byte curByte = 0;
		int i = 0;

		// adjustment: search up to and including limit...
		limit++;

		// make sure limit is sane
		if (limit > bufLen)
			limit = bufLen;

		// if pattern is empty don't bother searching...
		if (patLen == 0)
			return null;

		// use simple method for 1 byte searches
		if (patLen == 1)
			return FindNextSingle(limit);

		// while buffer has more bytes to search
		while (pos <= limit - patLen && !cancelled) {
			// while bytes match and more bytes are left in the pattern, continue...
			for (i = patLen - 1; i >= 0; --i) {
				curByte = buffer[pos+i];
				if (curByte != pattern[i]) break;
			}

			// if we found a match
			if (i < 0) {
				Range r = new Range(pos, pos + patLen - 1);
				pos++; // next time start search from next byte
				return r;
			}
			else {// shift
				int t = skipTable[curByte];

				// can we use the skip value?
				if (patLen - i > t) {
					// ...no, because the "text" byte that caused the mismatch
					// has already been matched in the pattern and the skipTable[]
					// holds info only for the rightmost occurence of a byte.
					// eg TEXT: abcdcg -> The c causes the mismatch
					//     PAT: aafdcg    but has already been matched
					// SAFE SOLUTION: move the pattern one position to the right and continue
					// BETTER: use the complete BM algorithm with good-suffix shift
					pos += patLen - i;
				}
				else // ...yes
					pos += t - patLen + 1 + i;
			} // end shift
		}

		return null;
	}

	// this is like FindNext(), only we are searching backwards
	// so everything is reversed
	public Range FindPrevious(long limit)
	{
		int patLen = pattern.Length;
		byte curByte = 0;
		int i;

		// make sure limit is sane
		if (limit < 0)
			limit = 0;

		// if pattern is empty don't bother searching...
		if (patLen == 0)
			return null;

		// use simple method for 1 byte searches
		if (patLen == 1)
			return FindPreviousSingle(limit);

		// start the search at the previous position
		pos--;

		// while buffer has more bytes to search
		while (pos >= limit + patLen - 1 && !cancelled) {
			// while bytes match and more bytes are left in the pattern, continue...
			for (i = 0; i < patLen; ++i) {
				curByte = buffer[pos-patLen+1+i];
				if (curByte != pattern[i]) break;
			}

			// if we found a match
			if (i >= patLen) {
				Range r = new Range(pos - patLen + 1, pos);
				return r;
			}
			else {// shift
				int t = skipTableReverse[curByte];

				// can we use the skip value?
				if (i > t) {
					// ...no, because the "text" byte that caused the mismatch
					// has already been matched in the pattern and the skipTableReverse[]
					// holds info only for the leftmost occurence of a byte.
					// eg TEXT: abcdcg -> The c causes the mismatch
					//     PAT: abcdfg    but has already been matched
					// SAFE SOLUTION: move the pattern one position to the left and continue
					// BETTER: use the complete BM algorithm with good-suffix shift
					pos--;
				}
				else // ...yes
					pos -= t - i;
			} // end shift
		}

		// make sure pos is not negative
		if (pos < 0) pos = 0;

		return null;
	}

	public Range FindNext()
	{
		return FindNext(long.MaxValue -1);
	}

	public Range FindPrevious()
	{
		return FindPrevious(0);
	}
}

} //end namespace
