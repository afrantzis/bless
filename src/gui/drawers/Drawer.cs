// created on 6/14/2004 at 10:55 PM
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
using Pango;

namespace Bless.Gui.Drawers {

///<summary>Fast font drawing class</summary>
public abstract class Drawer {

	public class Information {
		public string FontName;
		public string FontLanguage;
		
		public Gdk.Color[,] fgNormal;
		public Gdk.Color[,] bgNormal;
		
		public Gdk.Color[,] fgHighlight;
		public Gdk.Color[,] bgHighlight;
		
		public bool Uppercase;
		
		public Information() 
		{	
			FontName="Courier 12";
			FontLanguage="utf-8";
			
			fgNormal=new Gdk.Color[2, 2];
			bgNormal=new Gdk.Color[2, 2];
			
			fgHighlight=new Gdk.Color[2, (int)HighlightType.Sentinel];
			bgHighlight=new Gdk.Color[2, (int)HighlightType.Sentinel];
			
			// initialize default colors
			Gdk.Color.Parse("black", ref fgNormal[(int)RowType.Even, (int)ColumnType.Even]);
			Gdk.Color.Parse("white", ref bgNormal[(int)RowType.Even, (int)ColumnType.Even]);
			
			Gdk.Color.Parse("blue", ref fgNormal[(int)RowType.Even, (int)ColumnType.Odd]);
			Gdk.Color.Parse("white", ref bgNormal[(int)RowType.Even, (int)ColumnType.Odd]);
			
			Gdk.Color.Parse("black", ref fgNormal[(int)RowType.Odd, (int)ColumnType.Even]);
			Gdk.Color.Parse("white", ref bgNormal[(int)RowType.Odd, (int)ColumnType.Even]);
			
			Gdk.Color.Parse("blue", ref fgNormal[(int)RowType.Odd, (int)ColumnType.Odd]);
			Gdk.Color.Parse("white", ref bgNormal[(int)RowType.Odd, (int)ColumnType.Odd]);
			
			// leave unspecified...
			// if not specified by user they will
			// be set up using theme defaults
			fgHighlight[(int)RowType.Even, (int)HighlightType.Selection]=Gdk.Color.Zero;
			bgHighlight[(int)RowType.Even, (int)HighlightType.Selection]=Gdk.Color.Zero;
			
			fgHighlight[(int)RowType.Odd, (int)HighlightType.Selection]=Gdk.Color.Zero;
			bgHighlight[(int)RowType.Odd, (int)HighlightType.Selection]=Gdk.Color.Zero;
			
			fgHighlight[(int)RowType.Even, (int)HighlightType.PatternMatch]=Gdk.Color.Zero;
			bgHighlight[(int)RowType.Even, (int)HighlightType.PatternMatch]=Gdk.Color.Zero;
			
			fgHighlight[(int)RowType.Odd, (int)HighlightType.PatternMatch]=Gdk.Color.Zero;
			bgHighlight[(int)RowType.Odd, (int)HighlightType.PatternMatch]=Gdk.Color.Zero;
			
			Uppercase=false;
		}
		
		// setup unspecified hightlight colors using theme default colors
		public void SetupHighlight(Gtk.Widget widget)
		{
			Gdk.Color selFg;
			Gdk.Color selBg;
			Gdk.Color patMatchFg;
			Gdk.Color patMatchBg;
			
			selFg=widget.Style.TextColors[(int)StateType.Selected];
			selBg=widget.Style.BaseColors[(int)StateType.Selected];
			patMatchBg=MakeColorLighter(selBg, 0.6);
			patMatchFg=MakeColorDarker(selFg, 0.4);
			
			// Selection
			if (fgHighlight[(int)RowType.Even, (int)HighlightType.Selection].Equals(Gdk.Color.Zero))
				fgHighlight[(int)RowType.Even, (int)HighlightType.Selection]=selFg;
			
			if (bgHighlight[(int)RowType.Even, (int)HighlightType.Selection].Equals(Gdk.Color.Zero))
				bgHighlight[(int)RowType.Even, (int)HighlightType.Selection]=selBg;
			
			if (fgHighlight[(int)RowType.Odd, (int)HighlightType.Selection].Equals(Gdk.Color.Zero))
				fgHighlight[(int)RowType.Odd, (int)HighlightType.Selection]=selFg;
			
			if (bgHighlight[(int)RowType.Odd, (int)HighlightType.Selection].Equals(Gdk.Color.Zero))
				bgHighlight[(int)RowType.Odd, (int)HighlightType.Selection]=selBg;
			
			// Secondary selection
			if (fgHighlight[(int)RowType.Even, (int)HighlightType.PatternMatch].Equals(Gdk.Color.Zero))
				fgHighlight[(int)RowType.Even, (int)HighlightType.PatternMatch]=patMatchFg;
			
			if (bgHighlight[(int)RowType.Even, (int)HighlightType.PatternMatch].Equals(Gdk.Color.Zero))
				bgHighlight[(int)RowType.Even, (int)HighlightType.PatternMatch]=patMatchBg;
			
			if (fgHighlight[(int)RowType.Odd, (int)HighlightType.PatternMatch].Equals(Gdk.Color.Zero))
				fgHighlight[(int)RowType.Odd, (int)HighlightType.PatternMatch]=patMatchFg;
			
			if (bgHighlight[(int)RowType.Odd, (int)HighlightType.PatternMatch].Equals(Gdk.Color.Zero))
				bgHighlight[(int)RowType.Odd, (int)HighlightType.PatternMatch]=patMatchBg;
		}
		
