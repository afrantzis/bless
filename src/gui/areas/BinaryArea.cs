// created on 6/24/2004 at 2:32 PM
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
using Bless.Plugins;
using Bless.Gui.Drawers;
using Gtk;

namespace Bless.Gui.Areas.Plugins {

public class BinaryAreaPlugin : AreaPlugin
{
	public BinaryAreaPlugin()
	{
		name = "binary";
		author = "Alexandros Frantzis";
	}

	public override Area CreateArea(AreaGroup ag)
	{
		return new BinaryArea(ag);
	}
}

///<summary>An area that displays binary</summary>
public class BinaryArea : GroupedArea {

	public BinaryArea(AreaGroup ag)
			: base(ag)
	{
		type = "binary";
		dpb = 8;
	}

	public override bool HandleKey(Gdk.Key key, bool overwrite)
	{
		//System.Console.WriteLine("Binary: {0}", key);

		if (key == Gdk.Key.Key_0 || key == Gdk.Key.Key_1 || key == Gdk.Key.KP_0 || key == Gdk.Key.KP_1) {
			byte orig;

			// if we are after the end of the buffer, assume
			// a 0 byte
			if (areaGroup.CursorOffset == areaGroup.Buffer.Size || (overwrite == false && areaGroup.CursorDigit == 0))
				orig = 0;
			else
				orig = areaGroup.Buffer[areaGroup.CursorOffset];

			byte repl;

			if (key == Gdk.Key.Key_1 || key == Gdk.Key.KP_1 )
				repl = (byte)((1 << (dpb - areaGroup.CursorDigit - 1)) | orig);
			else
				repl = (byte)((~(1 << (dpb - areaGroup.CursorDigit - 1))) & orig);

			byte[] ba = new byte[]{repl};

			if (areaGroup.CursorOffset == areaGroup.Buffer.Size)
				areaGroup.Buffer.Append(ba, 0, ba.LongLength);
			else if (overwrite == false && areaGroup.CursorDigit == 0)
				areaGroup.Buffer.Insert(areaGroup.CursorOffset, ba, 0, ba.LongLength);
			else /*(if (overwrite==true || cursorDigit > 0)*/
				areaGroup.Buffer.Replace(areaGroup.CursorOffset, areaGroup.CursorOffset, ba);


			return true;

		}
		else
			return false;
	}

	public override void Realize ()
	{
		Gtk.DrawingArea da = areaGroup.DrawingArea;
		drawer = new BinaryDrawer(da, drawerInformation);
		base.Realize();
	}
}

}//namespace
