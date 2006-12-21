// created on 6/6/2004 at 10:46 AM
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
#define USING_GTK

using System.Collections;
using System;
using System.IO;
using System.Threading;
using Bless.Util;

namespace Bless.Buffers {

///<summary>
/// A buffer for holding bytes in a versatile manner.
/// It supports undo-redo and can easily handle large files.
/// Editing is also very cheap.
///</summary>
public class ByteBuffer : IBuffer {

	internal FileBuffer	fileBuf;
	internal SegmentCollection segCol;
	Deque undoDeque;
	Deque redoDeque;
	object SaveCheckpoint;
	FileSystemWatcher fsw;
	int maxUndoActions;
	bool changedBeyondUndo;
	string tempDir;
	
	// automatic file naming
	string autoFilename;
	static int autoNum=1;
	
	internal long size;
	
	// chaining related
	bool actionChaining;
	bool actionChainingFirst;
	MultiAction multiAction;
	
	// buffer permissions
	readonly public object LockObj=new object();
	bool readAllowed;
	bool modifyAllowed;
	bool fileOperationsAllowed;
	bool emitEvents;
	
	public delegate void ChangedHandler(ByteBuffer bb);
	
	///<summary>Emitted when buffer changes</summary>
	public event ChangedHandler Changed;
	///<summary>Emitted when file changes (outside DataView)</summary>
	public event ChangedHandler FileChanged;
	///<summary>Emitted when buffer permissions change</summary>
	public event ChangedHandler PermissionsChanged;
	
	// Wrappers to emit events
	// if gtk/glib is available make sure events are called in the main GUI thread
	public void EmitChanged()
	{
		if (emitEvents && Changed!=null)
#if USING_GTK
			Gtk.Application.Invoke(delegate {
				Changed(this);
			});
#else
			Changed(this);
#endif
	}

	public void EmitFileChanged()
	{
		if (emitEvents && FileChanged!=null)
#if USING_GTK
			Gtk.Application.Invoke(delegate {
				FileChanged(this);
			});
#else
			FileChanged(this);
#endif
	}
	
	public void EmitPermissionsChanged()
	{
		if (emitEvents && PermissionsChanged!=null)
#if USING_GTK
			Gtk.Application.Invoke(delegate {
				PermissionsChanged(this);
			});
#else
			PermissionsChanged(this);
#endif
	}
	
	// related to asynchronous save model
	AsyncCallback userSaveAsyncCallback;
	AsyncCallback userSaveAsAsyncCallback;
	AutoResetEvent saveAsFinishedEvent;
	AutoResetEvent saveFinishedEvent;
	bool useGLibIdle;
	
	public ByteBuffer() 
	{
		segCol=new SegmentCollection();
		undoDeque= new Deque();
		redoDeque= new Deque();
		size=0;
		SaveCheckpoint=null;
		
		// name the buffer automatically
		autoFilename="Untitled " + ByteBuffer.autoNum;
		ByteBuffer.autoNum++;
		readAllowed=true;
		fileOperationsAllowed=true;
		modifyAllowed=true;
		saveFinishedEvent=new AutoResetEvent(false);
		saveAsFinishedEvent=new AutoResetEvent(false);
		useGLibIdle=false;
		emitEvents=true;
		maxUndoActions=-1; // unlimited undo
		tempDir = Path.GetTempPath();
	}
	
	///<summary>Create a ByteBuffer loaded with a file</summary>
	static public ByteBuffer FromFile(string filename)
	{
		ByteBuffer bb=new ByteBuffer();
		bb.LoadWithFileBuffer(new FileBuffer(filename, 0xfffff));
		
		// fix automatic file naming
		ByteBuffer.autoNum--;
		
		return bb;
	}
	
	///<summary>Create a ByteBuffer with a dummy-name</summary>
	public ByteBuffer(string filename): this() 
	{
		this.autoFilename=filename;
		
		// fix automatic file naming
		ByteBuffer.autoNum--;
	}
	
	///<summary>Regard all following actions as a single one</summary>
	public void BeginActionChaining()
	{
		actionChaining=true;
		actionChainingFirst=true;
		multiAction=new MultiAction();
		emitEvents=false;
	}
	
