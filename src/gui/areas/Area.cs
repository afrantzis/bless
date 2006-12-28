// created on 6/14/2004 at 10:39 PM
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
 
using Bless.Gui.Drawers;
using Bless.Util; 
using Bless.Buffers;
using Bless.Tools.Find;
  
namespace Bless.Gui.Areas {

public abstract class Area {
	
	protected Gtk.DrawingArea drawingArea;
	protected Drawer drawer;
	protected Drawer.Information drawerInformation;
	protected ByteBuffer byteBuffer;
	protected IFindStrategy findStrategy; 
	protected string type;
	
	// display
	protected int x;
	protected int y;
	protected int width;
	protected int height;
	protected long offset;
	protected int bpr;
	protected int dpb; // digits per byte 
	protected int fixedBpr;
	protected Gdk.Drawable backPixmap;
	protected bool manualDoubleBuffer;
	protected bool isAreaRealized;
	
	// GC's
	protected Gdk.GC cursorGC;
	protected Gdk.GC activeCursorGC;
	protected Gdk.GC inactiveCursorGC;
	
	protected Range selection;
	protected RangeCollection[] highlights;
	protected bool[] enableHighlights; 
	
	// cursor
	protected long cursorOffset;
	protected int  cursorDigit;
	protected bool cursorFocus;
	protected bool canFocus;

	// track changes
	protected long prevOffset; 
	protected long prevCursorOffset;
	protected int  prevCursorDigit;
	
	// Abstract methods
	//
	abstract protected void RenderRowNormal(int i, int p, int n, bool blank);
	abstract protected void RenderRowHighlight(int i, int p, int n, bool blank, Drawer.HighlightType ht);
	
	abstract public void GetDisplayInfoByOffset(long off, out int orow, out int obyte, out int ox, out int oy);
	
	public enum GetOffsetFlags { Eof=1, Abyss=2}
	
	abstract public long GetOffsetByDisplayInfo(int x, int y,out int digit, out GetOffsetFlags rflags);
	
	virtual public bool HandleKey(Gdk.Key key, bool overwrite)
	{
		return false;
	}
	

	///<summary>
	/// Calculates the width in pixels the    
	/// area will occupy with n bytes per row.
	///</summary>
	abstract public int CalcWidth(int n, bool force);
	
	static public Area Factory(string name)
	{
		Area a;
		
		switch (name) {
			case "hexadecimal":
				a=new HexArea();
				break;
			case "binary":
				a=new BinaryArea();
				break;
			case "decimal":
				a=new DecimalArea();
				break;
			case "octal":
				a=new OctalArea();
				break;
			case "ascii":
				a=new AsciiArea();
				break;
			case "separator":
				a=new SeparatorArea();
				break;
			case "offset":
				a=new OffsetArea();
				break;				
			default:
				a=null;
				break;
		}	
		
		return a;
	
	}
	
	///<summary>
	/// Create an area.
	///</summary>
	public Area() 
	{	
		highlights=new RangeCollection[(int)Drawer.HighlightType.Sentinel];
		for(int i=0; i < (int)Drawer.HighlightType.Sentinel; i++)
			highlights[i]=new RangeCollection();
		
		enableHighlights=new bool[(int)Drawer.HighlightType.Sentinel];
		for(int i=0; i < (int)Drawer.HighlightType.Sentinel; i++)
			enableHighlights[i]=true;
		
		findStrategy=new BMFindStrategy();
		
		canFocus=false;
		dpb=1024; // arbitrary large int
		fixedBpr=-1;
		isAreaRealized=false;
	}
	
	///<summary>
	/// Realize the area.
	///</summary>
	public virtual void Realize(Gtk.DrawingArea da, Drawer d)
	{
		drawingArea=da;
		
		backPixmap=da.GdkWindow;
		
		activeCursorGC=new Gdk.GC(da.GdkWindow);
		inactiveCursorGC=new Gdk.GC(da.GdkWindow);
		
		drawer=d; 
		Gdk.Color col=new Gdk.Color();
		
		
		Gdk.Color.Parse("red", ref col); 
		activeCursorGC.RgbFgColor=col;
		Gdk.Color.Parse("gray", ref col);
		inactiveCursorGC.RgbFgColor=col;
		cursorGC=activeCursorGC;
		
		isAreaRealized=true;
	}
	
