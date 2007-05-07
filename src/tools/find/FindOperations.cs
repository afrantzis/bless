// created on 3/31/2005 at 2:23 PM
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

namespace Bless.Tools.Find {

///<summary>
/// Generic find operation using an asynchronous threaded model of operation.
///</summary>
public abstract class GenericFindOperation: ThreadedAsyncOperation
{

	protected IFindStrategy strategy;
	protected Bless.Util.Range match;
	
	
	public IFindStrategy Strategy {
		get {return strategy;}
	}
	
	public Bless.Util.Range Match {
		get { return match; }
	}
	
	public GenericFindOperation(IFindStrategy ifs, ProgressCallback pc,
							AsyncCallback ac): base(pc, ac, true)
	{
		strategy=ifs;
		
		strategy.Cancelled=false; 
		match=null;
	}
	
	protected override bool StartProgress()
	{
		return progressCallback(((double)strategy.Position)/strategy.Buffer.Size, ProgressAction.Show);
	}
	
	protected override bool UpdateProgress()
	{
		strategy.Cancelled=progressCallback(((double)strategy.Position)/strategy.Buffer.Size, ProgressAction.Update);
		return strategy.Cancelled;
	}
	
	protected override bool EndProgress()
	{
		return progressCallback(((double)strategy.Position)/strategy.Buffer.Size, ProgressAction.Hide);
	}
	
	protected override void IdleHandlerEnd()
	{
		// re-allow buffer usage
		strategy.Buffer.ReadAllowed=true;
		strategy.Buffer.ModifyAllowed=true;
		strategy.Buffer.FileOperationsAllowed=true;
	}
	
	protected override void DoOperation()
	{
		match=strategy.FindNext();	
	}
	
	protected override void EndOperation()
	{
		
	}
	
}

///<summary>
/// Find Next operation using an asynchronous threaded model of operation.
///</summary>
public class FindNextOperation: GenericFindOperation
{
	public FindNextOperation(IFindStrategy ifs, ProgressCallback pc,
							AsyncCallback ac): base(ifs, pc, ac)
	{
	}
	
	protected override void DoOperation()
	{
		match=strategy.FindNext();	
	}	
}

///<summary>
/// Find Previous operation using an asynchronous threaded model of operation.
///</summary>
public class FindPreviousOperation: GenericFindOperation
{
	public FindPreviousOperation(IFindStrategy ifs, ProgressCallback pc,
							AsyncCallback ac): base(ifs, pc, ac)
	{
	}
	
	protected override void DoOperation()
	{
		match=strategy.FindPrevious();	
	}	
}

///<summary>
/// Replace All operation using an asynchronous threaded model of operation.
///</summary>
public class ReplaceAllOperation: GenericFindOperation
{
	byte[] replacePattern;
	Bless.Util.Range firstMatch;
	long numReplaced;
	
	public long NumReplaced {
		get { return numReplaced; }
	}
	
	public byte[] ReplacePattern {
		get { return replacePattern; }
	}
	
	public Bless.Util.Range FirstMatch {
		get { return firstMatch; }
	}
	
	
	public ReplaceAllOperation(IFindStrategy ifs, ProgressCallback pc,
							AsyncCallback ac, byte[] repPat): base(ifs, pc, ac)
	{
		replacePattern=repPat;
	}
	
	protected override void IdleHandlerEnd()
	{
		// re-allow buffer usage
		strategy.Buffer.ReadAllowed=true;
		strategy.Buffer.ModifyAllowed=true;
		strategy.Buffer.FileOperationsAllowed=true;
		strategy.Buffer.EmitEvents=true;

		strategy.Buffer.EmitPermissionsChanged();		
		strategy.Buffer.EmitChanged();

		
		strategy.Buffer.EndActionChaining();
	}
	
	protected override void DoOperation()
	{
		Range m;
		match=new Range();
		firstMatch=null;
		
		numReplaced=0;
		
		strategy.Buffer.BeginActionChaining();
		strategy.Buffer.ModifyAllowed=false;
		strategy.Buffer.FileOperationsAllowed=false;
		strategy.Buffer.EmitEvents=false;
		
		while ((m=strategy.FindNext())!=null) {
			if (firstMatch==null) {
				firstMatch=new Range(m);
			}	
		
			match.Start=m.Start;
			match.End=m.End;
		
			lock (strategy.Buffer.LockObj) {
				strategy.Buffer.ModifyAllowed=true;
				strategy.Buffer.Replace(m.Start, m.End, replacePattern);
				strategy.Buffer.ModifyAllowed=false;
			}
		
			// start next search after the replaced pattern 
			strategy.Position=m.Start+replacePattern.Length;
				
			numReplaced++;
		}
		
	}	

}

} // end namespace
