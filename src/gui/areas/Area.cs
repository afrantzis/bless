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

/// <summary>
/// An area on the screen that displays data in a specific way.
/// </summary>
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
	
	public enum RenderMergeFlags {None = 0, Left = 1, Right = 2}
	// Abstract methods
	
	/// <summary>
	/// Render (a part of) a row normally (without highlighting)
	/// </summary>
	/// <param name="i">
	/// The row to render.
	/// </param>
	/// <param name="p">
	/// The offset of the byte in the row to start rendering.
	/// </param>
	/// <param name="n">
	/// The number of bytes to render.
	/// </param>
	/// <param name="blank">
	/// Whether to blank the background before rendering.
	/// </param>
	abstract protected void RenderRowNormal(int i, int p, int n, bool blank);
	
	/// <summary>
	/// Render (a part of) a row with highlighting.
	/// </summary>
	/// <param name="i">
	/// The row to render.
	/// </param>
	/// <param name="p">
	/// The offset of the byte in the row to start rendering.
	/// </param>
	/// <param name="n">
	/// The number of bytes to render.
	/// </param>
	/// <param name="blank">
	/// Whether to blank the background before rendering.
	/// </param>
	abstract protected void RenderRowHighlight(int i, int p, int n, bool blank, Drawer.HighlightType ht);

	/// <summary>
	/// Get info about where a specified buffer offset is currently displayed.
	/// </summary>
	/// <param name="off">
	/// The offset to query.
	/// </param>
	/// <param name="orow">
	/// The row the offset is displayed in.
	/// </param>
	/// <param name="obyte">
	/// The byte in the row the offset is displayed on.
	/// </param>
	/// <param name="ox">
	/// The X coordinate of the displayed position.
	/// </param>
	/// <param name="oy">
	/// The Y coordinate of the displayed position.
	/// </param>
	abstract public void GetDisplayInfoByOffset(long off, out int orow, out int obyte, out int ox, out int oy);

	/// <summary>
	/// Flags containing extra info about positions in the view. 
	/// </summary>
	public enum GetOffsetFlags {
		/// <summary>Position is beyond the end of file</summary>
		Eof = 1,
		/// <summary>Position is between bytes (in the so-called abyss)</summary>
		Abyss = 2
	}
	
	/// <summary>
	/// Gets the offset in the buffer that is displayed at specified location in the area
	/// </summary>
	/// <param name="x">
	/// The X coordinate of the queried location.
	/// </param>
	/// <param name="y">
	/// The Y coordinate of the queried location.
	/// </param>
	/// <param name="digit">
	/// The digit of the byte at the specified location.
	/// </param>
	/// <param name="rflags">
	/// Some flags with additional info about the location (<see cref="GetOffsetFlags"/>).
	/// </param>
	/// <returns>
	/// The offset of the byte at the specified location.
	/// </returns>
	abstract public long GetOffsetByDisplayInfo(int x, int y, out int digit, out GetOffsetFlags rflags);

	/// <summary>
	/// Special key handler for the area. 
	/// </summary>
	/// <param name="key">
	/// The key that was pressed.
	/// </param>
	/// <param name="overwrite">
	/// Whether we are in overwrite mode.
	/// </param>
	/// <returns>
	/// Whether the key was actually handled.
	/// </returns>
	virtual public bool HandleKey(Gdk.Key key, bool overwrite)
	{
		return false;
	}

	///<summary>
	/// Calculates the width in pixels the
	/// area will occupy with n bytes per row.
	///</summary>
	abstract public int CalcWidth(int n, bool force);

	/// <summary>
	/// A delegate that creates instances of area types.
	/// </summary>
	public delegate Area AreaCreatorFunc(AreaGroup ag);
	
	/// <summary>
	/// A dictionary to hold area type names and creation functions
	/// </summary>
	static private Dictionary<string, AreaCreatorFunc> pluginTable;
	
	/// <summary>
	/// Adds a new area type to the factory.
	/// </summary>
	/// <param name="name">
	/// The name of the area type
	/// </param>
	/// <param name="createArea">
	/// The delegate to call to create an instance of this area type.
	/// </param>
	/// <remarks>This is used by plugins to register new area types.</remarks>
	static public void AddFactoryItem(string name, AreaCreatorFunc createArea)
	{
		if (pluginTable == null) {
			pluginTable = new Dictionary<string, AreaCreatorFunc>();
		}
		// System.Console.WriteLine("Adding plugin name {0}", name);
		pluginTable.Add(name, createArea);
	}

	/// <summary>
	/// Factory method to create instances of area types by name.
	/// </summary>
	/// <param name="name">
	/// The name of the type of the area to create.
	/// </param>
	/// <param name="ag">
	/// The <see cref="AreaGroup"/> this area will belong in.
	/// </param>
	/// <returns>
	/// The created area.
	/// </returns>
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

	
	/// <summary>
	/// Create an area.
	/// </summary>
	/// <param name="areaGroup">
	/// The <see cref="AreaGroup"/> this area will belong in.
	/// </param>
	public Area(AreaGroup areaGroup)
	{
		this.areaGroup = areaGroup;
		
		drawerInformation = new Drawer.Information();

		canFocus = false;
		dpb = 0;
		fixedBpr = -1;
		isAreaRealized = false;
	}

	/// <summary>
	/// Configure an area using an xml configuration.
	/// </summary>
	/// <param name="parentNode">
	/// The <see cref="XmlNode"/> that holds the configuration for this area.
	/// </param>
	/// <remarks>This is overriden by inherited classes to accept extra options</remarks>
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

	/// <summary>
	/// This method is only called to draw extra stuff when doing a complete
	/// redraw of the area
	/// </summary>
	internal protected virtual void RenderExtra()
	{
	
	}
	
	
	void RenderRangeHelper(Drawer.HighlightType ht, int rstart, int bstart, int len)
	{
		if (ht != Drawer.HighlightType.Normal)
			RenderRowHighlight(rstart, bstart, len, false, ht);
		else
			RenderRowNormal(rstart, bstart, len, false);
	}


	/// <summary>
	/// Render the bytes in the highlight range.
	/// </summary>
	/// <param name="h">
	/// A <see cref="Highlight"/>
	/// </param>
	/// <param name="merge">
	/// Whether to visually merge the highlight with adjacent ones left and/or right.
	/// </param>
	/// <remarks>
	/// This method doesn't check whether the merge flags are correct (eg there is indeed
	/// a similar adjacent highlight). It just draws the highlight in such a way as 
	/// to appear merged to any similar highlights if they exist.
	///</remarks>
	internal protected virtual void RenderHighlight(Highlight h, RenderMergeFlags merge)
	{
		if (isAreaRealized == false)
			return;
		
		int rstart, bstart, xstart, ystart;
		int rend, bend, xend, yend;
		bool odd;
		Gdk.GC gc;
		Gdk.GC oddGC;
		Gdk.GC evenGC;


		oddGC = drawer.GetBackgroundGC(Drawer.RowType.Odd, h.Type);
		evenGC = drawer.GetBackgroundGC(Drawer.RowType.Even, h.Type);

		GetDisplayInfoByOffset(h.Start, out rstart, out bstart, out xstart, out ystart);
		GetDisplayInfoByOffset(h.End, out rend, out bend, out xend, out yend);
		
		//System.Console.WriteLine("Start {0:x} {1} {2} x:{3} y:{4}", h.Start, rstart, bstart, xstart, ystart);
		//System.Console.WriteLine("End {0:x} {1} {2} x:{3} y:{4}", h.End, rend, bend, xend, yend);
		
		if ((merge & RenderMergeFlags.Left) != 0 && bstart > 0) {
			int digit;
			GetOffsetFlags gof;
			GetOffsetByDisplayInfo(xstart - 1, ystart, out digit, out gof);
			if ((gof & GetOffsetFlags.Abyss) != 0) {
				xstart -= drawer.Width; 
			}
		}
		
		if ((merge & RenderMergeFlags.Right) != 0 && bend < bpr - 1) {
			int digit;
			GetOffsetFlags gof;
			GetOffsetByDisplayInfo(xend + dpb*drawer.Width, yend, out digit, out gof);
			if ((gof & GetOffsetFlags.Abyss) != 0) {
				xend += drawer.Width;
			}
		}
		
		// if the whole range is on one row
		if (rstart == rend) {
			if (areaGroup.ManualDoubleBuffer) {
				BeginPaint(x + xstart, y + ystart, xend - xstart + dpb*drawer.Width, drawer.Height);
			}
			// odd row?
			odd = (((h.Start / bpr) % 2) == 1);
			if (odd)
				gc = oddGC;
			else
				gc = evenGC;

			//render
			backPixmap.DrawRectangle(gc, true, x + xstart, y + ystart, xend - xstart + dpb*drawer.Width, drawer.Height);

			RenderRangeHelper(h.Type, rstart, bstart, bend - bstart + 1);
		}
		else { // multi-row range

			if (areaGroup.ManualDoubleBuffer) {
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
			odd = (((h.Start / bpr) % 2) == 1);
			if (odd)
				gc = oddGC;
			else
				gc = evenGC;
			backPixmap.DrawRectangle(gc, true, x + xstart, y + ystart, width - xstart, drawer.Height);

			RenderRangeHelper(h.Type, rstart, bstart, bpr - bstart);

			long curOffset = h.Start + bpr - bstart;

			// render middle rows
			for (int i = rstart + 1;i < rend;i++) {
				odd = (((curOffset / bpr) % 2) == 1);
				if (odd)
					gc = oddGC;
				else
					gc = evenGC;
				backPixmap.DrawRectangle(gc, true, x, y + i*drawer.Height, width, drawer.Height);
				RenderRangeHelper(h.Type, i, 0, bpr);
				curOffset += bpr;
			}

			// render last row
			odd = (((h.End / bpr) % 2) == 1);
			if (odd)
				gc = oddGC;
			else
				gc = evenGC;
			backPixmap.DrawRectangle(gc, true, x, y + yend, xend + dpb*drawer.Width, drawer.Height);
			RenderRangeHelper(h.Type, rend, 0, bend + 1);
		}

		if (areaGroup.ManualDoubleBuffer) {
			EndPaint();
		}

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
	
	/// <summary>
	/// Dispose the (server side) pixmaps used by this area.
	/// </summary>
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