		// Make a color lighter while keeping its hue
		Gdk.Color MakeColorLighter(Gdk.Color col, double factor)
		{
			Gdk.Color light=new Gdk.Color();
			
			light.Red=(ushort)(col.Red+(ushort.MaxValue-col.Red)*factor);
			light.Blue=(ushort)(col.Blue+(ushort.MaxValue-col.Blue)*factor);
			light.Green=(ushort)(col.Green+(ushort.MaxValue-col.Green)*factor);
			return light;
		}
		
		// Make a color darker while keeping its hue
		Gdk.Color MakeColorDarker(Gdk.Color col, double factor)
		{
			Gdk.Color dark=new Gdk.Color();
			
			dark.Red=(ushort)(col.Red*factor);
			dark.Blue=(ushort)(col.Blue*factor);
			dark.Green=(ushort)(col.Green*factor);
			return dark;
		}
	}
	
	public enum HighlightType { Normal, Selection, PatternMatch, Bookmark, Sentinel }
	public enum RowType { Even, Odd }
	public enum ColumnType { Even, Odd }
	
	// the widget the font will finally 
	// be printed on (used for info only)
	protected Gtk.Widget widget;
	protected Pango.FontDescription fontDescription;
	protected Information info;
	
	protected Gdk.Pixmap[,] pixmapsNormal;
	protected Gdk.Pixmap[,] pixmapsHighlight;
	
	// pango layout used for rendering text
	protected Pango.Layout pangoLayout;
	
	protected Gdk.GC[,] backGC;
	protected int width;
	protected int height;
	
	///<summary>Constructor</summary>
	public Drawer(Gtk.Widget wid, Information inf)
	{
		widget=wid;
		info=inf;
		
		// make sure highlight colors are set
		info.SetupHighlight(wid);
		
		fontDescription=Pango.FontDescription.FromString(info.FontName);
		Pango.Language lang=Pango.Language.FromString(info.FontLanguage);
		
		Pango.Context pangoCtx=widget.PangoContext;
		pangoCtx.FontDescription=fontDescription;                               
		pangoCtx.Language=lang;                                                 
                                                                                
		// set the font height and width                                        
		pangoLayout=new Pango.Layout(pangoCtx);                                 
		// we use a monospaced font, the actual character doesn't matter        
		pangoLayout.SetText("X");                                               
		pangoLayout.GetPixelSize(out width, out height);                        
		pangoLayout.SetText("");  
		
		// create the font pixmaps
		InitializePixmaps();
		
		InitializeBackgroundGCs();
	}
	
	void InitializePixmaps()
	{		
		pixmapsNormal=new Gdk.Pixmap[2,2];
		pixmapsHighlight=new Gdk.Pixmap[2,(int)HighlightType.Sentinel];
		
		Gdk.Color colorFg=new Gdk.Color();
		Gdk.Color colorBg=new Gdk.Color();
		
		//even rows
		colorFg=info.fgNormal[(int)RowType.Even, (int)ColumnType.Even];
		colorBg=info.bgNormal[(int)RowType.Even, (int)ColumnType.Even];
		pixmapsNormal[(int)RowType.Even, (int)ColumnType.Even]=Create(colorFg, colorBg);
		
		colorFg=info.fgNormal[(int)RowType.Even, (int)ColumnType.Odd];
		colorBg=info.bgNormal[(int)RowType.Even, (int)ColumnType.Odd];
		pixmapsNormal[(int)RowType.Even, (int)ColumnType.Odd]=Create(colorFg, colorBg);
		
		colorFg=info.fgHighlight[(int)RowType.Even, (int)HighlightType.Selection];
		colorBg=info.bgHighlight[(int)RowType.Even, (int)HighlightType.Selection];
		pixmapsHighlight[(int)RowType.Even, (int)HighlightType.Selection]=Create(colorFg, colorBg);
		
		colorFg=info.fgHighlight[(int)RowType.Even, (int)HighlightType.PatternMatch];
		colorBg=info.bgHighlight[(int)RowType.Even, (int)HighlightType.PatternMatch];
		pixmapsHighlight[(int)RowType.Even, (int)HighlightType.PatternMatch]=Create(colorFg, colorBg);
		
		
		//odd rows
		colorFg=info.fgNormal[(int)RowType.Odd, (int)ColumnType.Even];
		colorBg=info.bgNormal[(int)RowType.Odd, (int)ColumnType.Even];
		pixmapsNormal[(int)RowType.Odd, (int)ColumnType.Even]=Create(colorFg, colorBg);
				
		colorFg=info.fgNormal[(int)RowType.Odd, (int)ColumnType.Odd];
		colorBg=info.bgNormal[(int)RowType.Odd, (int)ColumnType.Odd];
		pixmapsNormal[(int)RowType.Odd, (int)ColumnType.Odd]=Create(colorFg, colorBg);
		
		colorFg=info.fgHighlight[(int)RowType.Odd, (int)HighlightType.Selection];
		colorBg=info.bgHighlight[(int)RowType.Odd, (int)HighlightType.Selection];
		pixmapsHighlight[(int)RowType.Odd, (int)HighlightType.Selection]=Create(colorFg, colorBg);
		
		colorFg=info.fgHighlight[(int)RowType.Odd, (int)HighlightType.PatternMatch];
		colorBg=info.bgHighlight[(int)RowType.Odd, (int)HighlightType.PatternMatch];
		pixmapsHighlight[(int)RowType.Odd, (int)HighlightType.PatternMatch]=Create(colorFg, colorBg);
	}
	
