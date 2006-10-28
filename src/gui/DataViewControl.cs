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

namespace Bless.Gui {

///<summary>Handles all dataview ui aspects except display</summary>
public class DataViewControl
{
 
	DataView dataView;
	DataViewDisplay dvDisplay;
	
	// mouse selection related
	long selStart;
	bool selStartInAbyss;
	
	// keyboard selection related
	bool shiftPressed;
	
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
 
	///<summary>Sets up the selection while dragging the mouse</summary>
	private void EvaluateSelectionDrag(long okp_selEnd, bool abyss)
	{
		long newSelStart=selStart;
		long newokp_selEnd=okp_selEnd;
		
	
		if (okp_selEnd!=selStart ) {
			if (selStart==dataView.Buffer.Size && !selStartInAbyss)
				newSelStart--;
			// find if the selection end is at a greater or
			// lesser position than the start and adjust
			// the selection accordingly
			 if (okp_selEnd > selStart && abyss)
				newokp_selEnd--;
			 else if (okp_selEnd < selStart && selStartInAbyss)
				newSelStart--;
				
			if (newokp_selEnd==dataView.Buffer.Size)
				newokp_selEnd--;
			
			// make sure selection range is sorted
			if (newSelStart > newokp_selEnd)
				dataView.SetSelection(newokp_selEnd, newSelStart);
			else
				dataView.SetSelection(newSelStart, newokp_selEnd);
			
			if (okp_selEnd >= selStart && !abyss && okp_selEnd<dataView.Buffer.Size) {
				dataView.MoveCursor(okp_selEnd+1, 0);
			}
		}
		else if (((abyss && !selStartInAbyss) || (!abyss && selStartInAbyss)) && selStart!=dataView.Buffer.Size) {
			dataView.SetSelection(selStart, okp_selEnd); // no need to sort, selStart==okp_selEnd
			
			if (okp_selEnd >= selStart && !abyss && okp_selEnd<dataView.Buffer.Size) {
				dataView.MoveCursor(okp_selEnd+1, 0);
			}
		}
		else {
			dataView.SetSelection(-1,-1);
		}		
	}
	
	///<summary>Handle mouse button presses</summary>
	internal void OnButtonPress (object o, ButtonPressEventArgs args)
	{
		dvDisplay.GrabKeyboardFocus();
	
		Gdk.EventButton e=args.Event;
		Area clickArea=GetAreaByXY((int)e.X, (int)e.Y);
		
		if (clickArea!=null) {
			Area.GetOffsetFlags flags;
			int digit;
			long off=clickArea.GetOffsetByDisplayInfo((int)(e.X-clickArea.X), (int)(e.Y-clickArea.Y), out digit, out flags);
			//Console.WriteLine("Press in {0} area @ {1:x}.{2}", clickArea.Type,off, digit);
			
			if ((flags & Area.GetOffsetFlags.Abyss) == Area.GetOffsetFlags.Abyss)
				off++;
			
			if ((flags & Area.GetOffsetFlags.Eof) == Area.GetOffsetFlags.Eof)
				off=dataView.Buffer.Size;
			
				
			// update cursor in areas if keypress outside selection
			if (!clickArea.Selection.Contains(off)) {
				if (clickArea.CanFocus) {	
					foreach(Area a in dvDisplay.Layout.Areas) 
						a.HasCursorFocus=false;
				}
				
				dataView.MoveCursor(off, digit);
				dataView.SetSelection(-1,-1);
				
				// set the focused area
				if (clickArea.CanFocus) {
					clickArea.HasCursorFocus=true;
					clickArea.MoveCursor(off, digit);
				}
			}
			
			if ((flags & Area.GetOffsetFlags.Abyss) == Area.GetOffsetFlags.Abyss)
				selStartInAbyss=true;
			else
				selStartInAbyss=false;
				
			selStart=off;
			
		}											
		
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
		
		// if left mouse button is down
		if ((state & Gdk.ModifierType.Button1Mask) != 0) {
			// find in which area the pointer is
			Area clickArea=GetAreaByXY(x, y);
						
			if (clickArea!=null) {
				Area.GetOffsetFlags flags;
				int digit;
				long off=clickArea.GetOffsetByDisplayInfo((int)(x-clickArea.X), (int)(y-clickArea.Y), out digit, out flags);
				//Console.WriteLine("Motion in {0} area @ {1:x}", clickArea.Type,off);
				if ((flags & Area.GetOffsetFlags.Abyss) == Area.GetOffsetFlags.Abyss)
					off++;
				if ((flags & Area.GetOffsetFlags.Eof) == Area.GetOffsetFlags.Eof)
					off=dataView.Buffer.Size;
				if (off<0)
					off=0;
					
				// Evaluate selection before moving the cursor
				// for better visual result
				bool abyss=((flags & Area.GetOffsetFlags.Abyss) == Area.GetOffsetFlags.Abyss);
				
				EvaluateSelectionDrag(off, abyss);
				
				// update cursor in areas	
				if (clickArea.CanFocus) {	
					foreach(Area a in dvDisplay.Layout.Areas) 
						a.HasCursorFocus=false;
				}
				
				dataView.MoveCursor(off, digit);
				
				// set the focused area
				if (clickArea.CanFocus) {
					clickArea.HasCursorFocus=true;
					clickArea.MoveCursor(off, digit);
				}
				
				dvDisplay.MakeOffsetVisible(off, DataViewDisplay.ShowType.Closest);
			}											
				
		}// end: if mouse button down
		
	}
	
