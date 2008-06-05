// created on 2/24/2008 at 4:04 PM
/*
 *   Copyright (c) 2008, Alexandros Frantzis (alf82 [at] freemail [dot] gr)
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
using System.Threading;
using Bless.Util;
using Mono.Unix;

namespace Bless.Buffers {

///<summary>
/// Saves the contents of a ByteBuffer using an asynchronous threaded model.
/// This class saves the file in place and can only be used if the file size
/// has not been changed. It works by just saving the changed parts.
///</summary>
public class SaveInPlaceOperation : ThreadedAsyncOperation, ISaveState
{
	protected ByteBuffer byteBuffer;
	protected long bytesSaved;
	
	protected string savePath;
	FileStream fs;
	
	SaveInPlaceStage stageReached;
	
	public ByteBuffer Buffer {
		get {return byteBuffer;}
	}
	
	public string SavePath {
		get { return savePath; }
		set { savePath = value; }
	}
	
	public long BytesSaved {
		get {return bytesSaved;}
	}
	
	public enum SaveInPlaceStage { BeforeClose, BeforeWrite }
	
	public SaveInPlaceStage StageReached {
		get { return stageReached; }
	}
	
	public SaveInPlaceOperation(ByteBuffer bb, ProgressCallback pc,
							AsyncCallback ac, bool glibIdle): base(pc, ac, glibIdle)
	{
		byteBuffer = bb;
		savePath = byteBuffer.Filename;
		fs = null;
		bytesSaved = 0;
	}
	
	protected override bool StartProgress()
	{
		progressCallback(string.Format(Catalog.GetString("Saving '{0}'"), SavePath), ProgressAction.Message);
		return progressCallback(((double)bytesSaved) / byteBuffer.Size, ProgressAction.Show);
	}
	
	protected override bool UpdateProgress()
	{
		return progressCallback(((double)bytesSaved) / byteBuffer.Size, ProgressAction.Update);
	}
	
	protected override bool EndProgress()
	{
		return progressCallback(((double)bytesSaved) / byteBuffer.Size, ProgressAction.Destroy);
	}
	
	protected override void DoOperation()
	{
		stageReached = SaveInPlaceStage.BeforeClose;
		
		// hold a reference to the bytebuffer's segment collection
		// because it is lost when the file is closed
		SegmentCollection segCol = byteBuffer.segCol;
		
		// close file
		if (!cancelled) {
			// close the file, make sure that File Operations
			// are temporarily allowed
			lock(byteBuffer.LockObj) {
				// CloseFile invalidates the file buffer,
				// so make sure undo/redo data stays valid
				byteBuffer.MakePrivateCopyOfUndoRedo();
				byteBuffer.FileOperationsAllowed = true;
				byteBuffer.CloseFile();
				byteBuffer.FileOperationsAllowed = false;
			}
		}
		
		// Open the file for editing
		fs = new FileStream(savePath, FileMode.Open, FileAccess.Write);
		
		stageReached = SaveInPlaceStage.BeforeWrite;
		
		const int blockSize = 0xffff;
		
		byte[] baTemp = new byte[blockSize];
		
		//
		// Just save the changed parts...
		//
		
		Util.List<Segment>.Node node = segCol.List.First;
		
		// hold the mapping of the start of the current segment
		// (at which offset in the file it is mapped)
		long mapping = 0;
		
		while (node != null && !cancelled)
		{
			// Save the data in the node 
			// in blocks of blockSize each
			Segment s = node.data;
			
			// if the segment belongs to the original buffer, skip it
			if (s.Buffer.GetType() == typeof(FileBuffer)) {
				mapping += s.Size;
				node = node.next;
				continue;
			}
				
			long len = s.Size;
			long nBlocks = len/blockSize;
			int last = (int)(len % blockSize); // bytes in last block
			long i;
			
			// move the file cursor to the current mapping
			fs.Seek(mapping, SeekOrigin.Begin);
			
			bytesSaved = mapping;
			
			// for every full block
			for (i = 0; i < nBlocks; i++) {
				s.Buffer.Read(baTemp, 0, s.Start + i * blockSize, blockSize);	
				fs.Write(baTemp, 0, blockSize);
				bytesSaved = (i + 1) * blockSize;
				
				if (cancelled)
					break;	
			}
		
			// if last non-full block is not empty
			if (last != 0 && !cancelled) {
				s.Buffer.Read(baTemp, 0, s.Start + i * blockSize, last);
				fs.Write(baTemp, 0, last);
			}
			
			mapping += s.Size;
			node = node.next;
		}	
		
		fs.Close();
		fs = null;	
	}
	
	protected override void EndOperation()
	{
		if (fs != null) {
			fs.Close();
			fs = null;
		}
	}
}

} // end namespace
