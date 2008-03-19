// created on 7/16/2004 at 7:54 PM
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
using System.Xml;

namespace Bless.Gui.Areas.Plugins {

///<summary>An area that displays grouped bytes</summary>
abstract public class GroupedArea : Area {

	int grouping;

	public GroupedArea(AreaGroup ag)
			: base(ag)
	{
		grouping = 1;
		canFocus = true;
	}


	public int Grouping {
		set {  grouping = value; }
		get {  return grouping; }

	}

	protected override void RenderRowNormal(int i, int p, int n, bool blank)
	{
		int rx = 0 + x;
		int ry = i * drawer.Height + y;
		long roffset = areaGroup.Offset + i * bpr + p;
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

		Drawer.ColumnType colType;
		Drawer.RowType rowType;

		if (odd)
			rowType = Drawer.RowType.Odd;
		else
			rowType = Drawer.RowType.Even;

		int pos = 0;
		// draw bytes
		while (true) {

			if (pos >= p) { //don't draw until we reach p
				if ((pos / grouping) % 2 == 0)
					colType = Drawer.ColumnType.Even;
				else
					colType = Drawer.ColumnType.Odd;

				drawer.DrawNormal(backEvenGC, backPixmap, rx, ry, areaGroup.Buffer[roffset++], rowType, colType);
				if (--n <= 0)
					break;
			}

			// space if necessary
			if (pos % grouping == grouping - 1)
				rx = rx + (dpb + 1) * drawer.Width;
			else
				rx = rx + dpb * drawer.Width;

			pos++;
		}
	}

	protected override void RenderRowHighlight(int i, int p, int n, bool blank, Drawer.HighlightType ht)
	{
		int rx = 0 + x;
		int ry = i * drawer.Height + y;
		long roffset = areaGroup.Offset + i * bpr + p;
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

		int pos = 0;
		// draw bytes
		while (true) {

			if (pos >= p) { //don't draw until we reach p
				drawer.DrawHighlight(backEvenGC, backPixmap, rx, ry, areaGroup.Buffer[roffset++], rowType, ht);
				if (--n <= 0)
					break;
			}

			// space if necessary
			if (pos % grouping == grouping - 1)
				rx = rx + (dpb + 1) * drawer.Width;
			else
				rx = rx + dpb * drawer.Width;

			pos++;
		}
	}

	public override int CalcWidth(int n, bool force)
	{
		if (n == 0)
			return 0;
		if (fixedBpr > 0 && n > fixedBpr && !force) // must adhere to fixed length
			return -1;
		if (n % grouping != 0 && !force) // can't break the grouping
			return -1;

		int ngroups = n / grouping;
		int groupWidth = grouping * dpb * drawer.Width;

		return ngroups*groupWidth + (ngroups - 1)*drawer.Width;
	}

	public override void GetDisplayInfoByOffset(long off, out int orow, out int obyte, out int ox, out int oy)
	{
		orow = (int)((off - areaGroup.Offset) / bpr);
		obyte = (int)((off - areaGroup.Offset) % bpr);

		oy = orow * drawer.Height;

		int group = obyte / grouping;
		int groupOffset = obyte % grouping;
		ox = group * (grouping * dpb * drawer.Width + drawer.Width) + dpb * drawer.Width * groupOffset;
	}

	public override long GetOffsetByDisplayInfo(int x, int y, out int digit, out GetOffsetFlags flags)
	{
		flags = 0;
		int groupWidth = (grouping * dpb * drawer.Width + drawer.Width);

		int row = y / drawer.Height;
		int group = x / groupWidth;
		int groupByte = (x - group * groupWidth) / (dpb * drawer.Width);

		digit = (x - group * groupWidth - groupByte * dpb * drawer.Width) / drawer.Width;

		if (groupByte >= grouping) {
			groupByte = grouping - 1;
			flags |= GetOffsetFlags.Abyss;
		}

		long off = row * bpr + (group * grouping + groupByte) + areaGroup.Offset;
		if (off >= areaGroup.Buffer.Size)
			flags |= GetOffsetFlags.Eof;

		return off;

	}

	public override void Configure(XmlNode parentNode)
	{
		base.Configure(parentNode);

		XmlNodeList childNodes = parentNode.ChildNodes;
		foreach(XmlNode node in childNodes) {
			if (node.Name == "grouping")
				this.Grouping = Convert.ToInt32(node.InnerText);
		}
	}

}// end GroupedArea

}//end namespace