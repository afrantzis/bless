// created on 6/4/2004 at 3:37 PM
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

using System;

namespace Bless.Buffers {


public interface IBuffer {

	void Insert(long pos, byte[] data, long index, long length);
	long Read(byte[] data, long index, long pos, long len);
	void Append(byte[] data, long index, long length);
	
	void InsertBuffer(long pos, IBuffer buf, long index, long length);
	void AppendBuffer(IBuffer buf, long index, long length);

	byte this[long index] {
		set;
		get;
	}
	
	long Size {
		get;
	}
}


public class BaseBuffer : IBuffer
{
	public virtual void Insert(long pos, byte[] data, long index, long length)
	{
		throw new NotImplementedException();
	}

	public virtual long Read(byte[] data, long index, long pos, long len)
	{
		throw new NotImplementedException();
	}

	public virtual void Append(byte[] data, long index, long length)
	{
		throw new NotImplementedException();
	}

	public virtual void InsertBuffer(long pos, IBuffer buf, long index, long length)
	{
		throw new NotImplementedException();
	}

	public virtual void AppendBuffer(IBuffer buf, long index, long length)
	{
		throw new NotImplementedException();
	}

	public virtual byte this[long index] {
		set { throw new NotImplementedException(); }
		get { throw new NotImplementedException(); }
	}
	
	public virtual long Size {
		get { throw new NotImplementedException(); }
	}
}

}
