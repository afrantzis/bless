// Created on 12:43 AM 19/3/2008
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

using System;
using System.Collections.Generic;
using Bless.Buffers;
using Bless.Util;
using Bless.Gui.Drawers;

namespace Bless.Gui.Areas
{

public class AreaGroup
{
	IList<Area> areas;
	ByteBuffer byteBuffer;
	Gtk.DrawingArea drawingArea;
	
	IntervalTree<Highlight> highlights;
	
	enum Changes { Offset = 1, Cursor = 2, Highlights = 4}
	
	Changes changes;
	
	// current offset of view in the buffer
	long offset; 
	
	// current cursor
	long cursorOffset;
	int cursorDigit;
	
	// track changes
	long prevOffset;
	long prevCursorOffset;
	int  prevCursorDigit;
	
	Highlight selection;
	
	public IList<Area> Areas {
		get { return areas; }
	}
	
	public long Offset {
		set { prevOffset = offset; offset = value; SetChangedAll();}
		get { return offset;}
	}

	public long CursorOffset {
		set { prevCursorOffset = cursorOffset; cursorOffset = value; SetChanged(Changes.Cursor);}
		get { return cursorOffset;}
	}
	
	public int CursorDigit {
		set { prevCursorDigit = cursorDigit; cursorDigit = value; SetChanged(Changes.Cursor); }
		get { return cursorDigit;}
	}
	
	public long PrevCursorOffset {
		get { return prevCursorOffset;}
	}
	
	public ByteBuffer Buffer {
		get { return byteBuffer; }
		set { byteBuffer = value; }
	}
	
	public Gtk.DrawingArea DrawingArea {
		get { return drawingArea; }
		set { drawingArea = value; }
	}
	
	public Range Selection {
		get { return selection; }
		set { 
			highlights.Delete(selection);
			selection.Start = value.Start; selection.End = value.End;
			highlights.Insert(selection);
			SetChanged(Changes.Highlights);
		}
	}
	
	public AreaGroup()
	{
		areas = new System.Collections.Generic.List<Area>();
		highlights = new IntervalTree<Highlight>();
		selection = new Highlight(Drawer.HighlightType.Selection);
	}
	
	/// <summary>
	/// Get the range of bytes and the number of rows that 
	/// are displayed in the current view.
	/// </summary>
	private Range GetViewRange(out int nrows)
	{
		// find out number of rows, bytes in current view
		
		int minRows = int.MaxValue;
		int minBpr = int.MaxValue;
		foreach (Area a in areas) {
			minRows = Math.Min(minRows, a.Height / a.Drawer.Height);
			minBpr = Math.Min(minBpr, a.BytesPerRow);
		}
		
		nrows = minRows;
		
		long bleft = minRows * minBpr;

		if (bleft + offset >= byteBuffer.Size)
			bleft = byteBuffer.Size - offset;
		
		// make sure we get an empty clipping Range when bleft==0
		if (bleft > 0)
			return new Range(offset, offset + bleft - 1);
		else
			return new Range();
	}
	
	private bool HasChanged(Changes c)
	{
		return ((changes & c) != 0);
	}
	
	private void ClearChanges()
	{
		changes = 0;
	}
	
	private void SetChanged(Changes c)
	{
		changes |= c;
		if (drawingArea != null)
			drawingArea.QueueDraw();
	}
	
	private void SetChangedAll()
	{
		SetChanged(Changes.Offset);
		SetChanged(Changes.Cursor);
		SetChanged(Changes.Highlights);
	}
	
	public void AddHighlight(long start, long end, Drawer.HighlightType ht)
	{
		highlights.Insert(new Highlight(start, end, ht));
	}
	
	public void ClearHighlights()
	{
		//highlights.Clear();
	}

	private void RenderRange(Range range, Drawer.HighlightType ht)
	{
		foreach(Area a in areas) {
			a.RenderRange(range, ht);
		}
	}
	
	private void BlankBackground()
	{
		foreach(Area a in areas) {
			a.BlankBackground();
		}
	}
	
	private IList<Highlight> BreakDownHighlights(Highlight s, IList<Highlight> lst)
	{
		IntervalTree<Highlight> it = new IntervalTree<Highlight>();
		
		it.Insert(s);
		
		foreach(Highlight r in lst) {
			IList<Highlight> overlaps = it.SearchOverlap(r);
			foreach(Highlight q in overlaps) {
				it.Delete(q);
				Highlight[] ha = new Highlight[3]{new Highlight(q.Type), new Highlight(r.Type), new Highlight(q.Type)};
				Range.SplitAtomic(ha, q, r);
				foreach(Highlight h in ha) {
					if (!h.IsEmpty())
						it.Insert(h);
				}	
			}
		}
		
		return it.GetValues();
	}
	
	private void RenderAll()
	{
		// blank the background
		BlankBackground();
	
		int nrows;
		Range clip = GetViewRange(out nrows);
		Highlight view = new Highlight(clip, Drawer.HighlightType.Normal);
		
		// get all highlights in current view
		IList<Highlight> viewableHighlights = highlights.SearchOverlap(view);
		
		IList<Highlight> hl = BreakDownHighlights(view, viewableHighlights);
				
		foreach(Highlight h in hl) {
			RenderRange(h, h.Type);
		}
				
	}
	
	public void Render()
	{
		Render(false);
	}
	
	public void Render(bool force)
	{
		RenderAll();
		
		if (HasChanged(Changes.Highlights) && !HasChanged(Changes.Offset))
			System.Console.WriteLine("Could optimize");
		/*
		// if offset has changed redraw completely
		if (force || HasChanged(Changes.Offset)) {
			RenderAll();
		}
		
		// if just highlights have changed...
		if (HasChanged(Changes.Highlights) && !HasChanged(Changes.Offset)) {
			//RenderHighlights();
		}
		
		if (force || HasChanged(Changes.Cursor)) {
			//RenderCursor();
		}*/
		
		
		ClearChanges();
	}

}

} // end namespace