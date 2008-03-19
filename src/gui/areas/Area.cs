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
using System.Collections.Generic;
using System.Xml;

namespace Bless.Gui.Areas {

public abstract class Area
{
	protected AreaGroup areaGroup;
	protected Drawer drawer;
	protected Drawer.Information drawerInformation;
	protected string type;

	// display
	protected int x;
	protected int y;
	protected int width;
	protected int height;
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

	
	protected bool cursorFocus;
	protected bool canFocus;
	
	// Abstract methods
	//
	abstract protected void RenderRowNormal(int i, int p, int n, bool blank);
	abstract protected void RenderRowHighlight(int i, int p, int n, bool blank, Drawer.HighlightType ht);

	abstract public void GetDisplayInfoByOffset(long off, out int orow, out int obyte, out int ox, out int oy);

	public enum GetOffsetFlags { Eof = 1, Abyss = 2}

	abstract public long GetOffsetByDisplayInfo(int x, int y, out int digit, out GetOffsetFlags rflags);

	virtual public bool HandleKey(Gdk.Key key, bool overwrite)
	{
		return false;
	}


	///<summary>
	/// Calculates the width in pixels the
	/// area will occupy with n bytes per row.
	///</summary>
	abstract public int CalcWidth(int n, bool force);

	public delegate Area AreaCreatorFunc(AreaGroup ag);

	static private Dictionary<string, AreaCreatorFunc> pluginTable;

	static public void AddFactoryItem(string name, AreaCreatorFunc createArea)
	{
		if (pluginTable == null) {
			pluginTable = new Dictionary<string, AreaCreatorFunc>();
		}
		// System.Console.WriteLine("Adding plugin name {0}", name);
		pluginTable.Add(name, createArea);
	}

	static public Area Factory(string name, AreaGroup ag)
	{
		try {
			AreaCreatorFunc acf = pluginTable[name];
			return acf(ag);
		}
		catch (KeyNotFoundException e) {
			System.Console.WriteLine(e.Message);
		}

		return null;
	}

	///<summary>
	/// Create an area.
	///</summary>
	public Area(AreaGroup areaGroup)
	{
		this.areaGroup = areaGroup;
		
		drawerInformation = new Drawer.Information();

		canFocus = false;
		dpb = 1024; // arbitrary large int
		fixedBpr = -1;
		isAreaRealized = false;
	}

	public virtual void Configure(XmlNode parentNode)
	{
		XmlNodeList childNodes = parentNode.ChildNodes;
		foreach(XmlNode node in childNodes) {
			if (node.Name == "bpr")
				this.FixedBytesPerRow = System.Convert.ToInt32(node.InnerText);
			if (node.Name == "display")
				ParseDisplay(node, drawerInformation);
		}
	}

	///<summary>Parse the <display> tag in layout files</summary>
	void ParseDisplay(XmlNode parentNode, Drawer.Information info)
	{
		XmlNodeList childNodes = parentNode.ChildNodes;
		foreach(XmlNode node in childNodes) {
			if (node.Name == "evenrow")
				ParseDisplayRow(node, info, Drawer.RowType.Even);
			else if (node.Name == "oddrow")
				ParseDisplayRow(node, info, Drawer.RowType.Odd);
			else if (node.Name == "font")
				info.FontName = node.InnerText;
		}
	}

	void ParseDisplayRow(XmlNode parentNode, Drawer.Information info, Drawer.RowType rowType)
	{
		Gdk.Color fg, bg;
		XmlNodeList childNodes = parentNode.ChildNodes;
		foreach(XmlNode node in childNodes) {
			ParseDisplayType(node, out fg, out bg);

			if (node.Name == "evencolumn") {
				if (!bg.Equal(Gdk.Color.Zero))
					info.bgNormal[(int)rowType, (int)Drawer.ColumnType.Even] = bg;
				if (!fg.Equal(Gdk.Color.Zero))
					info.fgNormal[(int)rowType, (int)Drawer.ColumnType.Even] = fg;
			}
			else if (node.Name == "oddcolumn") {
				if (!bg.Equal(Gdk.Color.Zero))
					info.bgNormal[(int)rowType, (int)Drawer.ColumnType.Odd] = bg;
				if (!fg.Equal(Gdk.Color.Zero))
					info.fgNormal[(int)rowType, (int)Drawer.ColumnType.Odd] = fg;
			}
			else if (node.Name == "selectedcolumn") {
				if (!bg.Equal(Gdk.Color.Zero))
					info.bgHighlight[(int)rowType, (int)Drawer.HighlightType.Selection] = bg;
				if (!fg.Equal(Gdk.Color.Zero))
					info.fgHighlight[(int)rowType, (int)Drawer.HighlightType.Selection] = fg;
			}
			else if (node.Name == "patternmatchcolumn") {
				if (!bg.Equal(Gdk.Color.Zero))
					info.bgHighlight[(int)rowType, (int)Drawer.HighlightType.PatternMatch] = bg;
				if (!fg.Equal(Gdk.Color.Zero))
					info.fgHighlight[(int)rowType, (int)Drawer.HighlightType.PatternMatch] = fg;
			}
		}
	}