	///<summary>Stop regarding actions as a single one</summary>
	public void EndActionChaining()
	{
		actionChaining=false;
		actionChainingFirst=false;
		emitEvents=true;
		
		EmitChanged();
	}
	
	///<summary>Handle actions as a single one</summary>
	bool HandleChaining(ByteBufferAction action)
	{
		if (!actionChaining)
			return false;
		
		// add the multiAction to the undo deque	
		if (actionChainingFirst) {
			AddUndoAction(multiAction);
			actionChainingFirst=false;
		}
		
		multiAction.Add(action);
		
		return true;
	}
	
	///<summary>
	/// Add an action to the undo deque, taking into
	/// account maximum undo action restrictions 
	///</summary>
	void AddUndoAction(ByteBufferAction action)
	{
		if (maxUndoActions!=-1)
			while (undoDeque.Count >= maxUndoActions) {
				undoDeque.RemoveEnd();
				changedBeyondUndo=true;
			}
			
		undoDeque.AddFront(action);
	}
	
	void RedoDequeDispose()
	{
		redoDeque.Clear();
		//System.GC.Collect();
	}
	
	///<summary>Append bytes at the end of the buffer</summary>
	public void Append(byte[] data) 
	{	
		lock (LockObj) {
			if (!modifyAllowed) return;
		
			AppendAction aa=new AppendAction(data, this);	
			aa.Do();
			
			// if action isn't handled as chained (ActionChaining==false)
			// handle it manually
			if (!HandleChaining(aa)) {
				AddUndoAction(aa);
				RedoDequeDispose();
			}
			
			EmitChanged();
		}
	}

	///<summary>Insert bytes into the buffer</summary>
	public void Insert(long pos, byte[] data) 
	{
		lock (LockObj) {
			if (!modifyAllowed) return;
			
			if (pos == size) {
				Append(data);
				return;
			}
			
			InsertAction ia=new InsertAction(pos, data, this);
			ia.Do();
			
			// if action isn't handled as chained (ActionChaining==false)
			// handle it manually
			if (!HandleChaining(ia)) {		
				AddUndoAction(ia);
				RedoDequeDispose();
			}
			
			EmitChanged();
		}
	}		

	///<summary>Delete bytes from the buffer</summary>
	public void Delete(long pos1, long pos2) 
	{
		lock (LockObj) {
			if (!modifyAllowed) return;
		
			DeleteAction da=new DeleteAction(pos1, pos2, this);
			da.Do();
			
			// if action isn't handled as chained (ActionChaining==false)
			// handle it manually
			if (!HandleChaining(da)) {	
				AddUndoAction(da);
				RedoDequeDispose();
			}
			
			EmitChanged();
		}
	}
	
	///<summary>Replace bytes in the buffer</summary>
	public void Replace(long pos1, long pos2, byte[] data) 
	{
		lock (LockObj) {
			if (!modifyAllowed) return;
			
			ReplaceAction ra=new ReplaceAction(pos1, pos2, data, this);
			ra.Do();
			
			// if action isn't handled as chained (ActionChaining==false)
			// handle it manually
			if (!HandleChaining(ra)) {
				AddUndoAction(ra);
				RedoDequeDispose();
			}
			
			EmitChanged();
		}
	}
	
	///<summary>Undo the last action</summary>
	public void Undo() 
	{
		lock (LockObj) {	
			if (!modifyAllowed) return;
		
			// while there are more actions
			if (undoDeque.Count>0) {
				ByteBufferAction action=(ByteBufferAction)undoDeque.RemoveFront();
				action.Undo();
				redoDeque.AddFront(action);
				
				EmitChanged();
			}
			
		}
	}	
	
	///<summary>Redo the last undone action</summary>
	public void Redo() 
	{
		lock (LockObj) {	
			if (!modifyAllowed) return;
			
			// while there are more actions
			if (redoDeque.Count>0) {
				ByteBufferAction action=(ByteBufferAction)redoDeque.RemoveFront();
				action.Do();
				AddUndoAction(action);
				
				EmitChanged();
			}
			
		}
	}
	
