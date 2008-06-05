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
	protected bool cancelled;
	protected ProgressCallback progressCallback;
	protected AsyncCallback opFinishedCallback;

	Exception threadException;
	OperationResult opResult;

	bool useGLibIdle;

	// the kind of event that the save thread can emit to
	// the main thread
	private enum Event { ShowProgress, Finished, Exception}

	// the result of the operation
	public enum OperationResult { Finished, Cancelled, CaughtException }

	public OperationResult Result {
		get { return opResult; }
		set { opResult = value; }
	}

	public Exception ThreadException {
		get { return threadException; }
		set { threadException = value;}
	}

	public ThreadedAsyncOperation(ProgressCallback pc,
								  AsyncCallback ac, bool glibIdle)
	{
		progressCallback = pc;
		opFinishedCallback = ac;
		useGLibIdle = glibIdle;

		cancelled = false;
	}

	protected abstract bool StartProgress();
	protected abstract bool UpdateProgress();
	protected abstract bool EndProgress();
	protected abstract void DoOperation();
	protected abstract void EndOperation();

	///<summary>
	/// Called when the operation has finished
	///</summary>
	void OperationFinished()
	{
		// destroy progress report
		if (progressCallback != null)
			EndProgress();

		// call callback
		if (opFinishedCallback != null)
			opFinishedCallback(new ThreadedAsyncResult(this, null, true));

	}

	///<summary>
	/// Called once by a timer to make the progress report visible
	///</summary>
	void ShowProgressTimerExpired(object o)
	{
		if (progressCallback != null) {
			if (useGLibIdle)
				GLib.Idle.Add(StartProgress);
			else
				StartProgress();
		}
	}

	///<summary>
	/// Called periodically by a timer to update the progress report
	///</summary>
	void ProgressTimerExpired(object o)
	{
		if (progressCallback != null) {
			if (useGLibIdle)
				GLib.Idle.Add(delegate { cancelled = UpdateProgress(); return false; });
			else
				cancelled = UpdateProgress();
		}
	}

	public void OperationThread()
	{
		// showProgressTimer fires once and makes progress reporting visible
		Timer showProgressTimer = new Timer(new TimerCallback(ShowProgressTimerExpired), null, 500, 0);

		// progressTimer fires periodically and updates progress reporting
		Timer progressTimer = new Timer(new TimerCallback(ProgressTimerExpired), null, 0, 50);

		try {
			DoOperation();

			if (cancelled) 
				opResult = OperationResult.Cancelled;
			else
				opResult = OperationResult.Finished;	
		}
		catch (Exception e) {
			threadException = e;
			opResult = OperationResult.CaughtException;	
		}
		finally {
			progressTimer.Dispose();
			showProgressTimer.Dispose();
			EndOperation();

			if (useGLibIdle)
				GLib.Idle.Add(delegate { OperationFinished(); return false; });
			else
				OperationFinished();
		}

	}
}

///<summary>
/// Class to hold information about an asynchronous threaded operation
///</summary>
public class ThreadedAsyncResult: IAsyncResult {
	object asyncState;
	WaitHandle asyncWaitHandle;
	bool isCompleted;

	public ThreadedAsyncResult(object state, WaitHandle handle, bool complete)
	{
		asyncState = state;
		asyncWaitHandle = handle;
		isCompleted = complete;
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