	///<summary>Parse a font type</summary>
	void ParseDisplayType(XmlNode parentNode, out Gdk.Color fg, out Gdk.Color bg)
	{
		fg = Gdk.Color.Zero;
		bg = Gdk.Color.Zero;
		XmlNodeList childNodes = parentNode.ChildNodes;
		foreach(XmlNode node in childNodes) {
			if (node.Name == "foreground")
				Gdk.Color.Parse(node.InnerText, ref fg);
			if (node.Name == "background")
				Gdk.Color.Parse(node.InnerText, ref bg);
		}
	}

	///<summary>
	/// Realize the area.
	///</summary>
	public virtual void Realize()
	{
		Gtk.DrawingArea da = areaGroup.DrawingArea;

		backPixmap = da.GdkWindow;

		activeCursorGC = new Gdk.GC(da.GdkWindow);
		inactiveCursorGC = new Gdk.GC(da.GdkWindow);

		Gdk.Color col = new Gdk.Color();


		Gdk.Color.Parse("red", ref col);
		activeCursorGC.RgbFgColor = col;
		Gdk.Color.Parse("gray", ref col);
		inactiveCursorGC.RgbFgColor = col;
		cursorGC = activeCursorGC;

		isAreaRealized = true;
	}

	// Properties
	//
	public int X {
		set { x = value; }
		get	{ return x; }
	}

	public int Y {
		set { y = value; }
		get	{ return y; }
	}

	public int Width {
		set { width = value; }
		get	{ return width; }
	}

	public int Height {
		set { height = value; }
		get	{ return height; }
	}

	public int BytesPerRow {
		set { bpr = value; }
		get	{ return bpr; }
	}

	public int FixedBytesPerRow {
		set { fixedBpr = value; }
		get { return fixedBpr; }
	}

	public int DigitsPerByte {
		get	{ return dpb; }
	}

	public bool HasCursorFocus {
		set { cursorFocus = value; }
		get { return cursorFocus;}
	}

	public bool CanFocus {
		get { return canFocus;}
	}

	public Drawer.Information DrawerInformation
	{
		get { return drawerInformation; }
		set { drawerInformation = value; }
	}

	public virtual Drawer Drawer {
		get { return drawer; }
	}

	public string Type {
		get { return type; }
	}

	public bool IsActive {
		set {
			if (value == true)
				cursorGC = activeCursorGC;
			else
				cursorGC = inactiveCursorGC;

			// doesn't actually change cursor position
			// just redraws it with correct color
			//MoveCursor(cursorOffset, cursorDigit);
		}

		get { return cursorGC == activeCursorGC; }
	}

