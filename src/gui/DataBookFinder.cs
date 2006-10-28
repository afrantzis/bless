// created on 1/17/2005 at 5:00 PM
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
using System.Text;
using System.Threading;
using Bless.Util;
using Bless.Buffers;
using Bless.Tools.Find;
 
namespace Bless.Gui {

public class DataBookFinder : IFinder
{
	DataBook dataBook;
	IFindStrategy strategy;
	Bless.Util.Range lastFound;
	Bless.Util.Range threadResult; // the result of the search thread
	Thread findThread;
	
	public event FirstFindHandler FirstFind;
	
	ProgressCallback progressCallback; // callback for progress reporting 
	AsyncCallback userFindAsyncCallback;
	AutoResetEvent findFinishedEvent; //
	
	// shared between main and ReplaceAll threads 
	Bless.Util.Range firstMatch;
	int numReplaced;
	
	bool firstTime;
	
	// The find progress handler runs an iteration of the application
	// event loop to make the gui more responsive. During such an iteration 
	// an event may arrive and trigger again a DataBookFinder method (in the same
	// thread) but DataBookFinder methods are not re-entrant. The inUse variable 
	// is used to check if the DataBookFinder is already in use and disallow such
	// a re-entrance. 
	bool inUse; 
	
	public DataBookFinder(DataBook db, ProgressCallback pc)
	{ 
		// connect with databook
		dataBook=db;
		lastFound=new Bless.Util.Range();
		firstTime=true;
		inUse=false;
		progressCallback=pc;
		
		// initialize events
		findFinishedEvent=new AutoResetEvent(false);
		
	}

	public IFindStrategy Strategy 
	{ 
		get { return strategy;} 
		set { strategy=value; }
	}
	
	public Bless.Util.Range LastFound 
	{ 
		get { return lastFound;}
		set { lastFound=value; }
	}
	
	///<summary>Sets up the messages displayed when reporting the search progress</summary>
	void SetUpFindProgressReport()
	{
		progressCallback("Searching", ProgressAction.Message);
		
		StringBuilder details=new StringBuilder();
		details.AppendFormat("File: {0}", strategy.Buffer.Filename);
		
		progressCallback(details.ToString(), ProgressAction.Details);
	}
	
	///<summary>Sets up the messages displayed when reporting the replace all progress</summary>
	void SetUpReplaceAllProgressReport()
	{
		progressCallback("Replacing All", ProgressAction.Message);
		
		StringBuilder details=new StringBuilder();
		details.AppendFormat("File: {0}", strategy.Buffer.Filename);
		
		progressCallback(details.ToString(), ProgressAction.Details);
	}

	///<summary> 
	/// Handles operations that will surely produce on result (eg when
	/// no DataView exists).
	///</summary>
	IAsyncResult HandleProblematicOp(GenericFindOperation op, AsyncCallback ac)
	{
		op.Result=GenericFindOperation.OperationResult.Finished;
		
		findFinishedEvent.Reset();
		
		IAsyncResult iar=new ThreadedAsyncResult(op, findFinishedEvent, true);
		if (ac!=null)
			ac(iar);
			
		findFinishedEvent.Set();
		return iar;
	}
	
	///<summary>
	/// Find the next occurence of the byte pattern. Works asynchronously.
	///</summary>
	public IAsyncResult FindNext(AsyncCallback ac)
	{
		if (dataBook.NPages==0 || inUse) {
			FindNextOperation op=new FindNextOperation(strategy, null, FindAsyncCallback);
			return HandleProblematicOp(op, ac);
		}
		
		inUse=true;
		
		userFindAsyncCallback=ac;
		findFinishedEvent.Reset();
		
		// if this is the first time we are checking
		// for a valid pattern, emit the event
		if (firstTime==true && strategy.Pattern.Length>0) {
			firstTime=false;
			if (FirstFind!=null)
				FirstFind();
		} // is pattern acceptable?
		else if (strategy.Pattern.Length==0)
			return null;
				
		DataView dv=((DataViewDisplay)dataBook.CurrentPageWidget).View;
		
		strategy.Buffer=dv.Buffer;
		
		// decide where to start searching from
		if (!dv.Selection.IsEmpty())
			strategy.Position=dv.Selection.Start+1;
		else
			strategy.Position=dv.CursorOffset;
		
		SetUpFindProgressReport();
		
		
		FindNextOperation fno=new FindNextOperation(strategy, progressCallback, FindAsyncCallback);
		
		// lock the buffer, so user can't modify it while
		// searching
		strategy.Buffer.ModifyAllowed=false;
		strategy.Buffer.FileOperationsAllowed=false;
		
		// start find thread
		Thread findThread=new Thread(fno.OperationThread);
		findThread.IsBackground=true;
		findThread.Start();
			
		return new ThreadedAsyncResult(fno, findFinishedEvent, false);
	}
	
