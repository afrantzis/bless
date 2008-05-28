// created on 6/29/2005 at 7:05 PM
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
using System;

namespace Bless.Buffers
{

///<summary>
/// A simple and lightweight buffer implementing the Buffer interface 
///</summary>
public class SimpleBuffer : IBuffer
{
	byte[] data;
	
	public SimpleBuffer() 
	{
		data=new byte[0];
	}
	
	public void Insert(long pos, byte[] d, long index, long length) 
	{
		throw new NotImplementedException();
	}
	
	public long Read(byte[] ba, long index, long pos, long len) 
	{
		Array.Copy(data, pos, ba, index, len);
		return len;
	}
	
	public void Append(byte[] d, long index, long length) 
	{
		if (length == 0)
			return;
		
		if (data.Length > 0) {
			byte[] tmp = new byte[data.LongLength + length];
			data.CopyTo(tmp, 0);
			Array.Copy(d, index, tmp, data.LongLength, length);
			data = tmp;
		}
		else {
			data = new byte[length];
			Array.Copy(d, index, data, 0, length);
		}
	}
	
	
	public byte this[long index] {
		set {
			if (index >= data.LongLength)
				return;
			data[index]=value;	
		 }
		 
		get {
			if (index >= data.LongLength)
				return 0;
			return data[index];
		}
	}

	public long Size {
		get { return data.LongLength; }
	}
	
}

} // end namespace
 
