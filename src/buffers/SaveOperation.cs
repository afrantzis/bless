// created on 4/5/2005 at 1:04 PM
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
using System.IO;
using System.Threading;
using Bless.Util;

using Mono.Unix;

namespace Bless.Buffers {

///<summary>
/// Saves the contents of a ByteBuffer using an asynchronous threaded model
///</summary>
public class SaveOperation :  ThreadedAsyncOperation, ISaveState
{

	protected ByteBuffer byteBuffer;
	protected long bytesSaved;
	
	protected string savePath;
	
	string tempPath;
	SaveStage stageReached;

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
	
	public string TempPath {
		get { return tempPath;}
	}
	
	public enum SaveStage { BeforeSaveAs, BeforeDelete, BeforeMove }
	
	public new SaveStage StageReached {
		get { return stageReached; }
	}
	
	public SaveOperation(ByteBuffer bb, string tempFilename, ProgressCallback pc,
							AsyncCallback ac, bool glibIdle)
							: base(pc, ac, glibIdle)
	{
		byteBuffer = bb;
		savePath = byteBuffer.Filename;
		tempPath = tempFilename;
		bytesSaved = 0;
		activateProgress = false;
	}
	
	protected bool CheckFreeSpace(string path, long extraSpace)
	{
		try {
			long freeSpace = Portable.GetAvailableDiskSpace(path);
			//System.Console.WriteLine("CFS {0}: {1}+{2} {3}", path, freeSpace, extraSpace, byteBuffer.Size);

			return (freeSpace + extraSpace >= byteBuffer.Size);
		}
		catch (NotImplementedException) {
			return true;	
		}

	}

	protected override bool StartProgress()
	{
		progressCallback(string.Format(Catalog.GetString("Moving '{0}' to '{1}'"), tempPath, savePath), ProgressAction.Message);
		return progressCallback(((double)bytesSaved)/byteBuffer.Size, ProgressAction.Show);
	}
	
	protected override bool UpdateProgress()
	{
		return progressCallback(((double)bytesSaved)/byteBuffer.Size, ProgressAction.Update);
	}
	
	protected override bool EndProgress()
	{
		return progressCallback(((double)bytesSaved)/byteBuffer.Size, ProgressAction.Destroy);
	}
	
	protected override void DoOperation()
	{
		SaveAsOperation sao = new SaveAsOperation(byteBuffer, tempPath, this.progressCallback, null, false);
		stageReached = SaveStage.BeforeSaveAs;

		// Check for free space for final file
		// free space for temporary file is checked in sao.DoOperation()
		if (!CheckFreeSpace(Path.GetDirectoryName(byteBuffer.Filename), byteBuffer.fileBuf.Size)) {
			string msg = string.Format(Catalog.GetString("There is not enough free space on the device to save file '{0}'."), byteBuffer.Filename);
			throw new IOException(msg);
		}

		// Save ByteBuffer as a temp file
		sao.OperationThread();

		if (sao.Result == ThreadedAsyncOperation.OperationResult.CaughtException)
			throw sao.ThreadException;
		else if (sao.Result == ThreadedAsyncOperation.OperationResult.Cancelled)	
			cancelled = true;

		// if user hasn't cancelled, move temp file to 
		// its final location	
		if (!cancelled) {
			this.ActivateProgressReport(true);
			stageReached = SaveStage.BeforeDelete;
			
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
			
			if (System.IO.File.Exists(savePath))
				System.IO.File.Delete(savePath);
			
			stageReached = SaveStage.BeforeMove;
			System.IO.File.Move(tempPath, savePath);
		}	
	}
	
	protected override void EndOperation()
	{
	}
}

} // end namespace
