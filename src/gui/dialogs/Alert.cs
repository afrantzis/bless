// created on 7/28/2004 at 7:09 PM
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

 
namespace Bless.Gui.Dialogs {

	///<summary>An alert dialog box as recommended in the Gnome HIG</summary>
	abstract public class Alert : Gtk.Dialog
	{
		protected Gtk.HBox hbox;
		protected Gtk.VBox labelBox;
		protected Gtk.Image image;
		protected Gtk.Label labelPrimary;
		protected Gtk.Label labelSecondary;
	
		public Alert(string primary, string secondary, Gtk.Window parent) 
		: base("", parent,Gtk.DialogFlags.DestroyWithParent)
		{
			// set-up alert
			this.Modal=true;
			//this.TypeHint=Gdk.WindowTypeHint.Utility;
			this.BorderWidth=6;
			this.HasSeparator=false;
			this.Resizable=false;
		
			this.VBox.Spacing=12;
		
			hbox=new Gtk.HBox();
			hbox.Spacing=12;
			hbox.BorderWidth=6;
			this.VBox.Add(hbox);
			
			// set-up image
			image=new Gtk.Image();
			image.Yalign=0.0F;
			hbox.Add(image);
			
			// set-up labels
			labelPrimary=new Gtk.Label();
			labelPrimary.Yalign=0.0F;
			labelPrimary.Xalign=0.0F;
			labelPrimary.UseMarkup=true;
			labelPrimary.Wrap=true;
			
			labelSecondary=new Gtk.Label();
			labelSecondary.Yalign=0.0F;
			labelSecondary.Xalign=0.0F;
			labelSecondary.UseMarkup=true;
			labelSecondary.Wrap=true;
			
			labelPrimary.Markup="<span weight=\"bold\" size=\"larger\">"+primary+"</span>";
			labelSecondary.Markup="\n"+secondary;
			
			labelBox=new VBox();
			labelBox.Add(labelPrimary);
			labelBox.Add(labelSecondary);
			
			hbox.Add(labelBox);
		}

	}	



} 