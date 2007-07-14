// created on 6/15/2004 at 4:10 PM
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
using System.Xml;
using Bless.Plugins;
using Bless.Gui.Drawers;
using Gtk;

namespace Bless.Gui.Areas.Plugins {

public class HexAreaPlugin : AreaPlugin
{
	public HexAreaPlugin()
	{
		name = "hexadecimal";
		author = "Alexandros Frantzis";
	}

	public override Area CreateArea()
	{
		return new HexArea();
	}
}
///<summary>An area that displays hexadecimal</summary>
public class HexArea : GroupedArea {


	public HexArea()
			: base()
	{
		type = "hexadecimal";
		dpb = 2;
	}

	private int KeyToHex(Gdk.Key key)
	{
		if (key >= Gdk.Key.Key_0 && key <= Gdk.Key.Key_9)
			return key - Gdk.Key.Key_0;
		else if (key >= Gdk.Key.A && key <= Gdk.Key.F)
			return key - Gdk.Key.A + 10;
		else if (key >= Gdk.Key.a && key <= Gdk.Key.f)
			return key - Gdk.Key.a + 10;
		else if (key >= Gdk.Key.KP_0 && key <= Gdk.Key.KP_9)
			return key - Gdk.Key.KP_0;
		else
			return -1;
	}

	public override bool HandleKey(Gdk.Key key, bool overwrite)
	{
		int hex = KeyToHex(key);

		if (hex != -1) {
			byte b = (byte)hex;
			byte orig;

			// if we are after the end of the buffer, assume
			// a 0 byte
			if (cursorOffset == byteBuffer.Size || (overwrite == false && cursorDigit == 0))
				orig = 0;
			else
				orig = byteBuffer[cursorOffset];

			byte orig1 = (byte)(orig % 16);
			byte orig16 = (byte)((orig / 16) % 16);


			if (cursorDigit == 0)
				orig16 = b;
			else if (cursorDigit == 1)
				orig1 = b;

			byte repl = (byte)(orig16 * 16 + orig1);

			byte[] ba = new byte[]{repl};

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

	public override void Configure(XmlNode parentNode)
	{
		base.Configure(parentNode);

		XmlNodeList childNodes = parentNode.ChildNodes;
		foreach(XmlNode node in childNodes) {
			if (node.Name == "case")
				drawerInformation.Uppercase = (node.InnerText == "upper");
		}
	}

	public override void Realize (DrawingArea da)
	{
		drawer = new HexDrawer(da, drawerInformation);
		base.Realize(da);
	}
}

}//namespace