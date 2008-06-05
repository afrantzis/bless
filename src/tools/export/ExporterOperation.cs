// created on 12/4/2006 at 9:36 PM
/*
 *   Copyright (c) 2006, Alexandros Frantzis (alf82 [at] freemail [dot] gr)
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
using Bless.Util;
using Bless.Buffers;

namespace Bless.Tools.Export
{

public class ExportOperation : ThreadedAsyncOperation
{
	IExporter exporter;

	IBuffer buffer;
	long rangeStart;
	long rangeEnd;


	public IExporter Exporter {
		get { return exporter; }
	}

	public ExportOperation(IExporter ex, IBuffer buf, long start, long end, ProgressCallback progressCb, AsyncCallback endCb)
			: base(progressCb, endCb, true)
	{
		exporter = ex;
		buffer = buf;
		rangeStart = start;
		rangeEnd = end;
	}

	private double CalculatePercentDone()
	{
		return ((double)(exporter.CurrentPosition - rangeStart)) / (rangeEnd - rangeStart);
	}

	protected override bool StartProgress()
	{
		return progressCallback(CalculatePercentDone(), ProgressAction.Show);
	}

	protected override bool UpdateProgress()
	{
		return progressCallback(CalculatePercentDone(), ProgressAction.Update);
	}

	protected override bool EndProgress()
	{
		return progressCallback(CalculatePercentDone(), ProgressAction.Hide);
	}

	protected override void DoOperation()
	{
		System.Console.WriteLine("Starting export");
		exporter.Export(buffer, rangeStart, rangeEnd, ref cancelled);
		System.Console.WriteLine("Ending export");
	}


	protected override void EndOperation()
	{

	}

}


}
