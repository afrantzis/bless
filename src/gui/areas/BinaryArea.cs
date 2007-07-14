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

	public override Area CreateArea()
	{
		return new BinaryArea();
	}
}

///<summary>An area that displays binary</summary>
public class BinaryArea : GroupedArea {

	public BinaryArea()
			: base()
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
			if (cursorOffset == byteBuffer.Size || (overwrite == false && cursorDigit == 0))
				orig = 0;
			else
				orig = byteBuffer[cursorOffset];

			byte repl;

			if (key == Gdk.Key.Key_1 || key == Gdk.Key.KP_1 )
				repl = (byte)((1 << (dpb - cursorDigit - 1)) | orig);
			else
				repl = (byte)((~(1 << (dpb - cursorDigit - 1))) & orig);

			byte[] ba = new byte[]{repl};

			if (cursorOffset == byteBuffer.Size)
				byteBuffer.Append(ba);
			else if (overwrite == false && cursorDigit == 0)
				byteBuffer.Insert(cursorOffset, ba);
			else /*(if (overwrite==true || cursorDigit > 0)*/
				byteBuffer.Replace(cursorOffset, cursorOffset, ba);


			return true;

		}
		else
			return false;
	}

	public override void Realize (DrawingArea da)
	{
		drawer = new BinaryDrawer(da, drawerInformation);
		base.Realize(da);
	}
}

}//namespace