	// Properties
	//
	public int X {
		set { x=value; }
		get	{ return x; }
	} 

	public int Y {
		set { y=value; }
		get	{ return y; }
	}
	
	public int Width {
		set { width=value; }
		get	{ return width; }
	} 

	public int Height {
		set { height=value; }
		get	{ return height; }
	}
	
	public int BytesPerRow {
		set { bpr=value; }
		get	{ return bpr; }
	}
	
	public int FixedBytesPerRow {	
		set { fixedBpr=value; }
		get { return fixedBpr; }
	}
	
	public int DigitsPerByte {
		get	{ return dpb; }
	}
	
	public long Offset {
		set { prevOffset=offset; offset=value; } 
		get { return offset;}
	}

	public long CursorOffset {
		set { prevCursorOffset=cursorOffset; cursorOffset=value;} 
		get { return cursorOffset;}
	}
	
	public long PrevCursorOffset { 
		get { return prevCursorOffset;}
	}
	
	public int CursorDigit {
		set { prevCursorDigit=cursorDigit; if (value >= dpb) cursorDigit=dpb-1; else cursorDigit=value;} 
		get { return cursorDigit;}
	}
	
	public bool HasCursorFocus {
		set { cursorFocus=value; } 
		get { return cursorFocus;}
	}
	
	public bool CanFocus { 
		get { return canFocus;}
	}
	
	public Drawer.Information DrawerInformation
	{
		get { return drawerInformation; }
		set { drawerInformation=value; }
	}
	
	public virtual Drawer Drawer {
		get { return drawer; }
	}

	public string Type {
		get { return type; }
	}
	
	public Gtk.DrawingArea DrawingArea {
		get { return drawingArea; }
	}
	
	public ByteBuffer Buffer {
		get { return byteBuffer; }
		set { byteBuffer=value; }
	}
	
	public Range Selection {
		get { return highlights[(int)Drawer.HighlightType.Selection].LastAdded; }
		set { Selection.Start=value.Start; Selection.End=value.End; }
	} 
	
	public bool IsActive {
		set { 
			if (value==true)
				cursorGC=activeCursorGC;
			else
				cursorGC=inactiveCursorGC;
			
			// doesn't actually change cursor position
			// just redraws it with correct color
			MoveCursor(cursorOffset, cursorDigit);
		}
		
		get { return cursorGC==activeCursorGC; } 
	}
	
	public bool[] EnableHighlights {
		get { return enableHighlights; } 
	}
	
