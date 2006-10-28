// created on 6/5/2004 at 4:00 PM
/*
 *   Copyright (c) 2004, Alexandros Frantzis (alf82 [at] freemail [dot] gr)
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
using System.Collections;
using System;

namespace Bless.Buffers
{

public class ChangeBuffer : Buffer  {

	ArrayList data;
	
	public ChangeBuffer() 
	{
		data = new ArrayList();
	}
	
	public override void Put(long pos, byte[] d) 
	{
		//if (pos < data.Count && pos+d.Length < data.Count)
			data.SetRange((int)pos, d);
	}
	
	public override int Get(byte[] ba, long pos, int len) 
	{
		if (pos >= data.Count || pos+len > data.Count)
			return 0;
		data.CopyTo((int)pos, ba, 0, (int)len);
		return len;
	}
	
	public override void Append(byte[] d) 
	{ 
			data.AddRange(d);
	}
	
	public override void Append(byte b) 
	{
			data.Add(b);		
	}
	
	public override byte this[long index] {
		set {
			if (index >= data.Count)
				return;
			data[(int)index]=value;	
		 } 
		get {
			if (index >= data.Count)
				return 0;
			return (byte)data[(int)index];
		}
	}

	public override long Size {
		get { return data.Count; }
	}
}

} // end namespace