// created on 6/28/2004 at 4:48 PM
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

namespace Bless.Gui.Drawers {

///<summary>Draws the binary representation of a byte</summary>
public class BinaryDrawer : Drawer {
	
	public BinaryDrawer(Gtk.Widget wid, Information inf)
	:base(wid, inf)
	{
	}

	
	protected override void Draw(Gdk.GC gc, Gdk.Drawable dest, int x, int y, byte b, Gdk.Pixmap pix) 
	{
		// draw from the end backwards	
		x+=6*width;
		for (int i=0; i<4; i++) {
			byte k=(byte)(b & 3);
			dest.DrawDrawable(gc, pix, k*2*width, 0, x, y, 2*width, height);
			x-=2*width;
			b = (byte)(b >> 2);
		}
	}
	
	protected override Gdk.Pixmap Create(Gdk.Color fg, Gdk.Color bg)
	{
		Gdk.Window win=widget.GdkWindow;
		Pango.Context pangoCtx=widget.PangoContext;
		
		Gdk.GC gc=new Gdk.GC(win);
		Gdk.Pixmap pix=new Gdk.Pixmap(win, 4*2*width, height, -1);
		
		// draw the background		
		gc.RgbFgColor=bg;
		pix.DrawRectangle(gc, true, 0, 0, 4*2*width, height);	
	
		// render the bytes
		string s="00011011";
		
		
		//Console.WriteLine(s);
		
		Pango.Layout layout=new Pango.Layout(pangoCtx);
		layout.FontDescription=fontDescription;
		layout.SetText(s);
		
		
		gc.RgbFgColor=fg;
		pix.DrawLayout(gc, 0, 0, layout);
		
		return pix;
	}

}

} //namespace