	///<summary>
	/// Save the buffer as a file, using an asynchronous model
	///</summary>
	public IAsyncResult BeginSaveAs(string filename, ProgressCallback progressCallback, AsyncCallback ac)
	{
		lock (LockObj) {
			if (!fileOperationsAllowed) return null;
			
			saveAsFinishedEvent.Reset();
			userSaveAsAsyncCallback=ac;
			
			SaveAsOperation so=new SaveAsOperation(this, filename, progressCallback, SaveAsAsyncCallback, useGLibIdle);
			
			// don't allow messing up with the buffer
			// while we are saving
			this.ReadAllowed=false;
			this.ModifyAllowed=false;
			this.FileOperationsAllowed=false;
			this.EmitEvents=false;
			if (fsw!=null)
				fsw.EnableRaisingEvents=false;
			
			// start save thread
			Thread saveThread=new Thread(so.OperationThread);
			saveThread.IsBackground=true;
			saveThread.Start();
			
			return new ThreadedAsyncResult(so, saveAsFinishedEvent, false);
		}
		
	}
	
	///<summary>
	/// Called when an asynchronous Save As operation finishes
	///</summary>
	void SaveAsAsyncCallback(IAsyncResult ar)
	{
		lock (LockObj) {
			SaveAsOperation so=(SaveAsOperation)ar.AsyncState;
			
			// re-allow buffer usage
			this.ReadAllowed=true;
			this.ModifyAllowed=true;
			this.FileOperationsAllowed=true;
			this.EmitEvents=true;
			
			
			// make sure Save As went smoothly before doing anything
			if (so.Result==SaveAsOperation.OperationResult.Finished) {
				CloseFile();
				LoadWithFileBuffer(new FileBuffer(so.SavePath));
				
				if (undoDeque.Count > 0)
					SaveCheckpoint=undoDeque.PeekFront();
				else
					SaveCheckpoint=null;
					
				changedBeyondUndo=false;
			}
			else { // if cancelled or caught an exception
				// delete the file only if we have altered it
				if (so.StageReached!=SaveAsOperation.SaveAsStage.BeforeCreate) {
					try {
						System.IO.File.Delete(so.SavePath);
					}
					catch(Exception ex) { }
				}
			}
			
			if (fsw!=null)
				fsw.EnableRaisingEvents=true;
			
			// notify the world about the changes			
			EmitPermissionsChanged();
			EmitChanged();
				
			// if user provided a callback, call it now
			if (userSaveAsAsyncCallback!=null)
				userSaveAsAsyncCallback(ar);
			
			// notify that Save As has finished
			saveAsFinishedEvent.Set();
		}
	}
	
	///<summary>
	/// Save the buffer under the same filename, using an asynchronous model
	///</summary>
	public IAsyncResult BeginSave(ProgressCallback progressCallback, AsyncCallback ac) 
	{
		lock (LockObj) {		
			if (!fileOperationsAllowed) return null;
			
			saveFinishedEvent.Reset();
			userSaveAsyncCallback=ac;
			
			SaveOperation so=new SaveOperation(this, TempFile.CreateName(tempDir), progressCallback, SaveAsyncCallback, useGLibIdle);
			
			// don't allow messing up with the buffer
			// while we are saving
			this.ReadAllowed=false;
			this.ModifyAllowed=false;
			this.FileOperationsAllowed=false;
			this.EmitEvents=false;
			fsw.EnableRaisingEvents=false;
			
			// start save thread
			Thread saveThread=new Thread(so.OperationThread);
			saveThread.IsBackground=true;
			saveThread.Start();
			
			return new ThreadedAsyncResult(so, saveFinishedEvent, false);
		}
	}
	