	void InitializeBackgroundGCs()
	{	
		// initialize background GCs
		backGC=new Gdk.GC[2, (int)Drawer.HighlightType.Sentinel];
		
		for(int i=0; i<2; i++)
			for(int j=0; j<(int)Drawer.HighlightType.Sentinel; j++)
				backGC[i,j]=new Gdk.GC(widget.GdkWindow);
		
		Gdk.Color col;
		
		// normal
		col=info.bgNormal[(int)RowType.Even, (int)ColumnType.Even]; 
		backGC[(int)RowType.Even, (int)HighlightType.Normal].RgbFgColor=col;
		
		col=info.bgNormal[(int)RowType.Odd, (int)ColumnType.Even]; 
		backGC[(int)RowType.Odd, (int)HighlightType.Normal].RgbFgColor=col;
		
		// selection
		col=info.bgHighlight[(int)RowType.Even, (int)HighlightType.Selection]; 
		backGC[(int)RowType.Even, (int)HighlightType.Selection].RgbFgColor=col;
		
		col=info.bgHighlight[(int)RowType.Odd, (int)HighlightType.Selection]; 
		backGC[(int)RowType.Odd, (int)HighlightType.Selection].RgbFgColor=col;
		
		// secondary selection
		col=info.bgHighlight[(int)RowType.Even, (int)HighlightType.PatternMatch]; 
		backGC[(int)RowType.Even, (int)HighlightType.PatternMatch].RgbFgColor=col;
		
		col=info.bgHighlight[(int)RowType.Odd, (int)HighlightType.PatternMatch]; 
		backGC[(int)RowType.Odd, (int)HighlightType.PatternMatch].RgbFgColor=col;
	}
	
	///<summary>Creates a pixmap with the drawn data</summary>
	abstract protected Gdk.Pixmap Create(Gdk.Color fg, Gdk.Color bg);
	
	///<summary>Draws the a byte</summary>
	
	abstract protected void Draw(Gdk.GC gc, Gdk.Drawable dest, int x, int y, byte b, Gdk.Pixmap pix);
	
	public void DrawNormal(Gdk.GC gc, Gdk.Drawable dest, int x, int y, byte b, RowType rowType, ColumnType colType)
	{
		Draw(gc, dest, x, y, b, pixmapsNormal[(int)rowType, (int)colType]);
	}
 	
 	public void DrawHighlight(Gdk.GC gc, Gdk.Drawable dest, int x, int y, byte b, RowType rowType, HighlightType ht)
 	{
 		Draw(gc, dest, x, y, b, pixmapsHighlight[(int)rowType, (int)ht]);
 	}
 	
 	public Gdk.GC GetBackgroundGC(RowType rowType, HighlightType ht)
 	{
 		return backGC[(int)rowType, (int)ht];
 	}
 	
	public void DisposePixmaps()
	{
		foreach(Pixmap pix in pixmapsNormal)
			if (pix!=null)
				pix.Dispose();
		
		foreach(Pixmap pix in pixmapsHighlight)
			if (pix!=null)
				pix.Dispose();
	}
	
	public int Width{
		get { return width; }
	}
	
	public int Height{
		get { return height; }
	}
	
	public Drawer.Information Info{
		get { return info; }
	}

}

//<summary>dummy</summary>
public class DummyDrawer : Drawer {
	
	public DummyDrawer(Gtk.Widget wid, Information inf)
	:base(wid, inf)
	{
	}
	
	protected override void Draw(Gdk.GC gc, Gdk.Drawable dest, int x, int y, byte b, Gdk.Pixmap pix) 
	{
		
	}
	
	protected override Gdk.Pixmap Create(Gdk.Color fg, Gdk.Color bg)
	{
		return null;
	}
	
	

}

} // end namespace
