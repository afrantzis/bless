// created on 7/1/2004 at 8:39 PM
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

namespace Bless.Gui.Areas {

public class OctalAreaPlugin : AreaPlugin
{
	public OctalAreaPlugin()
	{
		name = "octal";
		author = "Alexandros Frantzis";
	}

	public override Area CreateArea()
	{
		return new OctalArea();
	}
}

///<summary>An area that displays octal</summary>
public class OctalArea : GroupedArea {

	public OctalArea()
			: base()
	{
		type = "octal";
		dpb = 3;
	}

	public override bool HandleKey(Gdk.Key key, bool overwrite)
	{
		//System.Console.WriteLine("Octal: {0}", key);

		if (key >= Gdk.Key.Key_0 && key <= Gdk.Key.Key_7 || (key >= Gdk.Key.KP_0 && key <= Gdk.Key.KP_7)) {
			byte b;
			if (key >= Gdk.Key.Key_0 && key <= Gdk.Key.Key_7)
				b = (byte)(key - Gdk.Key.Key_0);
			else
				b = (byte)(key - Gdk.Key.KP_0);

			byte orig;

			// if we are after the end of the buffer, assume
			// a 0 byte
			if (cursorOffset == byteBuffer.Size || (overwrite == false && cursorDigit == 0))
				orig = 0;
			else
				orig = byteBuffer[cursorOffset];

			byte orig1 = (byte)(orig % 8);
			byte orig8 = (byte)((orig / 8) % 8);
			byte orig64 = (byte)((orig / 64) % 8);

			if (cursorDigit == 0)
				orig64 = b;
			else if (cursorDigit == 1)
				orig8 = b;
			else if (cursorDigit == 2)
				orig1 = b;

			int repl = orig64 * 64 + orig8 * 8 + orig1;

			if (repl > 255)
				return false;

			byte[] ba = new byte[]{(byte)repl};

			if (cursorOffset == byteBuffer.Size)
				byteBuffer.Append(ba);
			else if (overwrite == false && cursorDigit == 0)
				byteBuffer.Insert(cursorOffset, ba);
			else /*(if (overwrite==true || cursorDigit > 0)*/
				byteBuffer.Replace(cursorOffset, cursorOffset, ba);


			return true;

		} else
			return false;
	}

	public override void Realize (DrawingArea da)
	{
		drawer = new OctalDrawer(da, drawerInformation);
		base.Realize(da);
	}
}

}//namespace