	///<summary>
	/// Find the previous occurence of the byte pattern. Works asynchronously.
	///</summary>
	public IAsyncResult FindPrevious(AsyncCallback ac)
	{
		if (dataBook.NPages==0 || inUse) {
			FindPreviousOperation op=new FindPreviousOperation(strategy, null, FindAsyncCallback);
			return HandleProblematicOp(op, ac);
		}
		
		inUse=true;
		
		userFindAsyncCallback=ac;
		findFinishedEvent.Reset();
		
		// if this is the first time we are checking
		// for a valid pattern, emit the event
		if (firstTime==true && strategy.Pattern.Length>0) {
			firstTime=false;
			if (FirstFind!=null)
				FirstFind();
		} // is pattern acceptable?
		else if (strategy.Pattern.Length==0)
			return null;
		
		DataView dv=((DataViewDisplay)dataBook.CurrentPageWidget).View;
		
		strategy.Buffer=dv.Buffer;
		
		// decide where to start searching from
		if (!dv.Selection.IsEmpty())
			strategy.Position=dv.Selection.End;
		else
			strategy.Position=dv.CursorOffset;
		
		SetUpFindProgressReport();
		
		FindPreviousOperation fpo=new FindPreviousOperation(strategy, progressCallback, FindAsyncCallback);
		
		// lock the buffer, so user can't modify it while
		// searching
		strategy.Buffer.ModifyAllowed=false;
		strategy.Buffer.FileOperationsAllowed=false;
		
		// start find thread
		Thread findThread=new Thread(fpo.OperationThread);
		findThread.IsBackground=true;
		findThread.Start();
		
		return new ThreadedAsyncResult(fpo, findFinishedEvent, false);
	}
	
	///<summary>
	/// Called when an asynchronous Find Next/Previous operation finishes.
	///</summary>
	void FindAsyncCallback(IAsyncResult ar)
	{
		GenericFindOperation state=(GenericFindOperation)ar.AsyncState;
		ThreadedAsyncOperation.OperationResult result=state.Result;
		Range match=state.Match;
		
		DataView dv=null;
		
		// find DataView that owns bb
		foreach (DataViewDisplay dvtemp in dataBook.Children) {
			if (dvtemp.View.Buffer==strategy.Buffer) {
				dv=dvtemp.View;
				break;	
			}
		}
		
		// decide what to do based on the result of the find operation
		switch (result) {
			case ThreadedAsyncOperation.OperationResult.Finished:
				if (match!=null) {
					lastFound=match;
					dv.SetSelection(match.Start, match.End);//System.Console.WriteLine("Found at {0}-{1}", r.Start, r.End);
					dv.MoveCursor(match.End+1, 0);
					dv.Display.MakeOffsetVisible(match.Start, DataViewDisplay.ShowType.Closest);
				}
				else {
					lastFound.Clear();
				}
				break; 
			case ThreadedAsyncOperation.OperationResult.Cancelled:
				dv.MoveCursor(strategy.Position, 0);
				dv.Display.MakeOffsetVisible(strategy.Position, DataViewDisplay.ShowType.Closest);
				break;
			case ThreadedAsyncOperation.OperationResult.CaughtException:
				break;
			default:
				break;
		}
		
		inUse=false;
		
		// if user provided a callback, call it now
		if (userFindAsyncCallback!=null)
			userFindAsyncCallback(ar);
		
		// notify that the find operation has finished	
		findFinishedEvent.Set();
	}
	
	///<summary>Check if the specified range of bytes in a ByteBuffer equals a byte pattern</summary>
	bool RangeEqualsPattern(ByteBuffer bb, Bless.Util.Range sel, byte[] pattern)
	{
		int i=0;
		int len=pattern.Length;
		
		if (sel.IsEmpty())
			return false;
		
		while (i<len && pattern[i]==bb[sel.Start+i]) 
			i++;
		
		if (i!=len)
			return false;
		else
			return true;
		
	}
	
