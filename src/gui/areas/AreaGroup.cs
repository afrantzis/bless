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
/// An atomic highlight
/// </summary>
class AtomicHighlight : Highlight
{
	Highlight parent;
	
	public Highlight Parent {
		get { return parent; }
	}
	
	/// <summary>
	/// 
	/// </summary>
	public Area.RenderMergeFlags GetMergeFlags()
	{
		Area.RenderMergeFlags f = Area.RenderMergeFlags.None;
		
		if (this.Start > parent.Start)
			f |= Area.RenderMergeFlags.Left;
		
		if (this.End < parent.End)
			f |= Area.RenderMergeFlags.Right;
		
		return f;
		
	}
	
	public AtomicHighlight(Highlight parent) : base(parent)
	{
		this.parent = parent;
		
	}
	
	public AtomicHighlight(AtomicHighlight h) : base(h)
	{
		this.parent = h.parent;	
	}
}

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
	bool manualDoubleBuffer;
	
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
	IntervalTree<AtomicHighlight> prevAtomicHighlights;
	
	
	Highlight selection;
	
	public IList<Area> Areas {
		get { return areas; }
	}
	
	public long Offset {
		get { return offset;}
		set {
			if (offset == value)
				return;
			prevOffset = offset; 
			offset = value; 
			SetChanged(Changes.Offset);
		}
		
	}

	public long CursorOffset {
		get { return cursorOffset;}
		set {
			if (cursorOffset == value)
				return;
			prevCursorOffset = cursorOffset;
			cursorOffset = value;
			SetChanged(Changes.Cursor);
		}
		
	}
	
	public int CursorDigit {
		get { return cursorDigit;}
		set {
			if (cursorDigit == value)
				return;
			prevCursorDigit = cursorDigit;
			cursorDigit = value; 
			SetChanged(Changes.Cursor);
		}
		
	}
	
	public long PrevCursorOffset {
		get { return prevCursorOffset;}
	}
	
	public ByteBuffer Buffer {
		get { return byteBuffer; }
		set { byteBuffer = value; SetChanged(Changes.Offset);}
	}
	
	public Gtk.DrawingArea DrawingArea {
		get { return drawingArea; }
		set { drawingArea = value; }
	}
	
	public Range Selection {
		get { return selection; }
		set { 
			if (selection == value)
				return;
			highlights.Delete(selection);
			selection.Start = value.Start; selection.End = value.End;
			highlights.Insert(selection);
			SetChanged(Changes.Highlights);
		}
	}
	
	internal bool ManualDoubleBuffer {
		get { return manualDoubleBuffer; }
	}
	
	public AreaGroup()
	{
		areas = new System.Collections.Generic.List<Area>();
		highlights = new IntervalTree<Highlight>();
		selection = new Highlight(Drawer.HighlightType.Selection);
		prevAtomicHighlights = new IntervalTree<AtomicHighlight>();
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
	///<remarks>This causes the group to be rendered again.</remarks>
	private void SetChanged(Changes c)
	{
		changes |= c;
		
		if (drawingArea == null)
			return;
			
		if (HasChanged(Changes.Offset))
			drawingArea.QueueDraw();
		else
			ExposeManually();
		
	}
	
	/// <summary>
	/// Render this group, manually handling the double buffering
	/// </summary>
	private void ExposeManually()
	{
		manualDoubleBuffer = true;
		Render(false);
		manualDoubleBuffer = false;
	}
	
	/// <summary>
	/// Invalidate this group (visually). This forces a complete redraw
	/// on the next Render().
	/// </summary>
	public void Invalidate()
	{
		changes |= Changes.Offset;
	}
	
	/// <summary>
	/// Adds a highlight on a range of data.
	/// </summary>
	public void AddHighlight(long start, long end, Drawer.HighlightType ht)
	{
		highlights.Insert(new Highlight(start, end, ht));
	}
	
	public void ClearHighlights()
	{
		//highlights.Clear();
	}

	/// <summary>
	/// Renders the extra (data independent) portions of the view 
	/// </summary>
	private void RenderExtra()
	{
		foreach(Area a in areas) {
			a.RenderExtra();
		}
	}
	/// <summary>
	/// Renders a <see cref="Range"/> of data using a specified <see cref="Drawer.HighlightType"/>
	/// </summary>
	private void RenderHighlight(Highlight h, Area.RenderMergeFlags f)
	{
		foreach(Area a in areas) {
			a.RenderHighlight(h, f);
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
	private IntervalTree<AtomicHighlight> BreakDownHighlights(Highlight s, IList<Highlight> lst)
	{
		IntervalTree<AtomicHighlight> it = new IntervalTree<AtomicHighlight>();
		
		it.Insert(new AtomicHighlight(s));
		
		foreach(Highlight r in lst) {
			IList<AtomicHighlight> overlaps = it.SearchOverlap(r);
			foreach(AtomicHighlight q in overlaps) {
				it.Delete(q);
				AtomicHighlight[] ha = new AtomicHighlight[3]{new AtomicHighlight(q), new AtomicHighlight(r), new AtomicHighlight(q)};
				Range.SplitAtomic(ha, q, r);
				foreach(AtomicHighlight h in ha) {
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
	private IntervalTree<AtomicHighlight> GetAtomicHighlights()
	{
		int nrows;
		Range clip = GetViewRange(out nrows);
		Highlight view = new Highlight(clip, Drawer.HighlightType.Normal);
		
		// get all highlights in current view
		IList<Highlight> viewableHighlights = highlights.SearchOverlap(view);
		
		return BreakDownHighlights(view, viewableHighlights);
	}
	
	/// <summary>
	/// Renders the area group based on the specified atomic highlights.
	/// </summary>
	private void RenderAtomicHighlights(IntervalTree<AtomicHighlight> atomicHighlights)
	{
		IList<AtomicHighlight> hl = atomicHighlights.GetValues();
		
		foreach(AtomicHighlight h in hl) {
			RenderHighlight(h, h.GetMergeFlags());
		}
	}
	
	/// <summary>
	/// Render the whole view.
	/// </summary>
	/// <param name="atomicHighlights">
	/// The current atomic highlight ranges
	/// </param>
	private void RenderAll(IntervalTree<AtomicHighlight> atomicHighlights)
	{
		// blank the background
		BlankBackground();
		
		RenderExtra();
		
		RenderAtomicHighlights(atomicHighlights);
		
	}
	
	/// <summary>
	/// Render the new highlights taking into consideration the old highlights
	/// (this means that only the differences are actually rendered)
	/// </summary>
	private void RenderHighlightDiffs(IntervalTree<AtomicHighlight> atomicHighlights)
	{
		IList<AtomicHighlight> hl = atomicHighlights.GetValues();
		
		foreach(AtomicHighlight h in hl) {
			IList<AtomicHighlight> overlaps = prevAtomicHighlights.SearchOverlap(h);
			foreach(AtomicHighlight overlap in overlaps) {
				if (overlap.Type != h.Type) {
					AtomicHighlight h1 = new AtomicHighlight(h);
					h1.Intersect(overlap);
					RenderHighlight(h1, h1.GetMergeFlags());
				}
			}
		}
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
		
		/* This breaks the RenderExtra() optimizations in OffsetArea and SeparatorArea
		
		// if we are forced to redraw but nothing
		// has changed, just redraw the previous atomic highlights
		if (force && !HasAnythingChanged()) {
			//System.Console.WriteLine("Not changed");
			RenderAtomicHighlights(prevAtomicHighlights);
			return;
		}*/
		
		// get the current atomic highlights
		IntervalTree<AtomicHighlight> atomicHighlights = GetAtomicHighlights();
		
		// if we are forced to redraw or the view has scrolled (the offset has changed)
		// redraw everything
		if (force || HasChanged(Changes.Offset)) {
			//System.Console.WriteLine("Changed");
			RenderAll(atomicHighlights);
		} // otherwise redraw only what is needed
		
		else if (HasChanged(Changes.Highlights)) {
			//System.Console.WriteLine("Diffs");
			RenderHighlightDiffs(atomicHighlights);
		}
		
		// update prevAtomicHighlights
		prevAtomicHighlights = atomicHighlights;
		
		ClearChanges();
	}

}

} // end namespace