// created on 7/1/2004 at 8:10 PM
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
 
///<summary>Draws the decimal representation of a byte</summary>
public class DecimalDrawer : Drawer {

	static readonly string DecimalTable="000001002003004005006007008009010011012013014015016017018019020021022023024025026027028029030031032033034035036037038039040041042043044045046047048049050051052053054055056057058059060061062063064065066067068069070071072073074075076077078079080081082083084085086087088089090091092093094095096097098099100101102103104105106107108109110111112113114115116117118119120121122123124125126127128129130131132133134135136137138139140141142143144145146147148149150151152153154155156157158159160161162163164165166167168169170171172173174175176177178179180181182183184185186187188189190191192193194195196197198199200201202203204205206207208209210211212213214215216217218219220221222223224225226227228229230231232233234235236237238239240241242243244245246247248249250251252253254255";

	public DecimalDrawer(Gtk.Widget wid, Information inf)
	:base(wid, inf)
	{
	}

	protected override void Draw(Gdk.GC gc, Gdk.Drawable dest, int x, int y, byte b, Gdk.Pixmap pix) 
	{
		dest.DrawDrawable(gc, pix, b*3*width, 0, x, y, 3*width, height);
	}
	
	protected override Gdk.Pixmap Create(Gdk.Color fg, Gdk.Color bg)
	{
		Gdk.Window win=widget.GdkWindow;
		
		Gdk.GC gc=new Gdk.GC(win);
		Gdk.Pixmap pix=new Gdk.Pixmap(win, 256*3*width, height, -1);
		
		// draw the background		
		gc.RgbFgColor=bg;
		pix.DrawRectangle(gc, true, 0, 0, 256*3*width, height);	
	
		// render the bytes
		string s=DecimalDrawer.DecimalTable;
		
		//System.Console.WriteLine(s);
		
		pangoLayout.SetText(s);
		
		
		gc.RgbFgColor=fg;
		pix.DrawLayout(gc, 0, 0, pangoLayout);
		
		return pix;
	}
	
	

}

} //namespace