	///<summary>
	/// Called when an asynchronous save operation finishes
	///</summary>
	void SaveAsyncCallback(IAsyncResult ar)
	{
		lock (LockObj) {
			SaveOperation so=(SaveOperation)ar.AsyncState;
			
			// re-allow buffer usage
			this.ReadAllowed=true;
			this.ModifyAllowed=true;
			this.FileOperationsAllowed=true;
			
			if (so.Result==SaveOperation.OperationResult.Finished) { // save went ok
				LoadWithFileBuffer(new FileBuffer(so.SavePath));
				
				if (undoDeque.Count > 0)
					SaveCheckpoint=undoDeque.PeekFront();
				else
					SaveCheckpoint=null;
				
				changedBeyondUndo=false;
			}
			else if (so.Result==SaveOperation.OperationResult.Cancelled) { // save cancelled
				if (so.StageReached==SaveOperation.SaveStage.BeforeSaveAs) {
					System.IO.File.Delete(so.TempPath);
				}
				else if (so.StageReached==SaveOperation.SaveStage.BeforeDelete) {
					System.IO.File.Delete(so.TempPath);
					fileBuf.Load(so.SavePath);
				}
				else if (so.StageReached==SaveOperation.SaveStage.BeforeMove) {
					// cancel has no effect during move.
					// mark operation as successful
					so.Result=SaveOperation.OperationResult.Finished;
					LoadWithFileBuffer(new FileBuffer(so.SavePath));
				
					if (undoDeque.Count > 0)
						SaveCheckpoint=undoDeque.PeekFront();
					else
						SaveCheckpoint=null;
				}
			}
			else if (so.Result==SaveOperation.OperationResult.CaughtException) {
				if (so.StageReached==SaveOperation.SaveStage.BeforeSaveAs) {
					System.IO.File.Delete(so.TempPath);
				}
				else if (so.StageReached==SaveOperation.SaveStage.BeforeDelete) {
					System.IO.File.Delete(so.TempPath);
					fileBuf.Load(so.SavePath);
					// make sure FSW is valid (it is probably not
					// because bb.CloseFile has been called in SaveOperation)
					SetupFSW();
				}
				else if (so.StageReached==SaveOperation.SaveStage.BeforeMove) {
					// TO-DO: better handling?
					fileBuf.Load(so.SavePath);
				}
			}
			
			this.EmitEvents=true;
			fsw.EnableRaisingEvents=true;
			
			// notify the world about the changes			
			EmitPermissionsChanged();
			EmitChanged();			
			
			// if user provided a callback, call it now
			if (userSaveAsyncCallback!=null)
				userSaveAsyncCallback(ar);
			
			// notify that Save has finished	
			saveFinishedEvent.Set();
		}
	}
	
	///<summary> 
	/// Revert ByteBuffer to the last saved state
	///</summary> 
	public void Revert()
	{
		lock (LockObj) {
			if (!modifyAllowed) return;
			
			if (this.HasFile) {
				// reload file
				string filename=fileBuf.Filename;
				if (!File.Exists(filename))
					throw new FileNotFoundException(filename);
			
				fileBuf.Close();
				LoadWithFileBuffer(new FileBuffer(filename));

				undoDeque.Clear();
				redoDeque.Clear();
				SaveCheckpoint=null;
				changedBeyondUndo=false;
				
				// emit bytebuffer changed event
				EmitChanged();		
			}
		}
	}
	
	///<summary>
	/// Returns in a byte array the data contained in 
	/// the specified range in the buffer.  
	///</summary>
	public byte[] RangeToByteArray(Range range)
	{
		if (range.Size==0)
			return null;
		
		byte[] rangeData=new byte[range.Size];
		
		long i=0;
		
		while (i < range.Size) {
			rangeData[i]=this[range.Start+i];
			i++;
		}
		
		return rangeData;
	}
	
	
	///<summary> 
	/// Sets the file buffer and resets the segment collection
	///</summary> 
	private void LoadWithFileBuffer(FileBuffer fb)
	{
		fileBuf=fb;
		Segment s=new Segment(fileBuf, 0, fileBuf.Size-1);
		segCol=new SegmentCollection();
		segCol.Append(s);
		size=fileBuf.Size;
		
		SetupFSW();
	}
	
	private void SetupFSW()
	{	
		// monitor the file for changes
		//
		if (fsw!=null) {
			fsw.Dispose();
			fsw=null;
		}
			
		fsw=new FileSystemWatcher();
		fsw.Path=Path.GetDirectoryName(fileBuf.Filename);
		fsw.Filter=Path.GetFileName(fileBuf.Filename);
		fsw.NotifyFilter=NotifyFilters.FileName|NotifyFilters.LastAccess|NotifyFilters.LastWrite;
		fsw.Changed += new FileSystemEventHandler(OnFileChanged);
		//fsw.Deleted += new FileSystemEventHandler(OnFileChanged);
		
		fsw.EnableRaisingEvents = true;
	}
	