	///<summary>Handle mouse button release</summary>
	internal void OnButtonRelease (object o, ButtonReleaseEventArgs args)
	{
		Gdk.EventButton e=args.Event;
		Area clickArea=GetAreaByXY((int)e.X, (int)e.Y);
		
		
		if (clickArea!=null) {
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
				
			Area.GetOffsetFlags flags;
			int digit;
			long off=clickArea.GetOffsetByDisplayInfo((int)(x-clickArea.X), (int)(y-clickArea.Y), out digit, out flags);
			//Console.WriteLine("Release in {0} area @ {1:x}", clickArea.Type,off);
			if ((flags & Area.GetOffsetFlags.Abyss) == Area.GetOffsetFlags.Abyss)
				off++;
			if ((flags & Area.GetOffsetFlags.Eof) == Area.GetOffsetFlags.Eof)
				off=dataView.Buffer.Size;
			
			// Evaluate selection before moving the cursor
			// for better visual result
			bool abyss=((flags & Area.GetOffsetFlags.Abyss) == Area.GetOffsetFlags.Abyss);
				
			EvaluateSelectionDrag(off, abyss);
			
			// update cursor in areas	
			if (clickArea.CanFocus) {	
				foreach(Area a in dvDisplay.Layout.Areas) 
					a.HasCursorFocus=false;
			}
				
			dataView.MoveCursor(off, digit);
				
			// set the focused area
			if (clickArea.CanFocus) {
				clickArea.HasCursorFocus=true;
				clickArea.MoveCursor(off, digit);
			}
			
		}											
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
				return a;
			}
		}
		