	///<summary>
	/// Replace the last match with a pattern.
	/// Returns whether the replace operation was succesful.
	///</summary>
	public bool Replace(byte[] ba)
	{
		if (dataBook.NPages==0)
			return false;
		
		// DataBookFinder is already in use
		if (inUse)
			return true;
		
		inUse=true;
			
		DataView dv=((DataViewDisplay)dataBook.CurrentPageWidget).View;
		if (RangeEqualsPattern(dv.Buffer, dv.Selection, strategy.Pattern)) {
			dv.Buffer.Replace(dv.Selection.Start, dv.Selection.End, ba);
			
			dv.CursorUndoDeque.AddFront(new CursorState(dv.Selection.Start, 0, dv.Selection.Start+ba.Length, 0));
			dv.CursorRedoDeque.Clear();
			
			dv.MoveCursor(dv.Selection.Start+ba.Length, 0);
			dv.SetSelection(-1, -1);
			
			lastFound.Clear();
			
			inUse=false;
			return true;
		}
		
		inUse=false;
		
		return false;
	}
	
	///<summary>
	/// Replace all matches with a pattern. Works asynchronously.
	///</summary>
	public IAsyncResult ReplaceAll(byte[] ba, AsyncCallback ac)
	{
		if (dataBook.NPages==0 || inUse) {
			ReplaceAllOperation op=new ReplaceAllOperation(strategy, null, ReplaceAllAsyncCallback, ba);
			return HandleProblematicOp(op, ac);
		}
		
		// DataBookFinder is already in use
		if (inUse)
			return null;
		
		inUse=true;
		
		userFindAsyncCallback=ac;
		
		DataView dv=((DataViewDisplay)dataBook.CurrentPageWidget).View;
		
		// initialize strategy
		strategy.Buffer=dv.Buffer;
		strategy.Position=0;
		
		SetUpReplaceAllProgressReport();
		
		ReplaceAllOperation rao=new ReplaceAllOperation(strategy, progressCallback, ReplaceAllAsyncCallback, ba);	
		
		findFinishedEvent.Reset();
			
		// don't allow messing up with the buffer
		
		// start replace all thread
		Thread findThread=new Thread(rao.OperationThread);
		findThread.IsBackground=true;
		findThread.Start();
		
		return new ThreadedAsyncResult(rao, findFinishedEvent, false);
	}
	
	///<summary>
	/// Called when an asynchronous Replace All operation finishes.
	///</summary>
	void ReplaceAllAsyncCallback(IAsyncResult ar)
	{
		ReplaceAllOperation state=(ReplaceAllOperation)ar.AsyncState;
		ThreadedAsyncOperation.OperationResult result=state.Result;
		Range firstMatch=state.FirstMatch;
		
		DataView dv=null;
		
		// find DataView that owns bb
		foreach (DataViewDisplay dvtemp in dataBook.Children) {
			if (dvtemp.View.Buffer==strategy.Buffer) {
				dv=dvtemp.View;
				break;	
			}
		}
		
		// decide what to do based on the result of the Replace All operation
		if (result==ThreadedAsyncOperation.OperationResult.Cancelled) {
			dv.Buffer.Undo();
		} 
		// if we have replaced at least one occurence,
		else if (result==ThreadedAsyncOperation.OperationResult.Finished && firstMatch!=null) {
			lastFound=state.Match;
			// save the cursor state for undo/redo
			dv.CursorUndoDeque.AddFront(new CursorState(firstMatch.Start, 0, lastFound.Start+state.ReplacePattern.Length, 0));
		
			// move cursor after final replacement
			dv.SetSelection(-1, -1);
			dv.MoveCursor(lastFound.Start+state.ReplacePattern.Length, 0);
			dv.Display.MakeOffsetVisible(lastFound.Start+state.ReplacePattern.Length, DataViewDisplay.ShowType.Closest);
		}
		
		inUse=false;
		
		// if user provided a callback, call it now
		if (userFindAsyncCallback!=null)
			userFindAsyncCallback(ar);
		
		// notify that the replace all operation has finished	
		findFinishedEvent.Set();
	}
}
 
} // end namespace
