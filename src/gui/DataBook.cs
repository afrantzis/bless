// created on 12/14/2004 at 4:23 PM
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
using Gtk;
using Gdk;
using System;
using System.IO;
using Bless.Buffers;

namespace Bless.Gui {

///<summary>A delegate to call when a databook is closed</summary>
public delegate void CloseViewDelegate(DataView dv);

///<summary>A notebook containing DataViews</summary>
public class DataBook : Gtk.Notebook
{
 
	public DataBook()
	{
		this.Scrollable=true;
		this.CanFocus=true;
		this.EnablePopup=false;
	}
	
	///<summary>Append a DataView to the databook.</summary>
	public void AppendView(DataView dv, CloseViewDelegate deleg, string text)
	{
		base.AppendPage(dv.Display, new DataBookTabLabel(dv, deleg, text));
		this.ShowAll();
		
		dv.Buffer.Changed += new ByteBuffer.ChangedHandler(OnBufferContentsChanged);
		dv.BufferChanged += new DataView.DataViewEventHandler(OnBufferChanged);
		
		if (PageAdded!=null)
			PageAdded(dv);
		
		// this must be placed after the page added signal,
		// so that the page change is caught by others (eg EditOperationsPlugin)
		this.CurrentPage=this.NPages-1;
		
		this.FocusChild=dv.Display;
	} 
	
	///<summary>Remove a DataView from databook.</summary>
	public void RemoveView(DataView dv)
	{
		dv.Buffer.Changed -= new ByteBuffer.ChangedHandler(OnBufferContentsChanged);
		dv.BufferChanged -= new DataView.DataViewEventHandler(OnBufferChanged);
		
		this.RemovePage(this.PageNum(dv.Display));
		
		if (PageRemoved!=null)
			PageRemoved(dv);
	} 
 
	///<summary>
	/// Whether a databook page can be replaced by a new one.
	///</summary>
	public bool CanReplacePage(int nPage)
	{
		// if there isn't such a page, it can't be replaced 
		if (nPage >= this.NPages || nPage < 0)
			return false;
	
		DataView dv=((DataViewDisplay)this.GetNthPage(nPage)).View;
	
		ByteBuffer bb=dv.Buffer;
	
        if (bb.HasChanged==false && bb.Size==0 && !bb.HasFile)
			return true;
		else
			return false; 
	}
	
	///<summary>
	/// Sets the sensitivity status of the close button of a tab containing 'w'.
	///</summary>
	public void SetCloseSensitivity(Widget w, bool sensitive)
	{
		DataBookTabLabel dbt=(DataBookTabLabel)this.GetTabLabel(w);
		
		dbt.Button.Sensitive=sensitive;
	}
	
	///<summary>
	/// Sets the text on tab label of the tab containg 'w'.
	///</summary>
	public new void SetTabLabelText(Widget w, string text)
	{
		DataBookTabLabel dbt=(DataBookTabLabel)this.GetTabLabel(w);
		
		dbt.Text=text;
	}
	
	///<summary>
	/// Updates the title of the tab with the specified DataView
	///</summary>
	void UpdateTabLabel(DataView dv)
	{
		ByteBuffer bb=dv.Buffer;
		
		if (bb.HasChanged)
			SetTabLabelText(dv.Display, System.IO.Path.GetFileName(bb.Filename) +"*");
		else
			SetTabLabelText(dv.Display, System.IO.Path.GetFileName(bb.Filename));
	}
	
	protected override bool OnKeyPressEvent(Gdk.EventKey e)
	{
		int page=-1;
		
		// if alt is pressed
		if ((e.State & ModifierType.Mod1Mask) == ModifierType.Mod1Mask) {
			// select a page
			switch(e.Key) {
				case Gdk.Key.Key_1: page=0; break;
				case Gdk.Key.Key_2: page=1; break;
				case Gdk.Key.Key_3: page=2; break;
				case Gdk.Key.Key_4: page=3; break;
				case Gdk.Key.Key_5: page=4; break;
				case Gdk.Key.Key_6: page=5; break;
				case Gdk.Key.Key_7: page=6; break;
				case Gdk.Key.Key_8: page=7; break;
				case Gdk.Key.Key_9: page=8; break;
				case Gdk.Key.Left: page=this.CurrentPage-1; break;
				case Gdk.Key.Right: page=this.CurrentPage+1; break;
				default: break;
			}
		}
		
		// change the current page if the new one is valid
		if (page >= 0 && page < this.NPages)
			this.Page=page;
			
		return true;
	}
	
	protected override bool OnKeyReleaseEvent(Gdk.EventKey k)
	{
		return true;
	}
	
	void OnBufferChanged(DataView dv)
	{
		UpdateTabLabel(dv);
	}
	
	///<summary>Handle ByteBuffer changes</summary>
	void OnBufferContentsChanged(ByteBuffer bb)
	{
		DataView dv=null;
		
		// find DataView that owns bb
		foreach (DataViewDisplay dvtemp in this.Children) {
			if (dvtemp.View.Buffer==bb) {
				dv=dvtemp.View;
				break;	
			}
		}

		UpdateTabLabel(dv);
	}
	
	
	public new event DataView.DataViewEventHandler PageAdded;
	public new event DataView.DataViewEventHandler PageRemoved;
}

///<summary>A widget to display on each tab label</summary>
class DataBookTabLabel : Gtk.HBox 
{
	Gtk.Label label;
	Gtk.Button closeButton;
	DataView dataView;
	CloseViewDelegate doCloseFile;
	
	public string Text {
		get { return label.Text; }
		set { label.Text=value; }
	}
	
	public Gtk.Button Button {
		get { return closeButton; }
	}
			
	public DataBookTabLabel(DataView dv, CloseViewDelegate deleg, string str)
	{
		dataView=dv;
		doCloseFile=deleg;
		
		dataView.NotificationChanged+=OnNotificationChanged;
		
		label=new Gtk.Label(str);
		label.UseMarkup=true;
		
		Gtk.Image img=new Gtk.Image(Gtk.Stock.Close, Gtk.IconSize.Menu);
		img.SetSizeRequest(8, 8);
		
		// This doesn't compile in 1.0.2 and older,
		// keep it for later eg gtk# 2.0
		// closeButton=new Gtk.Button(img);
		closeButton=new Gtk.Button();
		closeButton.Add(img);
		
		closeButton.Relief=Gtk.ReliefStyle.None;
		closeButton.Clicked+=OnCloseClicked;
		closeButton.CanFocus=false;
		
		this.Spacing=2;
		this.PackStart(label, false, false, 0);
		this.PackStart(closeButton, false, false, 0);
		
		this.ShowAll();
	}
	
	private void OnNotificationChanged(DataView dv)
	{
		if (dv.Notification==true) {
			label.Markup="<span foreground=\"blue\" underline=\"single\">"+label.Text+"</span>";
		}
		else {
			label.Markup=label.Text;
		}
	}
	
	private void OnCloseClicked(object o, EventArgs args)
	{
		doCloseFile(dataView);
	}
	
}  
 
}