	/*
	///<summary>
	/// Render the 'newHi' taking
	/// into account differences from the 'prevHi'
	///</summary>
	protected void RenderHighlightDiffs(Range newHi, Range prevHi, Range clip, Drawer.HighlightType ht)
	{
		if (isAreaRealized == false)
			return;

		// find what parts of the selection have changed
		Range added = new Range(newHi);

		// find unchanged range
		Range common = new Range(prevHi);
		common.Intersect(added);
		common.Intersect(clip);

		// find lost (now unselected) ranges
		Range lost1 = new Range(prevHi);
		Range lost2 = new Range();
		lost1.Difference(added, lost2);
		lost1.Intersect(clip);
		lost2.Intersect(clip);

		// find added ranges
		Range old = new Range(prevHi);
		Range added1 = new Range();
		added.Difference(old, added1);
		added.Intersect(clip);

		//System.Console.WriteLine("RenderSelectionDiffs: Clip=({0},{1}) Added=({2},{3}) Lost1=({4},{5}) Lost2=({6},{7}) Common=({8},{9}) PrevSel=({10},{11}) CurSel=({12},{13})",
		//						clip.Start, clip.End, added.Start, added.End, lost1.Start, lost1.End, lost2.Start, lost2.End, common.Start, common.End, prevHi.Start, prevHi.End, newHi.Start, newHi.End );
		//System.Console.WriteLine("Common: {0} {1}",common.Start, common.End);
		//System.Console.WriteLine("Lost: {0} {1}",lost.Start, lost.End);
		//System.Console.WriteLine("Added: {0} {1}",added.Start, added.End);

		// render the lost parts normally
		int rLost1Start = -1;
		int rLost1End = -1;
		int rLost2Start = -1;
		int rLost2End = -1;

		if (!lost1.IsEmpty()) {
			rLost1Start = ((int)(lost1.Start - offset) / bpr);
			rLost1End = ((int)(lost1.End - offset) / bpr);
			//render the lines fully
			for (int j = rLost1Start; j < rLost1End; j++)
				RenderRowNormal(j, 0, bpr, true);
			// make sure we are ot out of range at the last line
			long len = areaGroup.Buffer.Size - (lost1.End / bpr) * bpr;
			if (len > bpr) len = bpr;
			RenderRowNormal(rLost1End, 0, (int)len, true);
		}

		if (!lost2.IsEmpty()) {
			rLost2Start = ((int)(lost2.Start - offset) / bpr);
			rLost2End = ((int)(lost2.End - offset) / bpr);
			//render the middle lines fully
			for (int j = rLost2Start; j < rLost2End; j++)
				RenderRowNormal(j, 0, bpr, true);
			// make sure we are ot out of range at the last line
			long len = areaGroup.Buffer.Size - (lost2.End / bpr) * bpr;
			if (len > bpr) len = bpr;
			RenderRowNormal(rLost2End, 0, (int)len, true);
		}


		// re-render the common parts that overlap the lost
		// parts as selections
		if (!common.IsEmpty()) {
			int rCommonStart = ((int)(common.Start - offset) / bpr);
			int rCommonEnd = ((int)(common.End - offset) / bpr);
			if (rCommonStart == rLost1End && rCommonEnd == rLost1End
					&& rLost1End == rLost1Start) {
				RenderRange(common, ht);
			}
			else if (rCommonStart == rLost2End && rCommonEnd == rLost2End
					 && rLost2End == rLost2Start) {
				RenderRange(common, ht);
			}
			else if (rCommonStart == rLost1End || rCommonStart == rLost2End) {
				int nbyte = (int)((common.Start - offset) % bpr);
				long overLeft = common.End - common.Start < bpr - nbyte ? common.End - common.Start : bpr - nbyte;
				Range rOverlap = new Range(common.Start, common.Start + overLeft);
				RenderRange(rOverlap, ht);
			}
			else if (rCommonEnd == rLost1Start || rCommonEnd == rLost2Start) {
				int nbyte = (int)((common.End - offset) % bpr);
				long rCommonEndOffset = rCommonEnd * bpr + offset;
				long overStart = rCommonEndOffset > common.Start ? rCommonEndOffset : common.Start;
				// part of common that is on row rCommonEnd
				Range rOverlap = new Range(overStart, rCommonEndOffset + nbyte);
				RenderRange(rOverlap, ht);
			}
		}


		// render the added parts as selections
		if (!added.IsEmpty()) {
			if ((added.Start == common.End + 1) && added.Start > 0)
				--added.Start;
			if (added.End == common.Start - 1)
				++added.End;
			RenderRange(added, ht);
		}

		if (!added1.IsEmpty()) {
			if ((added1.Start == common.End + 1) && added1.Start > 0)
				--added1.Start;
			if (added1.End == common.Start - 1)
				++added1.End;
			RenderRange(added1, ht);
		}

	}
	*/
	
	void RenderRangeHelper(Drawer.HighlightType ht, int rstart, int bstart, int len)
	{
		if (ht != Drawer.HighlightType.Normal)
			RenderRowHighlight(rstart, bstart, len, false, ht);
		else
			RenderRowNormal(rstart, bstart, len, false);
	}

