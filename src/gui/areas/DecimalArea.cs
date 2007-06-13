// created on 7/1/2004 at 8:06 PM
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

public class DecimalAreaPlugin : AreaPlugin
{
	public DecimalAreaPlugin()
	{
		name = "decimal";
		author = "Alexandros Frantzis";
	}

	public override Area CreateArea()
	{
		return new DecimalArea();
	}
}

///<summary>An area that displays decimal</summary>
public class DecimalArea : GroupedArea {

	public DecimalArea()
			: base()
	{
		type = "decimal";
		dpb = 3;
	}

	public override bool HandleKey(Gdk.Key key, bool overwrite)
	{
		//System.Console.WriteLine("Decimal: {0}", key);

		if ((key >= Gdk.Key.Key_0 && key <= Gdk.Key.Key_9) || (key >= Gdk.Key.KP_0 && key <= Gdk.Key.KP_9)) {
			byte b;
			if (key >= Gdk.Key.Key_0 && key <= Gdk.Key.Key_9)
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

			byte orig1 = (byte)(orig % 10);
			byte orig10 = (byte)((orig / 10) % 10);
			byte orig100 = (byte)((orig / 100) % 10);

			if (cursorDigit == 0)
				orig100 = b;
			else if (cursorDigit == 1)
				orig10 = b;
			else if (cursorDigit == 2)
				orig1 = b;

			int repl = orig100 * 100 + orig10 * 10 + orig1;
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
		drawer = new DecimalDrawer(da, drawerInformation);
		base.Realize(da);
	}
}

}//namespace