	///<summary>
	/// Render the 'newHi' taking
	/// into account differences from the 'prevHi'
	///</summary>	
	protected void RenderHighlightDiffs(Range newHi, Range prevHi, Range clip, Drawer.HighlightType ht)
	{
		if (isAreaRealized==false)
			return;
		
		// find what parts of the selection have changed
		Range added=new Range(newHi);
		added.Sort();
		
		// find unchanged range		
		Range common=new Range(prevHi);
		common.Sort();
		common.Intersect(added);
		common.Intersect(clip);
			
		// find lost (now unselected) ranges
		Range lost1=new Range(prevHi);
		Range lost2=new Range();
		lost1.Sort();
		lost1.Difference(added, lost2);
		lost1.Intersect(clip);
		lost2.Intersect(clip);
			
		// find added ranges
		Range old=new Range(prevHi);
		Range added1=new Range();
		old.Sort();
		added.Difference(old, added1);
		added.Intersect(clip);
		
		//System.Console.WriteLine("RenderSelectionDiffs: Clip=({0},{1}) Added=({2},{3}) Lost1=({4},{5}) Lost2=({6},{7}) Common=({8},{9}) PrevSel=({10},{11}) CurSel=({12},{13})",
		//						clip.Start, clip.End, added.Start, added.End, lost1.Start, lost1.End, lost2.Start, lost2.End, common.Start, common.End, prevHi.Start, prevHi.End, newHi.Start, newHi.End );
		//System.Console.WriteLine("Common: {0} {1}",common.Start, common.End);
		//System.Console.WriteLine("Lost: {0} {1}",lost.Start, lost.End);
		//System.Console.WriteLine("Added: {0} {1}",added.Start, added.End);
			
		// render the lost parts normally
		int rLost1Start=-1;
		int rLost1End=-1;
		int rLost2Start=-1;
		int rLost2End=-1;
		
		if (!lost1.IsEmpty()) {
			rLost1Start=((int)(lost1.Start-offset)/bpr);
			rLost1End=((int)(lost1.End-offset)/bpr);
			//render the lines fully
			for (int j=rLost1Start; j<rLost1End; j++)
				RenderRowNormal(j, 0, bpr, true);
			// make sure we are ot out of range at the last line
			long len = byteBuffer.Size-(lost1.End/bpr)*bpr;
            if (len > bpr) len=bpr;
			RenderRowNormal(rLost1End, 0, (int)len, true);
		}
	
		if (!lost2.IsEmpty()) {
			rLost2Start=((int)(lost2.Start-offset)/bpr);
			rLost2End=((int)(lost2.End-offset)/bpr);
			//render the middle lines fully
			for (int j=rLost2Start; j<rLost2End; j++)
				RenderRowNormal(j, 0, bpr, true);
			// make sure we are ot out of range at the last line
			long len = byteBuffer.Size-(lost2.End/bpr)*bpr;
            if (len > bpr) len=bpr;
			RenderRowNormal(rLost2End, 0, (int)len, true);
        }
		
		
		// re-render the common parts that overlap the lost
		// parts as selections
		if (!common.IsEmpty()) {
			int rCommonStart=((int)(common.Start-offset)/bpr);
			int rCommonEnd=((int)(common.End-offset)/bpr);
			if (rCommonStart == rLost1End && rCommonEnd == rLost1End
				&& rLost1End == rLost1Start) {
				RenderHighlight(common, ht);
			}
			else if (rCommonStart == rLost2End && rCommonEnd == rLost2End
				&& rLost2End == rLost2Start) {
				RenderHighlight(common, ht);
			}
			else if (rCommonStart == rLost1End || rCommonStart == rLost2End) {
				int nbyte=(int)((common.Start-offset)%bpr);
				long overLeft = common.End-common.Start < bpr-nbyte ? common.End-common.Start:bpr-nbyte;
				Range rOverlap=new Range(common.Start, common.Start+overLeft);
				RenderHighlight(rOverlap, ht);
			} 
			else if (rCommonEnd == rLost1Start || rCommonEnd == rLost2Start) {
				int nbyte=(int)((common.End-offset)%bpr);
				long rCommonEndOffset=rCommonEnd*bpr+offset;
				long overStart=rCommonEndOffset > common.Start? rCommonEndOffset:common.Start;
				// part of common that is on row rCommonEnd
				Range rOverlap=new Range(overStart, rCommonEndOffset+nbyte);
				RenderHighlight(rOverlap, ht);
			}
		}
		
		
		// render the added parts as selections
		if (!added.IsEmpty()) {
			if ((added.Start==common.End+1) && added.Start>0)
				--added.Start;
			if (added.End==common.Start-1)
				++added.End;
			RenderHighlight(added, ht);
		}
		
		if (!added1.IsEmpty()) {
			if ((added1.Start==common.End+1) && added1.Start>0)
				--added1.Start;
			if (added1.End==common.Start-1)
				++added1.End;
			RenderHighlight(added1, ht);
		}
		
	}
	
	void RenderRangeHelper(Drawer.HighlightType ht, int rstart, int bstart, int len) 
	{
		if (ht!=Drawer.HighlightType.Normal)
			RenderRowHighlight(rstart, bstart, len, false, ht);
		else
			RenderRowNormal(rstart, bstart, len, false);
	}
	
