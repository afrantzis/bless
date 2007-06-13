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
using System.Collections;
using Bless.Buffers;

namespace Bless.Tools.Export
{

public struct BuildBytesInfo
{
	public int Count;
	public char Type;
	public string Prefix;
	public string Suffix;
	public string Separator;
	public string Empty;
	public Hashtable Commands;
}

///<summary>
/// A builder that is used to export bytes
///</summary>
public interface IExportBuilder
{
	void BuildPrologue();
	void BuildEpilogue();
	int BuildBytes(IBuffer buffer, long offset, BuildBytesInfo info);
	void BuildString(string str);
	void BuildCharacter(char c);
	void BuildAlignment(int align);
	void BuildOffset(long offset, int length, char type);

	Stream OutputStream {
		get;
		}
	}



}
