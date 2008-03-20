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

/// <summary>
/// A group of areas that display data from the same source
/// and are synchronized.
/// </summary>
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
	
	/// <value>
	/// The previous atomic highlight ranges of the view.
	/// These are non-overlapping ranges that describe the highlighting of the whole view
	/// and can be used to render it quickly. They are also used to minimize
	/// redrawing when possible (<see cref="AreaGroup.RenderHighlightDiffs"/>)
	/// </value>
	IntervalTree<Highlight> prevAtomicHighlights;
	
	
	Highlight selection;
	
	public IList<Area> Areas {
		get { return areas; }
	}
	
	public long Offset {
		set { prevOffset = offset; offset = value; SetChanged(Changes.Offset);}
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
		prevAtomicHighlights = new IntervalTree<Highlight>();
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
	
	/// <summary>
	/// Whether a <see cref="Changes"/> has changed.
	/// </summary>
	private bool HasChanged(Changes c)
	{
		return ((changes & c) != 0);
	}
	
	/// <summary>
	/// Whether anything has changed.
	/// </summary>
	private bool HasAnythingChanged()
	{
		return changes != 0;
	}
	
	/// <summary>
	/// Clear all changes
	/// </summary>
	private void ClearChanges()
	{
		changes = 0;
	}
	
	/// <summary>
	/// Set a  <see cref="Changes"/> as changed.
	/// </summary>
	/// <param name="c">
	/// A <see cref="Changes"/>
	/// </param>
	private void SetChanged(Changes c)
	{
		changes |= c;
		if (drawingArea != null)
			drawingArea.QueueDraw();
	}
	
	/// <summary>
	/// Adds a highlight on a range of data.
	/// </summary>
	/// <param name="start">
	/// A <see cref="System.Int64"/>
	/// </param>
	/// <param name="end">
	/// A <see cref="System.Int64"/>
	/// </param>
	/// <param name="ht">
	///  <see cref="Drawer.HighlightType"/>
	/// </param>
	public void AddHighlight(long start, long end, Drawer.HighlightType ht)
	{
		highlights.Insert(new Highlight(start, end, ht));
	}
	
	public void ClearHighlights()
	{
		//highlights.Clear();
	}

	/// <summary>
	/// Renders a <see cref="Range"/> of data using a specified <see cref="Drawer.HighlightType"/>
	/// </summary>
	private void RenderRange(Range range, Drawer.HighlightType ht)
	{
		foreach(Area a in areas) {
			a.RenderRange(range, ht);
		}
	}
	
	/// <summary>
	/// Blanks the view background
	/// </summary>
	private void BlankBackground()
	{
		foreach(Area a in areas) {
			a.BlankBackground();
		}
	}
	
	/// <summary>
	/// Breaks down a base highlight and produces atomic highlights 
	/// </summary>
	private IntervalTree<Highlight> BreakDownHighlights(Highlight s, IList<Highlight> lst)
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
					// Keep only common parts to avoid duplications.
					// This also has the useful side effect that everything
					// is clipped inside s
					h.Intersect(q); 
					if (!h.IsEmpty())
						it.Insert(h);
				}	
			}
		}
		
		return it;
	}
	
	/// <summary>
	/// Gets the atomic highlight ranges of the current view.
	/// (Non-overlapping ranges that describe the highlighting of the whole view)
	/// </summary>
	private IntervalTree<Highlight> GetAtomicHighlights()
	{
		int nrows;
		Range clip = GetViewRange(out nrows);
		Highlight view = new Highlight(clip, Drawer.HighlightType.Normal);
		
		// get all highlights in current view
		IList<Highlight> viewableHighlights = highlights.SearchOverlap(view);
		
		return BreakDownHighlights(view, viewableHighlights);
	}
	
	private void RenderAtomicHighlights(IntervalTree<Highlight> atomicHighlights)
	{
		IList<Highlight> hl = atomicHighlights.GetValues(); 
			
		foreach(Highlight h in hl) {
			RenderRange(h, h.Type);
		}
	}
	
	/// <summary>
	/// Render the whole view.
	/// </summary>
	/// <param name="atomicHighlights">
	/// The current atomic highlight ranges
	/// </param>
	private void RenderAll(IntervalTree<Highlight> atomicHighlights)
	{
		// blank the background
		BlankBackground();
	
		RenderAtomicHighlights(atomicHighlights);
		
	}
	
	/// <summary>
	/// Render the new highlights taking into consideration the old highlights
	/// (this means that only the differences are actually rendered)
	/// </summary>
	private void RenderHighlightDiffs(IntervalTree<Highlight> atomicHighlights)
	{
		IList<Highlight> hl = atomicHighlights.GetValues();
		
		foreach(Highlight h in hl) {
			IList<Highlight> overlaps = prevAtomicHighlights.SearchOverlap(h);
			foreach(Highlight overlap in overlaps) {
				if (overlap.Type != h.Type) {
					Highlight h1 = new Highlight(h);
					h1.Intersect(overlap);
					RenderRange(h1, h1.Type);
				}
			}
		}
	}
	
	/// <summary>
	/// Wrapper around Render(false) <see cref="Render(bool)"/>
	/// </summary>
	public void Render()
	{
		Render(false);
	}
	
	/// <summary>
	/// Render this area group.
	/// </summary>
	/// <param name="force">
	/// Whether to force a complete redraw of the group.
	/// </param>
	/// <remarks>
	/// If force is false this method tries to redraw as little
	/// as possible by drawing only the parts of the screen that
	/// have changed (eg when changing the selection)
	/// </remarks>
	public void Render(bool force)
	{
		
		// if we are forced to redraw but nothing
		// has changed, just redraw the previous atomic highlights
		if (force && !HasAnythingChanged()) {
			RenderAtomicHighlights(prevAtomicHighlights);
			return;
		}
		
		// get the current atomic highlights
		IntervalTree<Highlight> atomicHighlights = GetAtomicHighlights();
		
		// if we are forced to redraw or the view has scrolled (the offset has changed)
		// redraw everything
		if (force || HasChanged(Changes.Offset)) {
			RenderAll(atomicHighlights);
		} // otherwise redraw only what is needed
		else if (HasChanged(Changes.Highlights)) {
			RenderHighlightDiffs(atomicHighlights);
		}
		
		// update prevAtomicHighlights
		prevAtomicHighlights = atomicHighlights;
		
		ClearChanges();
	}

}

} // end namespace