	///<summary>
	/// Render the bytes in 'range' 
	/// using the specified HighlightType 
	///</summary>
	protected virtual void RenderRange(Range range, Drawer.HighlightType ht)
	{
		if (isAreaRealized==false)
			return;
		
		int rstart, bstart, xstart, ystart;
		int rend, bend, xend, yend;
		bool odd;
		Gdk.GC gc;
		Gdk.GC oddGC;
		Gdk.GC evenGC;
		
	
		oddGC=drawer.GetBackgroundGC(Drawer.RowType.Odd, ht);
		evenGC=drawer.GetBackgroundGC(Drawer.RowType.Even, ht);
			
		GetDisplayInfoByOffset(range.Start,out rstart, out bstart, out xstart, out ystart);
		GetDisplayInfoByOffset(range.End,out rend, out bend, out xend, out yend);

		//System.Console.WriteLine("Start {0:x} {1} {2} x:{3} y:{4}", range.Start, rstart, bstart, xstart, ystart);
		//System.Console.WriteLine("End {0:x} {1} {2} x:{3} y:{4}", range.End, rend, bend, xend, yend);
		
		// if the whole range is on one row 
		if (rstart==rend) {
			if (manualDoubleBuffer)			
				BeginPaint(x+xstart, y+ystart, xend-xstart+dpb*drawer.Width, drawer.Height);
			
			// odd row?
			odd=(((range.Start/bpr)%2)==1);
			if (odd)
				gc=oddGC;
			else
				gc=evenGC;
				
			//render
			backPixmap.DrawRectangle(gc, true, x+xstart, y+ystart, xend-xstart, drawer.Height);
			
			RenderRangeHelper(ht, rstart, bstart, bend-bstart+1);
		}
		else { // multi-row range
			
			if (manualDoubleBuffer) {
				// handle double-buffering
				Gdk.Region paintRegion=new Gdk.Region();
			
				Gdk.Rectangle rectStart=new Gdk.Rectangle(x+xstart, y+ystart, width-xstart, drawer.Height);
			
				Gdk.Rectangle rectMiddle;
				if (rend > rstart + 1)
					rectMiddle=new Gdk.Rectangle(x, y+ystart+drawer.Height, width, yend-ystart-drawer.Height);
				else
					rectMiddle=Gdk.Rectangle.Zero;
			
				Gdk.Rectangle rectEnd=new Gdk.Rectangle(x, y+yend, xend+dpb*drawer.Width, drawer.Height);
		
				paintRegion.UnionWithRect(rectStart);
				paintRegion.UnionWithRect(rectMiddle);
				paintRegion.UnionWithRect(rectEnd);
			
				BeginPaintRegion(paintRegion);
			}
			
			// render first row
			odd=(((range.Start/bpr)%2)==1);
			if (odd)
				gc=oddGC;
			else	
				gc=evenGC;
			backPixmap.DrawRectangle(gc, true, x+xstart, y+ystart, width-xstart, drawer.Height);
			
			RenderRangeHelper(ht, rstart, bstart, bpr-bstart);
			
			long curOffset=range.Start+bpr-bstart;
			
			// render middle rows
			for(int i=rstart+1;i<rend;i++) {
				odd=(((curOffset/bpr)%2)==1);
				if (odd)
					gc=oddGC;
				else	
					gc=evenGC;
				backPixmap.DrawRectangle(gc, true, x, y+i*drawer.Height, width, drawer.Height);
				RenderRangeHelper(ht, i, 0, bpr);
				curOffset+=bpr;
			}
			
			// render last row
			odd=(((range.End/bpr)%2)==1);
			if (odd)
				gc=oddGC;
			else	
				gc=evenGC;
			backPixmap.DrawRectangle(gc, true, x, y+yend, xend, drawer.Height);
			RenderRangeHelper(ht, rend, 0, bend+1);
		}
		
		if (manualDoubleBuffer)
			EndPaint();
		
	}
	
	///<summary>Render a range of bytes as selected</summary>
	protected void RenderHighlight(Range sel, Drawer.HighlightType ht)
	{
		RenderRange(sel, ht);
	}
	
	///<summary>Render a range of bytes normally</summary>
	protected void RenderNormally(Range r)
	{
		RenderRange(r, Drawer.HighlightType.Normal);
	}
	