	///<summary>
	/// Render the bytes in 'range'
	/// using the specified HighlightType
	///</summary>
	internal protected virtual void RenderRange(Range range, Drawer.HighlightType ht)
	{
		if (isAreaRealized == false)
			return;

		int rstart, bstart, xstart, ystart;
		int rend, bend, xend, yend;
		bool odd;
		Gdk.GC gc;
		Gdk.GC oddGC;
		Gdk.GC evenGC;


		oddGC = drawer.GetBackgroundGC(Drawer.RowType.Odd, ht);
		evenGC = drawer.GetBackgroundGC(Drawer.RowType.Even, ht);

		GetDisplayInfoByOffset(range.Start, out rstart, out bstart, out xstart, out ystart);
		GetDisplayInfoByOffset(range.End, out rend, out bend, out xend, out yend);

		//System.Console.WriteLine("Start {0:x} {1} {2} x:{3} y:{4}", range.Start, rstart, bstart, xstart, ystart);
		//System.Console.WriteLine("End {0:x} {1} {2} x:{3} y:{4}", range.End, rend, bend, xend, yend);

		// if the whole range is on one row
		if (rstart == rend) {
			if (manualDoubleBuffer)
				BeginPaint(x + xstart, y + ystart, xend - xstart + dpb*drawer.Width, drawer.Height);

			// odd row?
			odd = (((range.Start / bpr) % 2) == 1);
			if (odd)
				gc = oddGC;
			else
				gc = evenGC;

			//render
			backPixmap.DrawRectangle(gc, true, x + xstart, y + ystart, xend - xstart, drawer.Height);

			RenderRangeHelper(ht, rstart, bstart, bend - bstart + 1);
		}
		else { // multi-row range

			if (manualDoubleBuffer) {
				// handle double-buffering
				Gdk.Region paintRegion = new Gdk.Region();

				Gdk.Rectangle rectStart = new Gdk.Rectangle(x + xstart, y + ystart, width - xstart, drawer.Height);

				Gdk.Rectangle rectMiddle;
				if (rend > rstart + 1)
					rectMiddle = new Gdk.Rectangle(x, y + ystart + drawer.Height, width, yend - ystart - drawer.Height);
				else
					rectMiddle = Gdk.Rectangle.Zero;

				Gdk.Rectangle rectEnd = new Gdk.Rectangle(x, y + yend, xend + dpb*drawer.Width, drawer.Height);

				paintRegion.UnionWithRect(rectStart);
				paintRegion.UnionWithRect(rectMiddle);
				paintRegion.UnionWithRect(rectEnd);

				BeginPaintRegion(paintRegion);
			}

			// render first row
			odd = (((range.Start / bpr) % 2) == 1);
			if (odd)
				gc = oddGC;
			else
				gc = evenGC;
			backPixmap.DrawRectangle(gc, true, x + xstart, y + ystart, width - xstart, drawer.Height);

			RenderRangeHelper(ht, rstart, bstart, bpr - bstart);

			long curOffset = range.Start + bpr - bstart;

			// render middle rows
			for (int i = rstart + 1;i < rend;i++) {
				odd = (((curOffset / bpr) % 2) == 1);
				if (odd)
					gc = oddGC;
				else
					gc = evenGC;
				backPixmap.DrawRectangle(gc, true, x, y + i*drawer.Height, width, drawer.Height);
				RenderRangeHelper(ht, i, 0, bpr);
				curOffset += bpr;
			}

			// render last row
			odd = (((range.End / bpr) % 2) == 1);
			if (odd)
				gc = oddGC;
			else
				gc = evenGC;
			backPixmap.DrawRectangle(gc, true, x, y + yend, xend, drawer.Height);
			RenderRangeHelper(ht, rend, 0, bend + 1);
		}

		if (manualDoubleBuffer)
			EndPaint();

	}
	/*
	///<summary>Render the cursor</summary>
	protected void RenderCursor()
	{
		if (isAreaRealized == false)
			return;

		Range sel = highlights[(int)Drawer.HighlightType.Selection].LastAdded;

		// don't draw the cursor when there is a selection
		if (!sel.IsEmpty())
			return;

		int cRow, cByte, cX, cY;
		GetDisplayInfoByOffset(cursorOffset, out cRow, out cByte, out cX, out cY);

		backPixmap.DrawRectangle(cursorGC, true, x + cX, y + cY + drawer.Height - 2, drawer.Width*dpb, 2);
		if (cursorFocus) {
			backPixmap.DrawRectangle(cursorGC, true, x + cX + cursorDigit*drawer.Width, y + cY, 1, drawer.Height - 2);
		}
	}
	*/
	
