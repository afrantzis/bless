// created on 1/11/2005 at 9:43 PM
/*
 *   Copyright (c) 2005, Alexandros Frantzis (alf82 [at] freemail [dot] gr)
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
using Bless.Buffers;
using Bless.Gui.Areas;
using Bless.Gui.Drawers;
using Bless.Util;

namespace Bless.Gui {

///<summary>A widget that displays data from a buffer</summary>
public class DataViewDisplay : Gtk.VBox {
	Layout layout;
	Gtk.HBox hbox;
	Gtk.DrawingArea drawingArea;
	//static Gtk.DrawingArea drawingArea=new Gtk.DrawingArea();
	Gtk.VScrollbar vscroll;
	Gtk.HBox fileChangedBar;
	bool widgetRealized;
	
	DataViewControl dvControl;
	DataView dataView;
	
	public enum ShowType { Closest, Start, End, Cursor }
	
	public DataView View {
		get { return dataView;}
	}
	
	public DataViewControl Control {
		set {
			DisconnectFromControl();
			// connect new control
			dvControl=value;
			ConnectToControl();
		}
		
		get { return dvControl; }
	}
	
	public Layout Layout {
		get { return layout; }
		
		set { 
			// temporarily save previous layout
			Layout prevLayout=layout;
			
			// dispose Area pixmaps
			DisposePixmaps();
			
			// set new layout
			layout=value;
			
			// set buffer
			foreach(Area a in layout.Areas) {
				a.Buffer=dataView.Buffer;
			}
			
			if (widgetRealized) {
				LayoutManager.RealizeLayout(layout, drawingArea);
				Gdk.Rectangle alloc=drawingArea.Allocation;
				Resize(alloc.Width, alloc.Height);

				long prevOffset=0;
				 
				// Setup new areas according to the old ones 
				if (prevLayout!=null && prevLayout.Areas.Count>0) {
					Area prevArea0=prevLayout.Areas[0] as Area;
					prevOffset=prevArea0.CursorOffset;
					foreach(Area a in layout.Areas) {
						a.SetSelection(prevArea0.Selection.Start, prevArea0.Selection.End);
						a.MoveCursor(prevOffset, 0);
					}
				}
				else {
					foreach(Area a in layout.Areas) 
						a.MoveCursor(0, 0);
				}
						
				// make sure cursor is visible		
				MakeOffsetVisible(prevOffset, ShowType.Closest);			
			}
		}
		
	}

	internal Gtk.VScrollbar VScroll {
		get { return vscroll; }
	}
	
	public new bool HasFocus {
		get { return drawingArea.HasFocus; }
		set { drawingArea.HasFocus=value; }
	}

	///<summary>Create a DataViewDisplay</summary>
	public DataViewDisplay(DataView dv)
	{
		dataView=dv;
		
		// load the default layout from the data directory
		layout = new Layout(FileResourcePath.GetSystemPath("..","data","bless-default.layout"));
		
		// initialize scrollbar
		Gtk.Adjustment 
			adj=new Gtk.Adjustment(0.0, 0.0, 1.0,1.0, 10.0, 0.0);
		vscroll=new Gtk.VScrollbar(adj); 
		
		adj.ValueChanged+=OnScrolled;
		
		// initialize drawing area
		drawingArea=new Gtk.DrawingArea();
		drawingArea.Realized+=OnRealized;
		drawingArea.ExposeEvent+=OnExposed;
		drawingArea.ConfigureEvent+=OnConfigured;
		drawingArea.ModifyBg(StateType.Normal, new Gdk.Color(0xff,0xff,0xff));
		
		// add events that we want to handle
		drawingArea.AddEvents((int)Gdk.EventMask.ButtonPressMask);
		drawingArea.AddEvents((int)Gdk.EventMask.ButtonReleaseMask);
		drawingArea.AddEvents((int)Gdk.EventMask.PointerMotionMask);
		drawingArea.AddEvents((int)Gdk.EventMask.PointerMotionHintMask);
		drawingArea.AddEvents((int)Gdk.EventMask.KeyPressMask);
		drawingArea.AddEvents((int)Gdk.EventMask.KeyReleaseMask);
		
		drawingArea.CanFocus=true; // needed to catch key events
		
		hbox=new Gtk.HBox();
		
		hbox.PackStart(drawingArea , true, true, 0);
		hbox.PackStart(vscroll , false, false, 0);
		
		this.PackStart(hbox);
	}
	
	///<summary>Force a complete redraw of the view</summary>
	public void Redraw()
	{
		if (!widgetRealized)
			return;
		
		Gdk.Rectangle alloc=drawingArea.Allocation;
		Resize(alloc.Width, alloc.Height);
		drawingArea.QueueDraw();
	}
	
	///<summary>
	/// Find the number of bytes per row in order 
	/// to best utilize the available space and
	/// keep all areas synchronized.
	///</summary>
	private int FindBestBpr(int width)
	{
		int n=1; // current bpr
		int bestBpr=-1; // best bpr so far
		int swBest=0; // width of best bpr so far 
		
		// try all values for n, from 0 upwards, 
		// until the width for a given n exceeds
		// the available or the fixed bpr of an
		// area is exceeded
		while(true) {
			int sw=0; // total width with current bpr
			bool breaksGrouping=false;
			bool breaksFixed=false;
			
			foreach(Area a in layout.Areas) {
				int w=a.CalcWidth(n, false);
				
				// if this number of bpr is not acceptable
				if (w==-1) {
					if (a.FixedBytesPerRow!=-1 && n > a.FixedBytesPerRow )
						breaksFixed=true;
					else
						breaksGrouping=true;
					break;
				}
				
				sw+=w;
			}
			
			// If current bpr breaks a fixed size area
			// stop searching and use the last bpr value
			// that did't break it.
			// If there isn't such an area, keep searching
			if (breaksFixed && bestBpr!=-1)
				break;
			
			// if current bpr breaks grouping, skip it
			if (!breaksGrouping) {
				bool shouldBreak=false;
				
				// stop searching if available width is exceeded
				// or last best width value equals current one
				if (sw>width || sw==swBest)
					shouldBreak=true;
				
				// if we should break, but haven't found a suitable
				// width yet, mark the current width as best so far
				// even if it violates available width constraints.
				if ((shouldBreak && bestBpr==-1) || (!shouldBreak)) {	
					// keep best bpr so far
					bestBpr=n;
					swBest=sw;
				}
				
				if (shouldBreak)
					break;
			}
			
			n++;
		}
		
		return bestBpr;
	}
	
	///<summary>Benchmark the rendering</summary>
	public void Benchmark() 
	{
		
		System.DateTime t1;
		System.DateTime t2;
		
		int sum=0;
		
		Gdk.Window win=drawingArea.GdkWindow;
		Gdk.Rectangle alloc=drawingArea.Allocation;
		Gdk.Rectangle rect1=new Gdk.Rectangle(0, 0,alloc.Width, alloc.Height); 
		
		for (int i=0; i<100; i++) {
			t1=System.DateTime.Now;
			
			win.BeginPaintRect(rect1);
		
			foreach(Area a in layout.Areas) 
				a.Scroll(0);	
			
			win.EndPaint();
		
			t2=System.DateTime.Now;
			
			sum+=(t2-t1).Milliseconds;
		}
		

		Gdk.Rectangle rect=drawingArea.Allocation;
		Console.WriteLine("100 render screen ({0},{1}): {2} ms", rect.Width, rect.Height,sum/100);
	
	}
	
	private void SetupScrollbarRange()
	{
		if (layout.Areas.Count<=0)
			return;
		
		long bpr=((Area)layout.Areas[0]).BytesPerRow;
		long nrows=((dataView.Buffer.Size+1)/bpr); // +1 because of append cursor position
			
		if (nrows < vscroll.Adjustment.PageSize) {
			vscroll.Value=0;
			// set adjustment manually instead of using SetRange 
			// because gtk+ complains if low==high in SetRange()
			vscroll.Adjustment.Lower=0;
			vscroll.Adjustment.Upper=nrows; 
			vscroll.Hide();	
		}
		else if ((dataView.Buffer.Size+1)%bpr==0) { 
			vscroll.SetRange(0, nrows);
			vscroll.Show();
		}
		else {
			vscroll.SetRange(0, nrows+1);
			vscroll.Show();
		}
	
	}
	
	///<summary>Handles window resizing</summary>
	private void Resize(int winWidth, int winHeight)
	{
		// find bytes per row...
		int bpr=FindBestBpr(winWidth);
		
		// configure areas
		int s=0;
		int fontHeight=winHeight;
		foreach(Area a in layout.Areas) {
			a.Height=winHeight;
			a.Width=a.CalcWidth(bpr, true);
			a.X=s;
			a.BytesPerRow=bpr;
			s+=a.Width;
			if (a.Drawer.Height<fontHeight) {
				fontHeight=a.Drawer.Height;
			}
		}

		// configure scrollbar
		vscroll.Adjustment.PageSize=(winHeight/fontHeight);
		vscroll.SetIncrements(3, vscroll.Adjustment.PageSize-1);
		
		// decide whether the scrollbar is visible
		// and its range 
		if (bpr==0) {
			// this can cause eternal loop!
			// because hiding the scrollbar changes the
			// area and causes a reconfigure which may
			// show it again, and so on
			//vscroll.Hide();
		}
		else
			SetupScrollbarRange();
		
	}
	
	///<summary>Handle the Configure Event</summary>
	void OnConfigured (object o, ConfigureEventArgs args)
	{
		if (widgetRealized==false)
			return;
		
		Gdk.EventConfigure conf=args.Event;
		
		Resize(conf.Width, conf.Height);
		
		// make sure the current offset is visible
		MakeOffsetVisible(dataView.Offset, ShowType.Start);
	}
	
	///<summary>Handle the Expose Event</summary>
	void OnExposed (object o, ExposeEventArgs args)
	{	
		
		foreach(Area a in layout.Areas) {
			a.Render();
		}
	}
	
	///<summary>Handle the Realized Event</summary>
	void OnRealized (object o, EventArgs args)
	{		
		// Create some default areas
		LayoutManager.RealizeLayout(layout, drawingArea);
		widgetRealized=true;
	
		// now we can configure properly
		Gdk.Rectangle alloc=((Widget)o).Allocation; 
		Resize(alloc.Width, alloc.Height);
	}
	
	///<summary>Handle scrolling</summary>
	void OnScrolled (object o, EventArgs args)
	{
		int bpr=0;
		if (layout.Areas.Count>0)
			bpr=((Area)layout.Areas[0]).BytesPerRow;
			
		long offset=(long)vscroll.Adjustment.Value*bpr;
		
		//System.Console.WriteLine("On Scrolled: {0}", offset);		
		Gdk.Window win=drawingArea.GdkWindow;
		Gdk.Rectangle alloc=drawingArea.Allocation;
		Gdk.Rectangle rect=new Gdk.Rectangle(0, 0,alloc.Width, alloc.Height); 
		win.BeginPaintRect(rect);
		
		foreach(Area a in layout.Areas) {
			a.Scroll(offset);
		}
		
		win.EndPaint();
		
		//System.Threading.Thread.Sleep(20);
	}
	
	///<summary>Scroll the view so that offset is visible</summary>
	public void MakeOffsetVisible(long offset, ShowType type)
	{
		if (layout.Areas.Count<=0)
			return;
		
		int	bpr=((Area)layout.Areas[0]).BytesPerRow;
		if (bpr==0)
			return ;
		
		long curOffset=((Area)layout.Areas[0]).Offset;
		int h=((Area)layout.Areas[0]).Height;
		Drawer font=((Area)layout.Areas[0]).Drawer;
		int nrows=h/font.Height;
		
		long curOffsetRow=curOffset/bpr;
		long curOffsetEndRow=curOffsetRow+nrows-1;
		long offsetRow=offset/bpr;
		
		//System.Console.WriteLine("curOffRow: {0} curOffEndRow: {1} offRow: {2}",
		//						curOffsetRow, curOffsetEndRow, offsetRow);
		
		if (type == ShowType.Closest) {
			// if already visible do nothing
			if (offsetRow >= curOffsetRow && offsetRow <= curOffsetEndRow)
				;
			else if (curOffsetRow > offsetRow)
				type = ShowType.Start;
			else if (curOffsetEndRow < offsetRow)
				type = ShowType.End;
		}
		
		// Make sure scrollbar range is updated.
		// We need to call this here because
		// a buffer change does not immediately
		// update the range (eg callback goes through GLib.Idle)
		// and a call to MakeOffsetVisible before
		// the update will not behave correctly.
		// eg see DataView.Paste() 
		SetupScrollbarRange();
		
		if (type == ShowType.Cursor) {
			long cursorRow=(layout.Areas[0] as Area).CursorOffset/bpr;
			int diff=(int)(cursorRow-curOffsetRow);
			
			if (diff <= nrows && diff >=0)
				vscroll.Value = offsetRow - diff; 
			// else if diff is outside of a full screen range...
			else if (diff > nrows)
				type = ShowType.End;
			else if (diff < 0)
				type = ShowType.Start;
		}
		
		if (type == ShowType.Start) {
			vscroll.Value=offsetRow;
		}
		else if (type == ShowType.End) {
			if (offsetRow-nrows>=0)
				vscroll.Value=offsetRow-nrows+1;
			else
				vscroll.Value=0;
		}
	}
	
	///<summary>
	/// Show a warning that the file has been changed
	/// outside of the respective DataView 
	///</summary>
	public void ShowFileChangedBar()
	{
		if (fileChangedBar==null) {
			fileChangedBar=new FileChangedBar(this.View);
		}
		
		this.PackStart(fileChangedBar, false, false, 0);
		this.ReorderChild(fileChangedBar, 0);
		fileChangedBar.ShowAll();
	}
	
	public void DisposePixmaps()
	{
		LayoutManager.DisposeLayout(layout);
	}
	
	public void GrabKeyboardFocus()
	{
		if (!drawingArea.HasFocus)
			drawingArea.GrabFocus();
	}
	

	
	private void ConnectToControl()
	{
		if (dvControl==null)
			return;
			
		drawingArea.ButtonPressEvent+=dvControl.OnButtonPress;
		drawingArea.ButtonReleaseEvent+=dvControl.OnButtonRelease;
		drawingArea.MotionNotifyEvent+=dvControl.OnMotionNotify;
		drawingArea.KeyPressEvent+=dvControl.OnKeyPress;
		drawingArea.KeyReleaseEvent+=dvControl.OnKeyRelease;
		drawingArea.ScrollEvent+=dvControl.OnMouseWheel;	
		drawingArea.FocusInEvent+=dvControl.OnFocusInEvent;
		drawingArea.FocusOutEvent+=dvControl.OnFocusOutEvent;
	}

	private void DisconnectFromControl()
	{
	// disconnect previous control
		if (dvControl==null)
			return;
		drawingArea.ButtonPressEvent-=dvControl.OnButtonPress;
		drawingArea.ButtonReleaseEvent-=dvControl.OnButtonRelease;
		drawingArea.MotionNotifyEvent-=dvControl.OnMotionNotify;
		drawingArea.KeyPressEvent-=dvControl.OnKeyPress;
		drawingArea.KeyReleaseEvent-=dvControl.OnKeyRelease;
		drawingArea.ScrollEvent-=dvControl.OnMouseWheel;
		drawingArea.FocusInEvent-=dvControl.OnFocusInEvent;
		drawingArea.FocusOutEvent-=dvControl.OnFocusOutEvent;
	}
	
}// end DataView

}//namespace