	///<summary>Render the cursor</summary>
	protected void RenderCursor()
	{
		if (isAreaRealized==false)
			return;
		
		Range sel=highlights[(int)Drawer.HighlightType.Selection].LastAdded;
		
		// don't draw the cursor when there is a selection
		if (!sel.IsEmpty())
			return;
		
		int cRow, cByte, cX, cY;
		GetDisplayInfoByOffset(cursorOffset, out cRow, out cByte, out cX, out cY);
		
		backPixmap.DrawRectangle(cursorGC, true, x+cX, y+cY+drawer.Height-2, drawer.Width*dpb, 2);
		if (cursorFocus) {
			backPixmap.DrawRectangle(cursorGC, true, x+cX+cursorDigit*drawer.Width, y+cY, 1, drawer.Height-2);
		}
	}
	
	public void DisposePixmaps()
	{
		if (isAreaRealized==false)
			return;
			
		backPixmap.Dispose();
		drawer.DisposePixmaps();
	}
	
	void BeginPaintRegion(Gdk.Region r)
	{
		Gdk.Window win=drawingArea.GdkWindow;
		
		win.BeginPaintRegion(r);
	}
	
	void BeginPaint()
	{
		BeginPaint(this.x, this.y, this.width, this.height);
	}
	
	void BeginPaint(int x, int y, int w, int h)
	{
		Gdk.Window win=drawingArea.GdkWindow;
		
		win.BeginPaintRect(new Gdk.Rectangle(x, y, w, h));
	}
	
	void EndPaint()
	{
		drawingArea.GdkWindow.EndPaint();
	}
	
	///<summary>
	/// Find the type of highlight an offset has. 
	///</summary>
	Drawer.HighlightType GetOffsetHighlight(long offs)
	{
		for(int i=1; i<(int)Drawer.HighlightType.Sentinel; i++) {
			foreach(Range range in highlights[i]) {
				if (range.Contains(offs))
					return (Drawer.HighlightType)i;
			} 
		}
		
		return Drawer.HighlightType.Normal;
	}
	
	///
	/// Public Interface
	///
	
	///<summary>Draws the area</summary>
	public virtual void Render() 
	{		
		// if we don't have enough space to draw return 
		if (bpr<=0)
			return;

		Scroll(offset);
	}
	
	///<summary>Moves and draws the cursor</summary>
	public virtual void MoveCursor(long offset, int digit)
	{
		RenderOffset(cursorOffset);
		this.CursorOffset=offset;
		this.CursorDigit=digit;
		RenderCursor();
	}
	
	///<summary>Moves but does not draw the cursor</summary>
	public virtual void MoveCursorNoRender(long offset, int digit)
	{
		this.CursorOffset=offset;
		this.CursorDigit=digit;
	}
	
	///<summary>Renders a single offset</summary>
	public virtual void RenderOffset(long offs)
	{
		if (isAreaRealized==false)
			return;
		
		int nrows=height/drawer.Height;
		long bleft=nrows*bpr;
        
		if (bleft+offset >= byteBuffer.Size)
			bleft=byteBuffer.Size - offset;
			
		if (offs >= offset && offs < offset+bleft) {	 
			int pcRow, pcByte, pcX, pcY;
			GetDisplayInfoByOffset(offs, out pcRow, out pcByte, out pcX, out pcY);
			Drawer.HighlightType ht=GetOffsetHighlight(offs);
			if (ht!=Drawer.HighlightType.Normal && enableHighlights[(int)ht])
				RenderRowHighlight(pcRow, pcByte, 1, false, ht);
			else
				RenderRowNormal(pcRow, pcByte, 1, false);
		}
		else if (offs == byteBuffer.Size && offs == offset+bleft) {
			int pcRow, pcByte, pcX, pcY;
			GetDisplayInfoByOffset(offs, out pcRow, out pcByte, out pcX, out pcY);
			Gdk.GC backEvenGC=drawer.GetBackgroundGC(Drawer.RowType.Even, Drawer.HighlightType.Normal);
			backPixmap.DrawRectangle(backEvenGC, true, x+pcX, y+pcY, drawer.Width*dpb, drawer.Height);
		}

	}
	
	
	public virtual void RenderBackground(Drawer.RowType rtype, Drawer.ColumnType ctype, int x, int y, int w, int h)
	{
	
	
	}
	