		return null;
	}
	
	// variables that are shared by OnKeyPress and OnKey* handler methods
	Area okp_focusArea;
	int okp_bpr;
	int okp_dpb;
	long okp_cOffset;
	int okp_cDigit;
	int okp_pageIncrement;
	long okp_bbSize;
	long okp_selEnd;
	bool okp_specialKey;
	DataViewDisplay.ShowType okp_showType;
		
	///<summary>Handle key presses</summary>
	internal void OnKeyPress (object o, KeyPressEventArgs args)
	{
		Gdk.EventKey e=args.Event;
		okp_focusArea=null;
		
		// find focused area
		FindFocusedArea(out okp_focusArea);
		
		// if no area has the focus, give it to the first one applicable
		if (okp_focusArea == null)
			okp_focusArea=CycleFocus();
		
		
		// if still no area got the focus give up
		if (okp_focusArea == null)
			return;
		
		okp_bpr=okp_focusArea.BytesPerRow;
		okp_dpb=okp_focusArea.DigitsPerByte;
		okp_cOffset=okp_focusArea.CursorOffset;
		okp_cDigit=okp_focusArea.CursorDigit;
		okp_pageIncrement=(int)dvDisplay.VScroll.Adjustment.PageIncrement;
		okp_bbSize=dataView.Buffer.Size;
		okp_selEnd=-1;
		okp_showType=DataViewDisplay.ShowType.Closest;
		
		// 
		if ((e.State & Gdk.ModifierType.ShiftMask) == Gdk.ModifierType.ShiftMask) {
			if (shiftPressed==false) {
				selStart=okp_cOffset;
				shiftPressed=true;
			}
		}
		
		// handle keys 
		okp_specialKey=false;
		
		switch(e.Key) {
			case Gdk.Key.Up:
				OnKeyUp(); 
				break;
			case Gdk.Key.Down:
				OnKeyDown();
				break;
			case Gdk.Key.Left:
				OnKeyLeft();
				break;
			case Gdk.Key.Right:
				OnKeyRight();
				break;
			case Gdk.Key.Page_Up:
				OnKeyPageUp();
				break;
			case Gdk.Key.Page_Down:
				OnKeyPageDown();
				break;
			case Gdk.Key.Home:
				OnKeyHome();
				break;
			case Gdk.Key.End:
				OnKeyEnd();
				break;
			case Gdk.Key.Insert:
				OnKeyInsert();
				break;
			case Gdk.Key.Tab:
				OnKeyTab();
				break;
			case Gdk.Key.BackSpace:
				OnKeyBackspace();
				return; // OnKeyBackspace() handles drawing
			default:
				OnKeyDefault(e); 
				break;
		} // end switch()
			
		
			
		// if we have a new selection end
		if (okp_selEnd != -1) {
			long newSelEnd=okp_selEnd;
			long newSelStart=selStart;
				
			if (okp_selEnd!=selStart) {
				// find if the selection end is at a greater or
				// lesser position than the start and adjust
				// the selection accordingly
				if (okp_selEnd > selStart)
					newSelEnd--;
				else if (okp_selEnd < selStart)
					newSelStart--;
				
				// make sure selection range is sorted
				if (newSelStart > newSelEnd)
					dataView.SetSelection(newSelEnd, newSelStart);
				else
					dataView.SetSelection(newSelStart, newSelEnd);	
			}
			else {
				dataView.SetSelection(-1, -1);
			}		
		}
		else if (!okp_specialKey && !shiftPressed)
			dataView.SetSelection(-1, -1);

		// update cursor
		dvDisplay.MakeOffsetVisible(okp_cOffset, okp_showType);	
		dataView.MoveCursor(okp_cOffset, okp_cDigit);	
	}
	
	///<summary>Handle key releases</summary>
	internal void OnKeyRelease (object o, KeyReleaseEventArgs args)
	{
		Gdk.EventKey e=args.Event;
		if ( (e.Key == Gdk.Key.Shift_L || e.Key == Gdk.Key.Shift_R) )
			shiftPressed=false;
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
	
	void OnKeyUp()
	{
		okp_cOffset -= okp_bpr;
		
		if (okp_cOffset < 0) {
			okp_cOffset=okp_focusArea.CursorOffset;
			okp_cDigit=okp_focusArea.CursorDigit;
			if (shiftPressed)
				okp_selEnd=-1;
		}
		else if (shiftPressed) {
			okp_selEnd=okp_cOffset;
			okp_cDigit=0;
		}
	}
	
	void OnKeyDown()
	{
		okp_cOffset += okp_bpr;
		
		if (okp_cOffset > okp_bbSize) {
			okp_cOffset = okp_bbSize;
			okp_cDigit=okp_focusArea.CursorDigit;
			if (shiftPressed)
				okp_selEnd=okp_bbSize;
		}
		else if (shiftPressed) {	
			okp_selEnd=okp_cOffset;
			okp_cDigit=0;
		}
	}
	
	void OnKeyLeft()
	{
		if (!shiftPressed) {
			okp_cDigit--;
			if (okp_cDigit<0) {
				okp_cOffset--;
				okp_cDigit = okp_dpb - 1;
			}
			if (okp_cOffset < 0) {
				okp_cOffset = 0;
				okp_cDigit=0;
			}
		}
		else {
			okp_cOffset--;
			if (okp_cOffset < 0)
				okp_cOffset = 0;
			else {
				okp_selEnd=okp_cOffset;	
				okp_cDigit=0;
			}
		}	
	}
	
	void OnKeyRight()
	{
		if (!shiftPressed) {
			okp_cDigit++;
			if (okp_cDigit>=okp_dpb) {
				okp_cOffset++;
				okp_cDigit = 0;
			}
			if (okp_cOffset > okp_bbSize) {
				okp_cOffset = okp_bbSize;
				okp_cDigit = okp_dpb-1;
			}
		}
		else {	// if (shiftPressed)	
			okp_cOffset++;
			if (okp_cOffset > okp_bbSize)
				okp_cOffset = okp_bbSize;
			else {
				okp_selEnd=okp_cOffset;	
				okp_cDigit=0;
			}
		}
	}
	
	
	void OnKeyPageUp()
	{
		okp_cOffset -= okp_bpr*okp_pageIncrement;
		
		if (okp_cOffset < 0) {
			okp_cOffset=0;
			okp_cDigit=okp_focusArea.CursorDigit;
			if (shiftPressed) 
				okp_selEnd=0;
		}
		else if (shiftPressed) {
			okp_selEnd=okp_cOffset;
			okp_cDigit=0;
		}
		
		okp_showType=DataViewDisplay.ShowType.Cursor;
	}
	
	void OnKeyPageDown()
	{
		okp_cOffset += okp_bpr*okp_pageIncrement;
		
		if (okp_cOffset > okp_bbSize) {
			okp_cOffset=okp_bbSize;
			okp_cDigit=okp_focusArea.CursorDigit;
			if (shiftPressed) 
				okp_selEnd=okp_bbSize;
		}
		else if (shiftPressed) {
			okp_selEnd=okp_cOffset;
			okp_cDigit=0;
		}
		
		okp_showType=DataViewDisplay.ShowType.Cursor;
	}
	
	void OnKeyHome()
	{
		okp_cOffset = 0;
		okp_cDigit = 0;
		if (shiftPressed) 
			okp_selEnd = okp_cOffset;
			
		okp_showType=DataViewDisplay.ShowType.Start;
	}
	
	void OnKeyEnd()
	{
		okp_cOffset = okp_bbSize;
		okp_cDigit = 0;
		if (shiftPressed) 
			okp_selEnd = okp_cOffset;
		
		okp_showType=DataViewDisplay.ShowType.End;
	}
	
	void OnKeyInsert()
	{
		dataView.Overwrite = !dataView.Overwrite;
	}
	
	void OnKeyTab()
	{
		CycleFocus();
		okp_specialKey=true;
	}
	
	void OnKeyBackspace()
	{
		// move one position backwards and delete
		dataView.DeleteBackspace();
	}
	
	void OnKeyDefault(Gdk.EventKey e)
	{
		if (!dataView.Buffer.ModifyAllowed) {
			okp_specialKey=true;
			return;
		}
		
		// any other key pass it to focused area
		// if area handled it move one position right
		if (imContext.FilterKeypress(e) && okp_focusArea.HandleKey(e.Key, dataView.Overwrite) == true) {
			okp_cDigit++;
			// bytebuffer size may have changed (eg append)
			okp_bbSize=dataView.Buffer.Size;
			if (okp_cDigit>=okp_dpb) {
				okp_cOffset++;
				okp_cDigit = 0;
				dataView.CursorUndoDeque.AddFront(new CursorState(okp_cOffset-1, okp_dpb-1, okp_cOffset, okp_cDigit));
			}
			else 
				dataView.CursorUndoDeque.AddFront(new CursorState(okp_cOffset, okp_cDigit-1, okp_cOffset, okp_cDigit));
				
			dataView.CursorRedoDeque.Clear();	
			
			dataView.SetSelection(-1, -1);
		}
		else
			okp_specialKey=true;
	}
	
 } // end DataViewControl
 
 } //end namespace