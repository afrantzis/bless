// created on 6/18/2004 at 7:46 PM
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
using Bless.Plugins;

namespace Bless.Gui.Areas.Plugins {

public class SeparatorAreaPlugin : AreaPlugin
{
	public SeparatorAreaPlugin()
	{
		name = "separator";
		author = "Alexandros Frantzis";
	}

	public override Area CreateArea(AreaGroup ag)
	{
		return new SeparatorArea(ag);
	}
}

///<summary>An area that contains a vertical separator line</summary>
public class SeparatorArea : Area
{
	Gdk.GC lineGC;

	public SeparatorArea(AreaGroup ag)
			: base(ag)
	{
		type = "separator";
	}

	public override void Realize()
	{
		Gtk.DrawingArea da = areaGroup.DrawingArea;
		
		drawer = new DummyDrawer(da, drawerInformation);

		lineGC = new Gdk.GC(da.GdkWindow);

		lineGC.RgbFgColor = drawer.Info.fgNormal[(int)Drawer.RowType.Even, (int)Drawer.ColumnType.Even];

		base.Realize();
	}

	protected override void RenderRowNormal(int i, int p, int n, bool blank)
	{
	}

	protected override void RenderRowHighlight(int i, int p, int n, bool blank, Drawer.HighlightType ht)
	{
	}

	public override int CalcWidth(int n, bool force)
	{
		return drawer.Width;
	}

	public override void GetDisplayInfoByOffset(long off, out int orow, out int obyte, out int ox, out int oy)
	{
		orow = (int)((off - areaGroup.Offset) / bpr);
		obyte = (int)((off - areaGroup.Offset) % bpr);

		oy = orow * drawer.Height;

		ox = 0;
	}

	public override long GetOffsetByDisplayInfo(int x, int y, out int digit, out  GetOffsetFlags flags)
	{
		flags = 0;
		int row = y / drawer.Height;
		long off = row * bpr + areaGroup.Offset;
		if (off >= areaGroup.Buffer.Size)
			flags |= GetOffsetFlags.Eof;

		digit = 0;

		return off;
	}
}


}//namespace