// created on 2/13/2005 at 7:02 PM
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
using System.Collections.Generic;

namespace Bless.Buffers {

///<summary>Abstract class for ByteBuffer actions</summary>
abstract class ByteBufferAction {
	abstract public void Do();
	abstract public void Undo();
	virtual public void MakePrivateCopyOfData() { }
}

///<summary>Action for appending at the end of a ByteBuffer</summary>
class AppendAction: ByteBufferAction {

	Segment seg;
	ByteBuffer byteBuf;
	
	public AppendAction(byte[] d, long index, long length, ByteBuffer bb) {
		byteBuf = bb;
		
		// if there is no data to append
		// don't create a segment
		if (d.Length == 0)
			seg = null;
		else {
			SimpleBuffer cb = new SimpleBuffer();
			seg = new Segment(cb, cb.Size, cb.Size + length - 1);
			
			cb.Append(d, index, length);
		}
	}
	
	public override void Do() {
		if (seg == null)
			return;
		// use copy of segment seg to protect it from alterations 
		byteBuf.segCol.Append(new Segment(seg.Buffer, seg.Start, seg.End));
		byteBuf.size += seg.Size;
	}
	
	public override void Undo() {
		if (seg == null)
			return;
		byteBuf.size -= seg.Size;
		byteBuf.segCol.DeleteRange(byteBuf.size, byteBuf.size + seg.Size - 1);
	}
}

///<summary>Action for inserting in a ByteBuffer</summary>
class InsertAction: ByteBufferAction {

	Segment seg;
	long pos;
	ByteBuffer byteBuf;
	
	public InsertAction(long p, byte[] d, long index, long length, ByteBuffer bb) {
		byteBuf = bb;
		pos = p;
		
		// if there is no data to insert
		// don't create a segment
		if (length == 0)
			seg = null;
		else {
			SimpleBuffer cb = new SimpleBuffer();
			seg = new Segment(cb, cb.Size, cb.Size + length - 1);
			cb.Append(d, index, length);
		}
	}
	
	public override void Do() {
		if (seg == null)
			return;
		SegmentCollection tmp = new SegmentCollection();
		// use copy of segment seg to protect it from alterations
		tmp.Append(new Segment(seg.Buffer, seg.Start, seg.End));
		byteBuf.segCol.Insert(tmp, pos);
		byteBuf.size += seg.Size;
	}
	
	public override void Undo() {
		if (seg == null)
			return;
		byteBuf.segCol.DeleteRange(pos, pos + seg.Size - 1);
		byteBuf.size -= seg.Size;
	}

}

///<summary>Action for deleting from a ByteBuffer</summary>
class DeleteAction: ByteBufferAction {

	SegmentCollection del;
	long pos1, pos2;
	ByteBuffer byteBuf;
	
	public DeleteAction(long p1, long p2, ByteBuffer bb) {
		byteBuf = bb;
		pos1 = p1;
		pos2 = p2;
	}
	
	public override void Do() {
		del = byteBuf.segCol.DeleteRange(pos1, pos2);
		byteBuf.size -= pos2 - pos1 + 1;
	}
	
	public override void Undo() {
		byteBuf.segCol.Insert(del, pos1);
		byteBuf.size += pos2 - pos1 + 1;
	}
	
	public override void MakePrivateCopyOfData()
	{
		foreach(Segment seg in del.List) {
			
			if (seg.Buffer.GetType() == typeof(FileBuffer)) {
				seg.MakePrivateCopyOfData();
			}
		}
	}
	
}

///<summary>Convenience action for replacing data in ByteBuffer</summary>
class ReplaceAction: ByteBufferAction {

	DeleteAction del;
	InsertAction ins;
	
	public ReplaceAction(long p1, long p2, byte[] d, long index, long length, ByteBuffer bb) {
		del = new DeleteAction(p1, p2, bb);
		ins = new InsertAction(p1, d, index, length, bb);
	}
	
	public override void Do() {
		del.Do();
		ins.Do();
	}
	
	public override void Undo() {
		ins.Undo();
		del.Undo();
	}
}

///<summary>Container fo many related ByteBufferActions</summary>
class MultiAction: ByteBufferAction {

	List<ByteBufferAction> list;
	
	public MultiAction()
	{ 
		list = new List<ByteBufferAction>();
	}
	
	public void Add(ByteBufferAction action)
	{
		list.Add(action);
	}
	
	public override void Do()
	{
		// do the actions in normal order
		for(int i = 0; i < list.Count; i++)
			list[i].Do();
	}
	
	public override void Undo()
	{
		// undo the actions in reverse order
		for(int i = list.Count - 1; i >= 0; i--)
			list[i].Undo();
	}
	
	public override void MakePrivateCopyOfData()
	{
		foreach(ByteBufferAction a in list) {
			a.MakePrivateCopyOfData();
		}
	}
}


} // end namespace
