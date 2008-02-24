// created on 2/24/2008 at 5:02 PM
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

using Bless.Util;

namespace Bless.Buffers {

///<summary>Holds information about the state of an asynchronous save operation</summary>
public interface ISaveState 
{
	
	ThreadedAsyncOperation.OperationResult Result {
		get;
	}

	string SavePath {
		get;
	}
	
	ByteBuffer Buffer { 
		get; 
	}
	
	System.Exception ThreadException {
		get;
	}
}

} // end namespace