	///<summary>Scrolls the view so that 'offset' is the first visible offset</summary>
	public virtual void Scroll(long offset)
	{	
		this.prevOffset=this.offset;
		this.offset=offset;
		
		if (isAreaRealized==false)
			return;
		
		// find out number of rows, bytes etc
		int nrows=height/drawer.Height;
		long bleft=nrows*bpr;
    
		if (bleft+offset >= byteBuffer.Size)
			bleft=byteBuffer.Size - offset;
                                                                                
		// blank the background
		Gdk.GC backEvenGC=drawer.GetBackgroundGC(Drawer.RowType.Even, Drawer.HighlightType.Normal);
		backPixmap.DrawRectangle(backEvenGC, true, x,y, width, height);

		Range rSel=new Range(highlights[(int)Drawer.HighlightType.Selection].LastAdded);
		rSel.Sort();
		
		Range rClip;
		// make sure we get an empty clipping Range when bleft==0 
		if (bleft>0)
			rClip=new Range(offset, offset+bleft-1);
		else
			rClip=new Range();

		// calculate the ranges to render normally (not selected)
		Range rNormal1=new Range(rClip);
		Range rNormal2=new Range();
		rNormal1.Difference(rSel, rNormal2);
		
		if (!rNormal1.IsEmpty())
			RenderNormally(rNormal1);
		if (!rNormal2.IsEmpty())
			RenderNormally(rNormal2);
				
		// render selection
		rSel.Intersect(rClip);

		if (!rSel.IsEmpty())
			if (enableHighlights[(int)Drawer.HighlightType.Selection])
				RenderHighlight(rSel, Drawer.HighlightType.Selection);
			else
				RenderNormally(rSel);
				
		// render secondary highlights, if they are enabled
		ClearHighlightsNoRender(Drawer.HighlightType.PatternMatch);
		byte[] ba=byteBuffer.RangeToByteArray(Selection);
		if (ba!=null && enableHighlights[(int)Drawer.HighlightType.PatternMatch])
			AddHighlightPattern(ba, Drawer.HighlightType.PatternMatch);
		
		// render the cursor	
		if ((cursorOffset >= offset && cursorOffset < offset+bleft)
			|| (cursorOffset == byteBuffer.Size && cursorOffset == offset+bleft))
			RenderCursor();
		
	}
	
	///<summary>Set and draw the selection</summary>
	public virtual void SetSelection(long start, long end)
	{
		UpdateHighlight(start, end, Drawer.HighlightType.Selection);
	} 
	
	///<summary>Set but don't draw the selection</summary>
	public virtual void SetSelectionNoRender(long start, long end)
	{
		UpdateHighlightNoRender(start, end, Drawer.HighlightType.Selection);
	}
	
	///<summary>Add and display a highlighted range of a certain type</summary>
	public virtual void AddHighlight(long start, long end, Drawer.HighlightType ht)
	{
		RangeCollection rc=highlights[(int)ht];
		
		manualDoubleBuffer=true;
		
		rc.Add(new Range(start, end));
		
		int nrows=height/drawer.Height;
		long bleft=nrows*bpr;
		
		if (bleft+offset >= byteBuffer.Size)
			bleft=byteBuffer.Size - offset;
		
		Range clip;
						
		if (bleft>0)
			clip=new Range(offset, offset+bleft-1);
		else
			clip=new Range();
		
		Range newHighlight=new Range(rc.LastAdded);
		newHighlight.Intersect(clip);
		
		// if highlights are enabled, render them
		if (enableHighlights[(int)ht])
			RenderHighlight(newHighlight, ht);
		
		manualDoubleBuffer=false;
	}
	
	///<summary>Add but don't display a highlighted range of a certain type</summary>
	public virtual void AddHighlightNoRender(long start, long end, Drawer.HighlightType ht)
	{
		RangeCollection rc=highlights[(int)ht];
		
		rc.Add(new Range(start, end));
	}
	
