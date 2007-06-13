// created on 3/31/2005 at 1:41 PM
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

using Bless.Util;
using System;
using System.Threading;


namespace Bless.Tools.Find {

///<summary>
/// Class to hold information about an asynchronous find operation
///</summary>
public class FindAsyncResult: IAsyncResult {
	object asyncState;
	WaitHandle asyncWaitHandle;
	bool isCompleted;

	public FindAsyncResult(object state, WaitHandle handle, bool complete)
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