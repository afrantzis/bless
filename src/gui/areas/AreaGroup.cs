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
	IList<Highlight> containers;
	
	public IList<Highlight> Containers {
		get { return containers; }
	}
	
	/// <summary>
	/// Adds a highlight as a container of this atomic highlight
	/// </summary>
	/// <remarks>The highlight is added only if it has lower or equal priority than this highlight</remarks>
	public void AddContainer(Highlight h)
	{
		if (h.Type > type)
			return;
		
		// insertion sort (higher priority first)
		for (int i = 0; i < containers.Count; i++) {
			if (h.Type >= containers[i].Type) {
				containers.Insert(i, h);
				return;
			}
		}
		
		containers.Add(h);
	}
	
	/// <summary>
	/// 
	/// </summary>
	public void GetAbyssHighlights(out Drawer.HighlightType left, out Drawer.HighlightType right)
	{		
		left = Drawer.HighlightType.Sentinel;
		right = Drawer.HighlightType.Sentinel;

		foreach(Highlight h in containers) {
			if (left == Drawer.HighlightType.Sentinel && h.Contains(start - 1))
				left = h.Type;
			
			if (right == Drawer.HighlightType.Sentinel && h.Contains(end + 1))
				right = h.Type;
		}
		
		if (left == Drawer.HighlightType.Sentinel)
			left = Drawer.HighlightType.Normal;
		
		if (right == Drawer.HighlightType.Sentinel)
			right = Drawer.HighlightType.Normal;
	}
	
	public AtomicHighlight(Highlight parent) : base(parent)
	{
		this.containers = new System.Collections.Generic.List<Highlight>(1);
		this.containers.Add(parent);	
	}
	
	public AtomicHighlight(AtomicHighlight h) : base(h)
	{
		this.containers = new System.Collections.Generic.List<Highlight>(h.containers);
	}
	
	public override string ToString()
	{
		string str =  base.ToString() + " Containers: ";
		
		foreach(Highlight h in containers) {
			str += h.ToString() + " ";
		}
		
		return str;
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
	Area focusedArea;
	
	IntervalTree<Highlight> highlights;
	
	enum Changes { Offset = 1, Cursor = 2, Highlights = 4}
	
	Changes changes;
	bool manualDoubleBuffer;
	
	// current offset of view in the buffer
	long offset; 
	
	// current cursor
	long cursorOffset;
	
	// track changes
	long prevCursorOffset;
	
	byte[] bufferCache;
	
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
	
	public Area FocusedArea {
		get { return focusedArea; }
		set { UpdateFocusedArea(value); }
	}
	
	public long Offset {
		get { return offset;}
		set {
			if (offset == value)
				return;
			offset = value; 
			SetChanged(Changes.Offset);
		}
		
	}

	public long CursorOffset {
		get { return cursorOffset;}
	}
	
	public int CursorDigit {
		get {
			if (focusedArea != null)
				return focusedArea.CursorDigit;
			else
				return 0;
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
			
			// make sure the cursor is also updated (because it may 
			// not have changed position so SetCursor() won't render it 
			// but it may have to become visible again eg when a selection is cleared)
			SetChanged(Changes.Highlights | Changes.Cursor);
		}
	}
	
	internal bool ManualDoubleBuffer {
		get { return manualDoubleBuffer; }
	}
	
	public void SetCursor(long coffset, int cdigit)
	{
		prevCursorOffset = cursorOffset;
		
		// if there is no change ignore...
		if (cursorOffset == coffset && this.CursorDigit == cdigit)
			return;
		
		cursorOffset = coffset;
		foreach(Area a in areas)
			a.CursorDigit = cdigit;
		
		SetChanged(Changes.Cursor);
	}
	
	public byte GetCachedByte(long pos)
	{
		return bufferCache[pos - offset];
	}
	
	public AreaGroup()
	{
		areas = new System.Collections.Generic.List<Area>();
		highlights = new IntervalTree<Highlight>();
		selection = new Highlight(Drawer.HighlightType.Selection);
		prevAtomicHighlights = new IntervalTree<AtomicHighlight>();
		bufferCache = new byte[0];
	}
	
	
	/// <summary>
	/// Get the range of bytes and the number of rows that 
	/// are displayed in the current view.
	/// </summary>
	public Range GetViewRange(out int nrows)
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
		
		if (drawingArea == null || drawingArea.GdkWindow == null)
			return;
			
		if (HasChanged(Changes.Offset)) {
			Gdk.Rectangle view = drawingArea.Allocation;
			view.X = 0;
			view.Y = 0;
			drawingArea.GdkWindow.BeginPaintRect(view);
			Render(false);
			drawingArea.GdkWindow.EndPaint();
		}
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
	/// Invalidate this group (visually). This forces a complete redraw
	/// on the next Render().
	/// </summary>
	public void RedrawNow()
	{
		System.Console.WriteLine("Redraw Now");
		SetChanged(Changes.Offset);
	}
	
	private void InitializeHighlights()
	{
		ClearHighlights();
		if (!selection.IsEmpty())
			highlights.Insert(selection);
	}
	
	/// <summary>
	/// Adds a highlight on a range of data.
	/// </summary>
	public void AddHighlight(long start, long end, Drawer.HighlightType ht)
	{
		highlights.Insert(new Highlight(start, end, ht));
		changes |= Changes.Highlights;
	}
	
	private void ClearHighlights()
	{
		highlights.Clear();
	}
	
	private void SetupBufferCache()
	{
		int nrows;
		Range view = GetViewRange(out nrows);
		if (view.Size != bufferCache.Length)
			bufferCache = new byte[view.Size];
		
		for(int i = 0; i < view.Size; i++)
			bufferCache[i] = byteBuffer[view.Start + i];
	}
	
	public void CycleFocus()
	{
		int faIndex; 
		for (faIndex = 0; faIndex < areas.Count; faIndex++)
			if (focusedArea == areas[faIndex])
				break;
		
		if (faIndex >= areas.Count)
			faIndex = -1;
		
		int end = faIndex + areas.Count;
		
		// use < instead of != so this will work correctly
		// even when faIndex = -1 in which case we should check
		// all areas (instead of only the rest areas.Count - 1 
		// areas when there is already a focused area)
		for (faIndex++; faIndex < end; faIndex++) {
			Area a = (areas[faIndex%areas.Count] as Area);
			if (a.CanFocus == true) {
				UpdateFocusedArea(a);
				return;
			}
		}
		
		focusedArea = null;
	}
	
	private void UpdateFocusedArea(Area fa)
	{
		focusedArea = fa;
		
		foreach(Area a in areas)
			a.HasCursorFocus = false;
		
		focusedArea.HasCursorFocus = true;
		
		// set the previous cursor so that when
		// the screen is rendered the byte under the
		// cursor is properly cleared (before being drawn
		// again)
		prevCursorOffset = cursorOffset;
		
		SetChanged(Changes.Cursor);
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
	private void RenderHighlight(AtomicHighlight h)
	{
		Drawer.HighlightType left;
		Drawer.HighlightType right;
		h.GetAbyssHighlights(out left, out right);
		
		//System.Console.WriteLine("  Rendering {0}  ({1} {2})", h, left, right);
		foreach(Area a in areas) {
			a.RenderHighlight(h, left, right);
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
	
	private AtomicHighlight[] SplitAtomicPrioritized(AtomicHighlight q, Highlight r)
	{
		AtomicHighlight[] ha;
		
		if (q.Type > r.Type) {
			ha = new AtomicHighlight[3]{new AtomicHighlight(r), new AtomicHighlight(q), new AtomicHighlight(r)};
			Range.SplitAtomic(ha, r, q);
			ha[1].AddContainer(r);
		}
		else {
			ha = new AtomicHighlight[3]{new AtomicHighlight(q), new AtomicHighlight(r), new AtomicHighlight(q)};
			Range.SplitAtomic(ha, q, r);
			foreach (Highlight h in q.Containers)
				ha[1].AddContainer(h);
		}
		
		return ha;
	}
	
	/// <summary>
	/// Breaks down a base highlight and produces atomic highlights 
	/// </summary>
	private IntervalTree<AtomicHighlight> BreakDownHighlights(Highlight s, IList<Highlight> lst)
	{
		//System.Console.WriteLine("breaking down {0}", s);
		IntervalTree<AtomicHighlight> it = new IntervalTree<AtomicHighlight>();
		
		if (!s.IsEmpty())
			it.Insert(new AtomicHighlight(s));
		
		foreach(Highlight r in lst) {
			//System.Console.WriteLine("  Processing {0}", r);
			IList<AtomicHighlight> overlaps = it.SearchOverlap(r);
			foreach(AtomicHighlight q in overlaps) {
				it.Delete(q);
				//System.Console.WriteLine("    Overlap {0}", q);
				AtomicHighlight[] ha = SplitAtomicPrioritized(q, r);			
				foreach(AtomicHighlight h in ha) {
					// Keep only common parts to avoid duplications.
					// This also has the useful side effect that everything
					// is clipped inside s
					h.Intersect(q);
					//System.Console.WriteLine("      Atomic {0}", h);
					if (!h.IsEmpty()) {
						it.Insert(h);
					}
				}	
			}
		}
		
		//foreach(AtomicHighlight ah in it.GetValues()) {
		//	System.Console.WriteLine("  " + ah);
		///}
		
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
		
		foreach(Highlight h in viewableHighlights) {
			h.Intersect(view);
		}
		
		return BreakDownHighlights(view, viewableHighlights);
	}
	
	/// <summary>
	/// Renders the area group based on the specified atomic highlights.
	/// </summary>
	private void RenderAtomicHighlights(IntervalTree<AtomicHighlight> atomicHighlights)
	{
		IList<AtomicHighlight> hl = atomicHighlights.GetValues();
		
		foreach(AtomicHighlight h in hl) {
			RenderHighlight(h);
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
		SetupBufferCache();
		
		// blank the background
		BlankBackground();
		
		RenderExtra();
		
		RenderAtomicHighlights(atomicHighlights);
		
		RenderCursor(atomicHighlights);
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
				bool diffType = overlap.Type != h.Type;
				
				Drawer.HighlightType left, right, oleft, oright;
								
				h.GetAbyssHighlights(out left, out right);
				overlap.GetAbyssHighlights(out oleft, out oright);
				
				bool diffAbyss = (left != oleft) || (right != oright);
				
				if (diffType || diffAbyss) {
					AtomicHighlight h1 = new AtomicHighlight(h);
					h1.Intersect(overlap);
					//System.Console.Write(diffType?"DiffType> ":"DiffFlags> ");
					RenderHighlight(h1);
				}
			}
		}
	}
	
	private void RenderCursor(IntervalTree<AtomicHighlight> atomicHighlights)
	{
		// find the kind of highlight the cursor was previously on
		// if we don't find an overlap this means that either
		// 1. the prev cursor position is not visible on the screen
		// 2. the prev cursor position is at the end of the file
		IList<AtomicHighlight> overlaps = atomicHighlights.SearchOverlap(new Range(prevCursorOffset, prevCursorOffset));
		
		AtomicHighlight h = null;
		
		// if we find an overlap create a highlight
		// to use to restore the prev position
		if (overlaps.Count > 0) {
			h = new AtomicHighlight(overlaps[0]);
			h.Start = prevCursorOffset;
			h.End = prevCursorOffset;
		}
		
		bool prevCursorAtEof =  prevCursorOffset == byteBuffer.Size;
		
		if (h != null) {
			RenderHighlight(h);
		}
		else if (prevCursorAtEof) { // case 2
			foreach(Area a in areas)
				a.BlankEof();
		}
		
		if (selection.IsEmpty())
			foreach(Area a in areas)
				a.RenderCursor();
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
		InitializeHighlights();
		
		if (PreRenderEvent != null)
			PreRenderEvent(this);
		
		/* This breaks the RenderExtra() optimizations in OffsetArea and SeparatorArea
		
		// if we are forced to redraw but nothing
		// has changed, just redraw the previous atomic highlights
		if (force && !HasAnythingChanged()) {
			//System.Console.WriteLine("Not changed");
			RenderAtomicHighlights(prevAtomicHighlights);
			return;
		}*/
		
		// get the current atomic highlights
		IntervalTree<AtomicHighlight> atomicHighlights;
		
		// if atomic highlights have not changed, reuse them
		if (!force && !HasChanged(Changes.Highlights) && !HasChanged(Changes.Offset))
			atomicHighlights = prevAtomicHighlights; 
		else {
			//System.Console.WriteLine("Re-eval atomic highs");
			atomicHighlights = GetAtomicHighlights();
		}
		
		// if we are forced to redraw or the view has scrolled (the offset has changed)
		// redraw everything
		if (force || HasChanged(Changes.Offset)) {
			//System.Console.WriteLine("Scroll");
			RenderAll(atomicHighlights);
		} // otherwise redraw only what is needed
		else if (HasChanged(Changes.Highlights)) {
			//System.Console.WriteLine("Diffs");
			RenderHighlightDiffs(atomicHighlights);
		}
		
		if (HasChanged(Changes.Cursor)) {
			//System.Console.WriteLine("Cursor");
			RenderCursor(atomicHighlights);
		}
		
		// update prevAtomicHighlights
		prevAtomicHighlights = atomicHighlights;
		
		ClearChanges();
	}
	
	public delegate void PreRenderHandler(AreaGroup ag);

	public event PreRenderHandler PreRenderEvent;
}

} // end namespace