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
public class SaveOperation : SaveAsOperation
{

	string tempPath;
	SaveStage stageReached;
	
	public string TempPath {
		get { return tempPath;}
	}
	
	public enum SaveStage { BeforeSaveAs, BeforeDelete, BeforeMove }
	
	public new SaveStage StageReached {
		get { return stageReached; }
	}
	
	public SaveOperation(ByteBuffer bb, string tempFilename, ProgressCallback pc,
							AsyncCallback ac, bool glibIdle)
							: base(bb, tempFilename, pc, ac, glibIdle)
	{
#if ENABLE_UNIX_SPECIFIC
		// get info about the device the file will be saved on
		FileInfo fi=new FileInfo(bb.Filename);
			
		Mono.Unix.Native.Statvfs stat=new Mono.Unix.Native.Statvfs();
		Mono.Unix.Native.Syscall.statvfs(bb.Filename, out stat);
			
		long freeSpace=(long)(stat.f_bavail*stat.f_bsize) + fi.Length;
			
		// make sure there is enough disk space in the device
		if (freeSpace < bb.Size) {
			if (System.IO.File.Exists(savePath))
				System.IO.File.Delete(savePath);
			string msg = string.Format(Catalog.GetString("There is not enough free space on the device to save file '{0}'."), bb.Filename);
			throw new IOException(msg);
		}
#endif
	}
	
	protected override bool StartProgress()
	{
		progressCallback(string.Format("Saving '{0}'", byteBuffer.Filename), ProgressAction.Message);
		return progressCallback(((double)bytesSaved)/byteBuffer.Size, ProgressAction.Show);
	}
	
	protected override void DoOperation()
	{
		stageReached=SaveStage.BeforeSaveAs;
		tempPath=savePath;
		
		// Save ByteBuffer as a temp file
		base.DoOperation();
		
		savePath=byteBuffer.Filename;
		
		// if user hasn't cancelled, move temp file to 
		// its final location	
		if (!cancelled) {
			stageReached=SaveStage.BeforeDelete;
			
			// close the file, make sure that File Operations
			// are temporarily allowed
			lock(byteBuffer.LockObj) {
				byteBuffer.FileOperationsAllowed=true;
				byteBuffer.CloseFile();
				byteBuffer.FileOperationsAllowed=false;
			}
			
			if (System.IO.File.Exists(savePath))
				System.IO.File.Delete(savePath);
			
			stageReached=SaveStage.BeforeMove;
			System.IO.File.Move(tempPath, savePath);
		}	
	}
	
}

} // end namespace
