// created on 12/13/2004 at 1:15 PM
/*
 *   Copyright (c) 2004, Alexandros Frantzis (alf82 [at] freemail [dot] gr)
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
 
using Gtk;
using Bless.Buffers;
using Bless.Gui.Areas;
using Bless.Util;

namespace Bless.Gui {

///<summary>Handles all dataview ui aspects except display</summary>
public class DataViewControl
{
	private struct Position
	{
		public long First;
		public long Second;
		public int Digit;
	}
	
	DataView dataView;
	DataViewDisplay dvDisplay;
	
	// mouse selection related
	Position selStartPos;
	Position selEndPos;
	
	// Filter keypresses
	Gtk.IMContext imContext;
	
	public DataViewDisplay Display {
		get { return dvDisplay;}
		set { dvDisplay=value;}
	}
	
	public DataView View {
		get { return dataView;}
	}
	
	public DataViewControl(DataView dv)
	{
		dataView=dv;
		
		selStartPos = new Position();
		selEndPos = new Position();
		
		imContext=new IMContextSimple();
	}
 
	///<summary>Get the area that contains (X,Y)</summary>
	private Area GetAreaByXY(int x, int y)
	{
		Area clickArea=null;
		
		foreach(Area a in dvDisplay.Layout.Areas) { 
			if ((x >= a.X) && (x <= a.X + a.Width)) {
				clickArea=a;
				break;
			}
		}
		
		return clickArea;
	}
	
	private void UpdateSelection(bool abyss)
	{
		//System.Console.WriteLine("* (1) Start: {0},{1},{2} End: {3},{4},{5}", selStartPos.First, selStartPos.Second, selStartPos.Digit, selEndPos.First, selEndPos.Second, selEndPos.Digit);
		Bless.Util.Range r;
		if (selStartPos.Second <= selEndPos.First)
			r = new Bless.Util.Range(selStartPos.Second, selEndPos.First);
		else
			r = new Bless.Util.Range(selEndPos.Second, selStartPos.First);
		
		//System.Console.WriteLine("Selection is ({0}, {1}) Expected ({2}, {3})", dataView.Selection.Start, dataView.Selection.End, r.Start, r.End);
		// if nothing is selected and cursor position has changed externally
		if (dataView.Selection.IsEmpty() && dataView.CursorOffset != selStartPos.Second) {
			long offset = dataView.CursorOffset;
			selStartPos.First = offset - (abyss?1:0);
			selStartPos.Second = offset;
			selStartPos.Digit = dataView.CursorDigit;
			selEndPos = selStartPos;
			//System.Console.WriteLine("* (2) Start: {0},{1},{2} End: {3},{4},{5}", selStartPos.First, selStartPos.Second, selStartPos.Digit, selEndPos.First, selEndPos.Second, selEndPos.Digit);
		}
		else if (!dataView.Selection.IsEmpty() && !r.Equals(dataView.Selection)) {
			selStartPos.Second = dataView.Selection.Start;
			selStartPos.First = selStartPos.Second - 1;
			selEndPos.First = dataView.Selection.End;
			selEndPos.Second = selEndPos.First + 1; 
		}
		//System.Console.WriteLine("* Selection is ({0}, {1}) Expected ({2}, {3}) Fixed ({4}, {5})", dataView.Selection.Start, dataView.Selection.End, r.Start, r.End, selStartPos.Second, selEndPos.First);
		
	}
	
	private bool UpdateFocus(Area area)
	{
		if (area.CanFocus && !area.HasCursorFocus) {

			foreach(Area a in dvDisplay.Layout.Areas) 
				a.HasCursorFocus=false;
			
			dataView.MoveCursor(dataView.CursorOffset, dataView.CursorDigit);
			
			area.HasCursorFocus=true;
			area.MoveCursor(area.CursorOffset, area.CursorDigit);
			return true;
		}
		
		return false;
	}
	
 	private void CalculatePosition(Area area, int x, int y, ref Position pos)
 	{
 		Area.GetOffsetFlags flags;
		int digit;
 		
 		long off2;
		long off1 = area.GetOffsetByDisplayInfo(x, y, out digit, out flags);
		
		if ((flags & Area.GetOffsetFlags.Eof) == Area.GetOffsetFlags.Eof) {
			off1 = dataView.Buffer.Size;
			off2 = off1;
		}
		else if ((flags & Area.GetOffsetFlags.Abyss) == Area.GetOffsetFlags.Abyss) {
			off2 = off1 + 1;
 		}
 		else
 			off2 = off1;
 		
 		pos.First = off1;
 		pos.Second = off2;
 		pos.Digit = digit;
 	}
 	
	private long ValidateOffset(long offset)
	{
		if (offset < 0)
			return 0;
		
		if (offset >= dataView.Buffer.Size)
			return dataView.Buffer.Size - 1;
		
		return offset;
	}
	
	///<summary>
	/// Sets the selection and the cursor position according to the values of selStartPos, selEndPos 
	///</summary>
	private void EvaluateSelection(DataViewDisplay.ShowType showType)
	{	
		long cursorOffset;
		
		// no selection
		if (selStartPos.First == selEndPos.First && selStartPos.Second == selEndPos.Second) {
			cursorOffset = selStartPos.Second;
			// make sure cursor (or end of selection) is visible
			dvDisplay.MakeOffsetVisible(cursorOffset, showType);
			
			dataView.SetSelection(-1, -1);
			dataView.MoveCursor(selStartPos.Second, selEndPos.Digit);
		}
		// selection with start pos <= end pos
		else if (selStartPos.Second <= selEndPos.First) {
			// set end position between bytes
			selEndPos.Second = selEndPos.First + 1;
			
			// if selEndPos.Second is at or beyond the EOF
			long off = ValidateOffset(selEndPos.Second);
			if (selEndPos.First >= off)
				off++;
			cursorOffset = off;
			dvDisplay.MakeOffsetVisible(cursorOffset, showType);
			
			dataView.SetSelection(ValidateOffset(selStartPos.Second), ValidateOffset(selEndPos.First));
			dataView.MoveCursor(off, 0);
		}
		// selection with start pos > end pos
		else {
			long off = ValidateOffset(selEndPos.Second);
			cursorOffset = off;
			dvDisplay.MakeOffsetVisible(cursorOffset, showType);
			
			dataView.SetSelection(ValidateOffset(selEndPos.Second), ValidateOffset(selStartPos.First));
			dataView.MoveCursor(off, 0);
		}
		
		
	}
	
	///<summary>Handle mouse button presses</summary>
	internal void OnButtonPress (object o, ButtonPressEventArgs args)
	{
		dvDisplay.GrabKeyboardFocus();
	
		Gdk.EventButton e = args.Event;
		Area clickArea = GetAreaByXY((int)e.X, (int)e.Y);
		
		if (clickArea == null)
			return;
		
		// get the position in the file
		Position pos = new Position();
		
		CalculatePosition(clickArea, (int)(e.X-clickArea.X), (int)(e.Y-clickArea.Y), ref pos);
		
		// update selection and cursor if they have changed externally
		UpdateSelection(pos.First != pos.Second);
		
		// if shift is pressed the position is the end position of the selection
		if ((e.State & Gdk.ModifierType.ShiftMask) != 0) {
			selEndPos = pos;
		}
		else { // ... start a new selection
			selStartPos = pos;
			selEndPos = pos;
		}
		
		EvaluateSelection(DataViewDisplay.ShowType.Closest);
		
		// give the focus to the appropriate area
		UpdateFocus(clickArea);
	}
	
	///<summary>Handle mouse motion</summary>
	internal void OnMotionNotify(object o, MotionNotifyEventArgs args)
	{
		Gdk.EventMotion e=args.Event;
		Gtk.DrawingArea da=(Gtk.DrawingArea)o;
		int x,y;
		Gdk.ModifierType state;
		
		if (e.IsHint)
			da.GdkWindow.GetPointer(out x, out y, out state);
		else {
			x = (int)e.X;
			y = (int)e.Y;
			state = e.State;
		}
		
		// if left mouse button is not down
		if ((state & Gdk.ModifierType.Button1Mask) == 0)
			return;
		
		// find in which area the pointer is
		Area clickArea=GetAreaByXY(x, y);
					
		if (clickArea == null)
			return;
		
		// get the position in the file
		Position pos = new Position();
		CalculatePosition(clickArea, (int)(x-clickArea.X), (int)(y-clickArea.Y), ref pos);
		
		// update selection and cursor if they have changed externally
		UpdateSelection(pos.First != pos.Second);		
		
		// Evaluate selection before moving the cursor
		// for better visual result
		selEndPos = pos;
		
		EvaluateSelection(DataViewDisplay.ShowType.Closest);
		
		// give the focus to the appropriate area
		UpdateFocus(clickArea);
	}
	
	///<summary>Handle mouse button release</summary>
	internal void OnButtonRelease (object o, ButtonReleaseEventArgs args)
	{
		Gdk.EventButton e=args.Event;
		Area clickArea=GetAreaByXY((int)e.X, (int)e.Y);
		
		if (clickArea == null)
			return;
		
		int x=(int)e.X;
		int y;
		// if the pointer has moved out of the area height borders
		// make sure there is consistency in the selection
		// when depressing the mouse button
		if (e.Y > clickArea.Height + clickArea.Y) 
			y=clickArea.Height + clickArea.Y - clickArea.Drawer.Height;
		else if (e.Y < clickArea.Y)
			y=clickArea.Y;
		else
			y=(int)e.Y;
		
		// get the position in the file
		Position pos = new Position();
		CalculatePosition(clickArea, (int)(x-clickArea.X), (int)(y-clickArea.Y), ref pos);
		
		// update selection and cursor if they have changed externally
		UpdateSelection(pos.First != pos.Second);		
		
		// Evaluate selection before moving the cursor
		// for better visual result
		selEndPos = pos;
		//System.Console.WriteLine("Start: {0},{1},{2} End: {3},{4},{5}", selStartPos.First, selStartPos.Second, selStartPos.Digit, selEndPos.First, selEndPos.Second, selEndPos.Digit);
		EvaluateSelection(DataViewDisplay.ShowType.Closest);
		
		// give the focus to the appropriate area
		UpdateFocus(clickArea);										
	}
	
	///<summary>Find the area that has the focus and its index</summary>
	internal int FindFocusedArea(out Area focusArea)
	{
		int faIndex=0;
		focusArea=null;
		
		for (; faIndex < dvDisplay.Layout.Areas.Count; faIndex++) {
			if ((dvDisplay.Layout.Areas[faIndex] as Area).HasCursorFocus==true) {
				focusArea=(dvDisplay.Layout.Areas[faIndex] as Area);
				break;
			}
		}
		if (focusArea!=null)
			return faIndex;
		else
			return -1;
	}
	
	///<summary>Give the focus to the next applicable area</summary>
	Area CycleFocus()
	{
		Area focusArea=null;
		int faIndex=FindFocusedArea(out focusArea);
		
		int start=faIndex;
		int areaCount=dvDisplay.Layout.Areas.Count;
		
		for (faIndex++; (faIndex%areaCount)!=start; faIndex++) {
			Area a=(dvDisplay.Layout.Areas[faIndex%areaCount] as Area);
			if (a.CanFocus==true) {
				a.HasCursorFocus=true;
				if (focusArea!=null)
					focusArea.HasCursorFocus=false;
				dataView.MoveCursor(dataView.CursorOffset, dataView.CursorDigit);
				return a;
			}
		}
		
		return null;
	}
	
	// variables that are shared by OnKeyPress and OnKey* handler methods
	Area okp_focusArea;
	int okp_bpr;
	int okp_dpb;
	DataViewDisplay.ShowType okp_showType;
		
	///<summary>Handle key presses</summary>
	internal void OnKeyPress (object o, KeyPressEventArgs args)
	{
		Gdk.EventKey e = args.Event;
		
		okp_focusArea = null;
		bool shiftPressed = false;
		
		// find focused area
		FindFocusedArea(out okp_focusArea);
		
		// if no area has the focus, give it to the first one applicable
		if (okp_focusArea == null)
			okp_focusArea=CycleFocus();
		
		
		// if still no area got the focus give up
		if (okp_focusArea == null)
			return;
		
		okp_bpr = okp_focusArea.BytesPerRow;
		okp_dpb = okp_focusArea.DigitsPerByte;
		okp_showType = DataViewDisplay.ShowType.Closest;
		
		if ((e.State & Gdk.ModifierType.ShiftMask) == Gdk.ModifierType.ShiftMask)
			shiftPressed = true;
		
		UpdateSelection(true);
		
		Position cur = new Position();
		if (shiftPressed || !okp_focusArea.Selection.IsEmpty()) {
			cur = selEndPos;
			cur.Digit = 0;
		}
		else {
			cur = selStartPos;
		}
		
		Position next = new Position();
		// set next to cur in case it is not set by a keypress handler
		next = cur;
		
		// handle keys
		bool specialKey = false;
		
		switch(e.Key) {
			case Gdk.Key.Up:
				OnKeyUp(ref cur, ref next); 
				break;
			case Gdk.Key.Down:
				OnKeyDown(ref cur, ref next);
				break;
			case Gdk.Key.Left:
				OnKeyLeft(ref cur, ref next);
				break;
			case Gdk.Key.Right:
				if (shiftPressed)
					cur.Digit = okp_dpb - 1;
				OnKeyRight(ref cur, ref next);
				break;
			case Gdk.Key.Page_Up:
				OnKeyPageUp(ref cur, ref next);
				break;
			case Gdk.Key.Page_Down:
				OnKeyPageDown(ref cur, ref next);
				break;
			case Gdk.Key.Home:
				OnKeyHome(ref cur, ref next);
				break;
			case Gdk.Key.End:
				OnKeyEnd(ref cur, ref next);
				break;
			case Gdk.Key.Insert:
				OnKeyInsert();
				break;
			case Gdk.Key.Tab:
				OnKeyTab();
				specialKey = true;
				break;
			case Gdk.Key.BackSpace:
				OnKeyBackspace();
				return; // OnKeyBackspace() handles drawing
			default:
				OnKeyDefault(e, ref cur, ref next, out specialKey); 
				break;
		} // end switch()
		//System.Console.WriteLine("Current: {0},{1},{2} Next: {3},{4},{5}", cur.First, cur.Second, cur.Digit, next.First, next.Second, next.Digit);
		
		// if just a special key was pressed (eg shift, ctrl, tab or a digit the area doesn't understand don't do anything) 
		if (specialKey)
			return;
		
		if (shiftPressed) {
			next.Digit = 0;
			if (selStartPos.First == selStartPos.Second)
				selStartPos.First--;
			selEndPos = next;
		}
		else {
			selEndPos = next;
			selStartPos = next;
		}
		//System.Console.WriteLine("Start: {0},{1},{2} End: {3},{4},{5}", selStartPos.First, selStartPos.Second, selStartPos.Digit, selEndPos.First, selEndPos.Second, selEndPos.Digit);
		EvaluateSelection(okp_showType);		
	}
	
	///<summary>Handle key releases</summary>
	internal void OnKeyRelease (object o, KeyReleaseEventArgs args)
	{
		// do nothing
	}
	
	internal void OnMouseWheel(object o, ScrollEventArgs args)
	{
		Gdk.EventScroll e=args.Event;
		
		if (e.Direction == Gdk.ScrollDirection.Down) {
			double newValue=dvDisplay.VScroll.Value + dvDisplay.VScroll.Adjustment.StepIncrement;
			if (newValue <= dvDisplay.VScroll.Adjustment.Upper - dvDisplay.VScroll.Adjustment.PageSize)
				dvDisplay.VScroll.Value=newValue;
			else
				dvDisplay.VScroll.Value=dvDisplay.VScroll.Adjustment.Upper - dvDisplay.VScroll.Adjustment.PageSize;
		}	
		else if (e.Direction == Gdk.ScrollDirection.Up) { 
			double newValue=dvDisplay.VScroll.Value - dvDisplay.VScroll.Adjustment.StepIncrement;
			if (newValue >= dvDisplay.VScroll.Adjustment.Lower)
				dvDisplay.VScroll.Value=newValue;
			else
				dvDisplay.VScroll.Value=dvDisplay.VScroll.Adjustment.Lower;
		 }
	}
	
	///<summary>
	/// Handle DataView focus gain 
	///</summary>
	internal void OnFocusInEvent (object o, FocusInEventArgs args)
	{
		foreach(Area a in dvDisplay.Layout.Areas)
			a.IsActive=true;
		
		dataView.FireFocusChangedEvent();
	}
	
	///<summary>
	/// Handle DataView focus loss 
	///</summary>
	internal void OnFocusOutEvent (object o, FocusOutEventArgs args)
	{
		foreach(Area a in dvDisplay.Layout.Areas)
			a.IsActive=false;
		
		dataView.FireFocusChangedEvent();
	}
	
	//
	// Key Handlers, called by OnKeyPress
	//
	
	void OnKeyUp(ref Position cur, ref Position next)
	{
		long offset = cur.Second;
		
		offset -= okp_bpr;
		
		if (offset < 0) {
			next.First = cur.Second;
			next.Second = cur.Second;
		}
		else {
			next.First = offset - 1;
			next.Second = offset;
		}
		
		next.Digit = cur.Digit;
	}
	
	void OnKeyDown(ref Position cur, ref Position next)
	{
		 long offset = cur.Second;
		 offset += okp_bpr;
		
		if (offset > dataView.Buffer.Size) {
			next.First = dataView.Buffer.Size;
			next.Second = dataView.Buffer.Size;
		}
		else {
			next.First = offset - 1;
			next.Second = offset;
		}
		
		next.Digit = cur.Digit;
	}
	
	void OnKeyLeft(ref Position cur, ref Position next)
	{
		long offset = cur.Second;
		int digit = cur.Digit;
		
		digit--;
		
		if (digit < 0) {
			offset--;
			digit = okp_dpb - 1;
		}
		
		if (offset < 0) {
			offset = 0;
			digit = 0;
		}
		
		next.First = offset - 1;
		next.Second = offset;
		next.Digit = digit;
		
	}
	
	void OnKeyRight(ref Position cur, ref Position next)
	{
		long offset = cur.Second;
		int digit = cur.Digit;
		
		digit++;
		
		if (digit >= okp_dpb) {
			offset++;
			digit = 0;
		}
		
		if (offset > dataView.Buffer.Size) {
			offset = dataView.Buffer.Size;
			digit = okp_dpb - 1;
		}
		
		next.First = offset - 1;
		next.Second = offset;
		next.Digit = digit;
		
	}
	
	
	void OnKeyPageUp(ref Position cur, ref Position next)
	{
		long offset = cur.Second;
		int digit = cur.Digit;
		
		offset -= okp_bpr * (int)dvDisplay.VScroll.Adjustment.PageIncrement;
		
		if (offset < 0) {
			offset = 0;
			//digit = 0;
		}
		
		okp_showType = DataViewDisplay.ShowType.Cursor;
				
		next.First = offset;
		next.Second = offset;
		next.Digit = digit;
		
	}
		
	void OnKeyPageDown(ref Position cur, ref Position next)
	{
		long offset = cur.Second;
		int digit = cur.Digit;
		
		offset += okp_bpr * (int)dvDisplay.VScroll.Adjustment.PageIncrement;
		
		if (offset > dataView.Buffer.Size) {
			offset = dataView.Buffer.Size;
			//digit = 0;
		}
		
		okp_showType = DataViewDisplay.ShowType.Cursor;
				
		next.First = offset;
		next.Second = offset;
		next.Digit = digit;
		
	}
		
	void OnKeyHome(ref Position cur, ref Position next)
	{
		next.First = 0;
		next.Second = 0;
		next.Digit = 0;
			
		okp_showType = DataViewDisplay.ShowType.Start;
	}
	
	void OnKeyEnd(ref Position cur, ref Position next)
	{
		next.First = dataView.Buffer.Size;
		next.Second = dataView.Buffer.Size;
		next.Digit = 0;
			
		okp_showType = DataViewDisplay.ShowType.End;
	}
	
	
	void OnKeyInsert()
	{
		dataView.Overwrite = !dataView.Overwrite;
	}
	
	void OnKeyTab()
	{
		CycleFocus();
	}
	
	void OnKeyBackspace()
	{
		// move one position backwards and delete
		dataView.DeleteBackspace();
	}
	
	void OnKeyDefault(Gdk.EventKey e, ref Position cur, ref Position next, out bool specialKey)
	{
		if (!dataView.Buffer.ModifyAllowed) {
			specialKey = true;
			return;
		}
		
		if (dataView.Selection.IsEmpty()) {
			if (imContext.FilterKeypress(e) && okp_focusArea.HandleKey(e.Key, dataView.Overwrite) == true) {
				OnKeyRight(ref cur, ref next);
				dataView.CursorUndoDeque.AddFront(new CursorState(cur.Second, cur.Digit, next.Second, next.Digit));
							
				dataView.CursorRedoDeque.Clear();	
				
				selStartPos = selEndPos = next;
				specialKey = false;
			}
			else
				specialKey = true;
		}
		else {
			if (imContext.FilterKeypress(e) && okp_focusArea.HandleKey(e.Key, dataView.Overwrite) == true) {
				dataView.Delete();
				specialKey = false;
			}
			else {
				specialKey = true;
			}
			
		}
		// any other key pass it to focused area
		// if area handled it move one position right
		
	}
	
 } // end DataViewControl
 
 } //end namespace