	private void OnFileChanged(object source, FileSystemEventArgs e)
	{
		EmitFileChanged();
	}
	
	public int Read(byte[] data, long pos, int len)
	{
		throw new NotImplementedException();
	}
	
	public void Append(byte b)
	{
		throw new NotImplementedException();
	}
	
	public byte this[long index] {
		set { } 
		get {
			lock (LockObj) {
				long map; 
				Util.List.Node node;
				Segment seg=segCol.FindSegment(index, out map, out node);
				//Console.WriteLine("Searching index {0} at {1}:{2}", index, map, seg);
				if (seg==null)
					throw new IndexOutOfRangeException(string.Format("ByteBuffer[{0}]",index));
				else {
					try {
						return seg.Buffer[seg.Start+index-map];	
					}
					catch(IndexOutOfRangeException e) {
						Console.WriteLine("Problem at index {0} at {1}:{2}", index, map, seg);
						throw;
					}
				}
			}
		}
	}
	
	
	public void CloseFile()
	{
		lock (LockObj) {
			// close the file buffer and dispose the file watcher
			if (fileBuf != null && fileOperationsAllowed) {
				fileBuf.Close();
				fsw.Dispose();
				fsw=null;
			}
		}
	}
	
	public long Size {
		get { return size;}
	}

	public bool HasFile {
		get { return fileBuf != null; }
			
	}
	
	public string Filename {
		get {
			if (fileBuf != null)
				return fileBuf.Filename;
			else
				return this.autoFilename; 
			}	
	}

	public bool HasChanged {
		get {
			if (undoDeque.Count>0)
				return (changedBeyondUndo || SaveCheckpoint != undoDeque.PeekFront());
			else
				return (changedBeyondUndo || SaveCheckpoint != null);
		}
	
	}
	
	public bool CanUndo {
		get { return (undoDeque.Count>0); }
	}
	
	public bool CanRedo {
		get { return (redoDeque.Count>0); }
	}
	
	public bool ActionChaining {
		get { return actionChaining; }
	}
	
	// Whether the ByteBuffer will emit events
	// (eg Changed event)
	public bool EmitEvents {
		get { return emitEvents; }
		set {
			emitEvents=value; 
			//if (emitEvents && Changed!=null)
			//	Changed(this);
		}
	}
	
	// whether buffer can be safely read
	// by user eg to display data in a DataView.
	// This is only a hint and doesn't modify
	// the behavior of the buffer.
	public bool ReadAllowed { 
		get { return readAllowed; }
		set { 
			readAllowed=value;
			EmitPermissionsChanged();
		}
	}
	
	// Whether buffer can be modified.
	// If it is false, all buffer actions
	// that can modify the buffer are 
	// rendered ineffective.
	public bool ModifyAllowed {
		get { return modifyAllowed;}
		set { 
			modifyAllowed=value; 
			EmitPermissionsChanged();
		}
	}
	
	// Whether buffer can be saved, closed etc
	// If it is false, all save and close operations
	// are ignored.
	public bool FileOperationsAllowed {
		get { return fileOperationsAllowed;}
		set { 
			fileOperationsAllowed=value; 
			EmitPermissionsChanged();
		}
	}
	
	// Use the GLib Idle handler for progress reporting.
	// Mandatory if progress reporting involves Gtk+ widgets.
	public bool UseGLibIdle {
		get { return useGLibIdle; }
		set { useGLibIdle=value; }
	}
	
	// The maximum number of actions the Buffer
	// will be able to undo
	public int MaxUndoActions {
		get { return maxUndoActions; }
		set { 
			maxUndoActions=value;
			if (maxUndoActions!=-1) {
				// if we are going to remove undo actions,
				// mark that we won't be able to get back to 
				// the original buffer state
				if (undoDeque.Count > maxUndoActions)
					changedBeyondUndo=true;
				// clear all undo actions beyond the limit
				while (undoDeque.Count > maxUndoActions) {
					undoDeque.RemoveEnd();
				}
				
			}
		}
	}
	
	public string TempDir {
		get { return tempDir; }
		set { tempDir = value;}
	}
	
	internal void Display(string s) 
	{
		Console.Write(s);
		segCol.List.Display();
	}

}

} // end namespace 