	public void DisposePixmaps()
	{
		if (isAreaRealized == false)
			return;

		backPixmap.Dispose();
		drawer.DisposePixmaps();
	}

	
	void BeginPaintRegion(Gdk.Region r)
	{
		Gdk.Window win = areaGroup.DrawingArea.GdkWindow;

		win.BeginPaintRegion(r);
	}

	void BeginPaint()
	{
		BeginPaint(this.x, this.y, this.width, this.height);
	}

	void BeginPaint(int x, int y, int w, int h)
	{
		Gdk.Window win = areaGroup.DrawingArea.GdkWindow;

		win.BeginPaintRect(new Gdk.Rectangle(x, y, w, h));
	}

	void EndPaint()
	{
		areaGroup.DrawingArea.GdkWindow.EndPaint();
	}

	///
	/// Public Interface
	///
	/*
	///<summary>Renders a single offset</summary>
	protected virtual void RenderOffset(long offs)
	{
		if (isAreaRealized == false)
			return;

		int nrows = height / drawer.Height;
		long bleft = nrows * bpr;

		if (bleft + offset >= areaGroup.Buffer.Size)
			bleft = areaGroup.Buffer.Size - offset;

		if (offs >= offset && offs < offset + bleft) {
			int pcRow, pcByte, pcX, pcY;
			GetDisplayInfoByOffset(offs, out pcRow, out pcByte, out pcX, out pcY);
			Drawer.HighlightType ht = GetOffsetHighlight(offs);
			if (ht != Drawer.HighlightType.Normal && enableHighlights[(int)ht])
				RenderRowHighlight(pcRow, pcByte, 1, false, ht);
			else
				RenderRowNormal(pcRow, pcByte, 1, false);
		}
		else if (offs == areaGroup.Buffer.Size && offs == offset + bleft) {
			int pcRow, pcByte, pcX, pcY;
			GetDisplayInfoByOffset(offs, out pcRow, out pcByte, out pcX, out pcY);
			Gdk.GC backEvenGC = drawer.GetBackgroundGC(Drawer.RowType.Even, Drawer.HighlightType.Normal);
			backPixmap.DrawRectangle(backEvenGC, true, x + pcX, y + pcY, drawer.Width*dpb, drawer.Height);
		}

	}
	*/

	internal virtual void BlankBackground()
	{
		Gdk.GC backEvenGC = drawer.GetBackgroundGC(Drawer.RowType.Even, Drawer.HighlightType.Normal);
		backPixmap.DrawRectangle(backEvenGC, true, x, y, width, height);
	}
/*
	///<summary>
	/// Highlight all the ranges that match the specified pattern and are visible in the DataView.
	///</summary>
	public virtual void AddHighlightPattern(byte[] pattern, Drawer.HighlightType ht)
	{
		int patLen = pattern.Length;

		// set low limit
		long lowLimit = offset - patLen + 1;
		if (lowLimit < 0)
			lowLimit = offset;

		// set high limit
		int nrows = height / drawer.Height;
		long bleft = nrows * bpr;
		long highLimit;

		if (bleft + offset >= areaGroup.Buffer.Size)
			bleft = areaGroup.Buffer.Size - offset;

		if (bleft > 0)
			highLimit = offset + bleft - 1;
		else
			highLimit = -1;

		Range rClip = new Range(offset, highLimit);

		if (highLimit + patLen - 1 < areaGroup.Buffer.Size)
			highLimit += patLen - 1;
		else
			highLimit = areaGroup.Buffer.Size - 1;

		findStrategy.Buffer = areaGroup.Buffer;
		findStrategy.Position = lowLimit;
		findStrategy.Pattern = pattern;

		Range match;
		Range inter1 = new Range();


		while ((match = findStrategy.FindNext(highLimit)) != null) {
			// highlight areas that don't overlap with the selection
			match.Difference(Selection, inter1);
			match.Intersect(rClip);
			Range prevHighlight = new Range(highlights[(int)ht].LastAdded);
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
*/
	public virtual void ShowPopup(Gtk.UIManager uim)
	{
		Gtk.Widget popup = uim.GetWidget("/DefaultAreaPopup");
		(popup as Gtk.Menu).Popup();
	}

}// Area


} //namespace
