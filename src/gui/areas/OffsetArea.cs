// created on 6/14/2004 at 11:05 PM
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

using System;
using Gtk;
using Gdk;
using Bless.Gui.Drawers;
using Bless.Util;
using System.Xml;
using Bless.Plugins;

namespace Bless.Gui.Areas.Plugins {

public class OffsetAreaPlugin : AreaPlugin
{
	public OffsetAreaPlugin()
	{
		name = "offset";
		author = "Alexandros Frantzis";
	}

	public override Area CreateArea()
	{
		return new OffsetArea();
	}
}

///<summary>An area that display the offsets</summary>
public class OffsetArea : Area {

	int bytes;

	public int Bytes {
		get { return bytes; }
		set { bytes = value; }
	}

	public OffsetArea()
			: base()
	{
		type = "offset";
		bytes = 4;
	}

	protected override void RenderRange(Bless.Util.Range range, Drawer.HighlightType ht)
	{
	}

	public override void Scroll(long offset)
	{
		if (bpr <= 0)
			return;

		Gdk.GC backEvenGC = drawer.GetBackgroundGC(Drawer.RowType.Even, Drawer.HighlightType.Normal);
		// draw the area background
		backPixmap.DrawRectangle(backEvenGC, true, x, y, width, height);

		int nrows = height / drawer.Height;
		long bleft = nrows * bpr;
		int rfull = 0;
		int blast = 0;

		if (bleft + offset > byteBuffer.Size)
			bleft = byteBuffer.Size - offset + 1;

		// calculate number of full rows
		// and number of bytes in last (non-full)
		rfull = (int)(bleft / bpr);
		blast = (int)(bleft % bpr);

		if (blast > 0)
			rfull++;

		this.offset = offset;

		for (int i = 0; i < rfull; i++)
			RenderRowNormal(i, 0, bpr, true);

		//Gdk.Window win=drawingArea.GdkWindow;
		//win.DrawDrawable(backEvenGC, backPixmap, 0, 0, x, y, width, height);
	}

	protected override void RenderRowNormal(int i, int p, int n, bool blank)
	{
		int rx = (bytes - 1) * 2 * drawer.Width + x;
		int ry = i * drawer.Height + y;
		long roffset = offset + i * bpr;
		bool odd;
		Gdk.GC backEvenGC = drawer.GetBackgroundGC(Drawer.RowType.Even, Drawer.HighlightType.Normal);
		Gdk.GC backOddGC = drawer.GetBackgroundGC(Drawer.RowType.Odd, Drawer.HighlightType.Normal);

		// odd row?
		odd = (((roffset / bpr) % 2) == 1);

		if (blank == true) {
			if (odd)
				backPixmap.DrawRectangle(backOddGC, true, rx, ry, width, drawer.Height);
			else
				backPixmap.DrawRectangle(backEvenGC, true, rx, ry, width, drawer.Height);
		}

		Drawer.RowType rowType;

		if (odd)
			rowType = Drawer.RowType.Odd;
		else
			rowType = Drawer.RowType.Even;

		// if nothing to draw return
		if (n == 0)
			return;

		// draw offsets
		for (int j = 0; j < bytes; j++) {
			drawer.DrawNormal(backEvenGC, backPixmap, rx, ry, (byte)(roffset & 0xff), rowType, Drawer.ColumnType.Even);
			roffset = roffset >> 8;
			rx = rx - 2 * drawer.Width;
		}
	}

	protected override void RenderRowHighlight(int i, int p, int n, bool blank, Drawer.HighlightType ht)
	{
		RenderRowNormal(i, p, n, blank);
	}

	public override int CalcWidth(int n, bool force)
	{
		return 2*bytes*drawer.Width;
	}

	public override void GetDisplayInfoByOffset(long off, out int orow, out int obyte, out int ox, out int oy)
	{
		orow = (int)((off - offset) / bpr);
		obyte = (int)((off - offset) % bpr);

		oy = orow * drawer.Height;

		ox = 0;
	}

	public override long GetOffsetByDisplayInfo(int x, int y, out int digit, out GetOffsetFlags flags)
	{
		flags = 0;
		int row = y / drawer.Height;
		long off = row * bpr + offset;
		if (off >= byteBuffer.Size)
			flags |= GetOffsetFlags.Eof;

		digit = 0;

		return off;
	}

	public override void SetSelection(long start, long end)
	{
		SetSelectionNoRender(start, end);
	}

	public override void MoveCursor(long offset, int digit)
	{
		MoveCursorNoRender(offset, digit);
	}

	public override void Configure(XmlNode parentNode)
	{
		base.Configure(parentNode);

		XmlNodeList childNodes = parentNode.ChildNodes;
		foreach(XmlNode node in childNodes) {
			if (node.Name == "case")
				drawerInformation.Uppercase = (node.InnerText == "upper");
			if (node.Name == "bytes")
				this.Bytes = Convert.ToInt32(node.InnerText);
		}
	}

	public override void Realize (DrawingArea da)
	{
		drawer = new HexDrawer(da, drawerInformation);
		base.Realize(da);
	}
}


}//namespace