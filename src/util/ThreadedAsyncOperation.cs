// created on 4/4/2005 at 4:20 PM
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
using System.Threading;

namespace Bless.Util {

///<summary>
/// Abstract class modelling a threaded asynchronous operation
///</summary>
public abstract class ThreadedAsyncOperation
{
	AutoResetEvent showProgressEvent;
	AutoResetEvent opFinishedEvent;
	AutoResetEvent opExceptionEvent;

	WaitHandle[] threadEvents;

	protected bool cancelled;
	protected ProgressCallback progressCallback;
	protected AsyncCallback opFinishedCallback;
	
	Exception threadException;
	OperationResult opResult;
	Thread opThread;
	
	readonly object idleLockObj=new object();
	bool useGLibIdle;
	
	// the kind of event that the save thread can emit to
	// the main thread 
	private enum Event { ShowProgress, Finished, Exception}
	
	// the result of the operation
	public enum OperationResult { Finished, Cancelled, CaughtException }
	
	public OperationResult Result {
		get { return opResult; }
		set { opResult=value; }
	}
	
	public Exception ThreadException {
		get { return threadException; }
		set { threadException=value;}
	}
	
	public ThreadedAsyncOperation(ProgressCallback pc,
							AsyncCallback ac, bool glibIdle)
	{
		
		progressCallback=pc;
		opFinishedCallback=ac;
		useGLibIdle=glibIdle;
		
		showProgressEvent=new AutoResetEvent(false);
		opFinishedEvent=new AutoResetEvent(false);
		opExceptionEvent=new AutoResetEvent(false);
		threadEvents=new WaitHandle[]{showProgressEvent, opFinishedEvent, opExceptionEvent};
		
		cancelled=false;
	}
	
	protected abstract void IdleHandlerEnd();
	protected abstract bool StartProgress();
	protected abstract bool UpdateProgress();
	protected abstract bool EndProgress();
	protected abstract void DoOperation();
	protected abstract void EndOperation();
	
	///<summary>
	/// A method that is called periodically (optionally as a GLib idle handler)
	/// to handle syncronization of main and operation threads
	//</summary>
	bool OpIdleHandler()
	{	
		lock (idleLockObj) {
		
		// if we have already cancelled, return
		if (cancelled)
			return false;
		
		// update progress report and check for cancellation 
		if (progressCallback!=null)
			cancelled=UpdateProgress();
		
		bool exceptionOccured=false;
		
		// if user has not just cancelled
		if (!cancelled) {
			// check for any event from operation thread
			ThreadedAsyncOperation.Event n=(ThreadedAsyncOperation.Event)WaitHandle.WaitAny(threadEvents, 0, true);
			if (n==ThreadedAsyncOperation.Event.ShowProgress) { // show progress event
				if (progressCallback!=null)
					StartProgress();
				return false;
			}
			else if (n==ThreadedAsyncOperation.Event.Finished) { // save finished Event
			}
			else if (n==ThreadedAsyncOperation.Event.Exception) { // save thread caught exception
				exceptionOccured=true;
			}
			else { //no event, continue normally
				return false;
			}
		}
		
		// if we reached this point it means that either: 
		// * user cancelled or
		// * thread finished normally or
		// * thread caught an exception
		
		// wait for operation thread to finish
		// opThread==current thread when
		// not using GLib main loop for calling
		// the idle handler
		if (opThread!=Thread.CurrentThread)
			opThread.Join();
		
		IdleHandlerEnd();
		
		// set the operation result
		if (exceptionOccured) {
			opResult=OperationResult.CaughtException;
		}
		else if (cancelled)
			opResult=OperationResult.Cancelled;
		else
			opResult=OperationResult.Finished;
			
		// destroy progress report
		if (progressCallback!=null)
			EndProgress();
		
		// call callback
		if (opFinishedCallback!=null)
			opFinishedCallback(new ThreadedAsyncResult(this, null, true));
		
		// remove handler from glib idle 
		return false;
		}
	}
	
	///<summary>
	/// Called once by a timer to make the progress report visible
	///</summary>
	void ShowProgressTimerExpired(object o)
	{
		showProgressEvent.Set();
	}
	
	///<summary>
	/// Called periodically by a timer to update the progress report
	///</summary>
	void ProgressTimerExpired(object o)
	{
		lock(idleLockObj) {
			if (useGLibIdle)
				GLib.Idle.Add(OpIdleHandler);
			else
				OpIdleHandler();
		}
	}
	
	public void OperationThread()
	{
		opThread=Thread.CurrentThread;
		
		// showProgressTimer fires once and makes progress reporting visible
		Timer showProgressTimer=new Timer(new TimerCallback(ShowProgressTimerExpired), null, 500, 0);
		
		// progressTimer fires periodically and updates progress reporting 
		Timer progressTimer=new Timer(new TimerCallback(ProgressTimerExpired), null, 0, 50);
		
		try {
			DoOperation();
			opFinishedEvent.Set();
		}
		catch(Exception e) {
			threadException=e;
			opExceptionEvent.Set();
		}
		finally {
			progressTimer.Dispose();
			showProgressTimer.Dispose();
			EndOperation();
			// one last invocation needed,
			// so that the saveFinished or SaveException 
			// events are caught and handled
			if (useGLibIdle)
				GLib.Idle.Add(OpIdleHandler);
			else 
				OpIdleHandler();
		}	
	
	}
}

///<summary>
/// Class to hold information about an asynchronous threaded operation
///</summary>
public class ThreadedAsyncResult:IAsyncResult {
	object asyncState;
	WaitHandle asyncWaitHandle;
	bool isCompleted;
	
	public ThreadedAsyncResult(object state, WaitHandle handle, bool complete)
	{
		asyncState=state;
		asyncWaitHandle=handle;
		isCompleted=complete;
	} 
	public object AsyncState {
		get { return asyncState;}
	}
	public WaitHandle AsyncWaitHandle {
		get { return asyncWaitHandle;}
	}
	public bool CompletedSynchronously {
		get { return false;}
	}
	public bool IsCompleted {
		get { return isCompleted;}
	}
}

} // end namespace