	///<summary>Update and display the last added highlighted range of a certain type</summary>
	public virtual void UpdateHighlight(long start, long end, Drawer.HighlightType ht)
	{
		RangeCollection rc=highlights[(int)ht];
		
		if (start==rc.LastAdded.Start && end==rc.LastAdded.End)
			return;
			
		manualDoubleBuffer=true;
		
		Range prevHighlight=new Range(rc.LastAdded);
		
		rc.LastAdded.Start=start;
		rc.LastAdded.End=end;
				
		int nrows=height/drawer.Height;
		long bleft=nrows*bpr;
		
		if (bleft+offset >= byteBuffer.Size)
			bleft=byteBuffer.Size - offset;
		
		Range clip;
						
		if (bleft>0)
			clip=new Range(offset, offset+bleft-1);
		else
			clip=new Range();
		
		// if highlights are enabled, render them
		if (enableHighlights[(int)ht])
			RenderHighlightDiffs(rc.LastAdded, prevHighlight, clip, ht);
		
		manualDoubleBuffer=false;
	}
	
	///<summary>Update but don't display the last added highlighted range of a certain type</summary>
	public virtual void UpdateHighlightNoRender(long start, long end, Drawer.HighlightType ht)
	{
		RangeCollection rc=highlights[(int)ht];
		
		rc.LastAdded.Start=start;
		rc.LastAdded.End=end;
	}
	
	///<summary>Clear and display all highlighted ranges of a certain type</summary>
	public virtual void ClearHighlights(Drawer.HighlightType ht)
	{
		RangeCollection rc=highlights[(int)ht];
		manualDoubleBuffer=true;
		
		// clipping range is whole buffer
		Range rClip=new Range();
		
		if (byteBuffer.Size>0) {
			rClip.Start=0;
			rClip.End=byteBuffer.Size-1;
		}
			
		foreach(Range range in rc) {
			// make sure range is still valid...
			range.Intersect(rClip);
			if (!range.IsEmpty()) {
				//System.Console.WriteLine("Clearing Range: {0}-{1}", range.Start, range.End);
				RenderNormally(range);
			}
		}
		
		manualDoubleBuffer=false;
		
		rc.Clear();
	}
	
	///<summary>Clear but don't display all the highlighted ranges of a certain type</summary>
	public virtual void ClearHighlightsNoRender(Drawer.HighlightType ht)
	{
		RangeCollection rc=highlights[(int)ht];
		rc.Clear();
	}
	
	///<summary>
	/// Highlight all the ranges that match the specified pattern and are visible in the DataView.
	///</summary>
	public virtual void AddHighlightPattern(byte[] pattern, Drawer.HighlightType ht)
	{
		int patLen=pattern.Length;
		
		// set low limit
		long lowLimit=offset-patLen+1;
		if (lowLimit<0)
			lowLimit=offset;
		
		// set high limit
		int nrows=height/drawer.Height;
		long bleft=nrows*bpr;
		long highLimit;
		
		if (bleft+offset >= byteBuffer.Size)
			bleft=byteBuffer.Size - offset;
								
		if (bleft>0)
			highLimit=offset+bleft-1;
		else
			highLimit=-1;
		
		Range rClip=new Range(offset, highLimit);
		
		if (highLimit+patLen-1 < byteBuffer.Size)
			highLimit+=patLen-1;
		else
			highLimit=byteBuffer.Size-1;
		
		findStrategy.Buffer=byteBuffer;
		findStrategy.Position=lowLimit;
		findStrategy.Pattern=pattern;
		
		Range match;
		Range inter1=new Range();
		
		
		while((match=findStrategy.FindNext(highLimit))!=null) {
			// highlight areas that don't overlap with the selection
			match.Difference(Selection, inter1);
			match.Intersect(rClip);
			Range prevHighlight=new Range(highlights[(int)ht].LastAdded);
			prevHighlight.Intersect(rClip);
			if (!match.IsEmpty()) {
				// if new highlight is a continuation of the previous one
				// don't add the highlight, just update the old one
				// NOTE: not entirely correct, because the individual continuous
				// matches are not saved, only the final one, but things
				// get a lot faster
				if (prevHighlight.Contains(match.Start)) {
					UpdateHighlight(prevHighlight.Start, match.End, ht);
				}
				else
					AddHighlight(match.Start, match.End, ht);
				//System.Console.WriteLine("Adding Highlight range: {0}-{1}, Selection {2}-{3}", match.Start, match.End, Selection.Start, Selection.End);
			}
		}
		
	}
	
}// Area


} //namespace
