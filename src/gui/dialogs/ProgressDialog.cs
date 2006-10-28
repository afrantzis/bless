// created on 3/20/2005 at 2:47 PM
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
using Glade;
using Bless.Tools.Find;
using Bless.Util;
 
namespace Bless.Gui.Dialogs { 

///<summary>
/// A customizable progress dialog
///</summary>
public class ProgressDialog : Gtk.Window {
	
	[Glade.Widget]Gtk.VBox ProgressVBox;
	[Glade.Widget]Gtk.Button CancelButton;
	[Glade.Widget]Gtk.ProgressBar ProgressBar;
	[Glade.Widget]Gtk.Label MessageLabel;
	[Glade.Widget]Gtk.Label DetailsLabel;
	
	bool cancelClicked;
	
	public ProgressDialog(string primary, Gtk.Window main) : base(primary)
	{
		Glade.XML gxml = new Glade.XML (FileResourcePath.GetSystemPath("..","data","bless.glade"), "ProgressVBox", null);
		gxml.Autoconnect (this);
		
		// setup window
		this.SkipTaskbarHint=false;
		this.TypeHint=Gdk.WindowTypeHint.Dialog;
		this.TransientFor=main;
		this.BorderWidth=6;
		
		MessageLabel.Markup="<span weight=\"bold\" size=\"larger\">"+primary+"</span>";
		
		this.Add(ProgressVBox);
		this.Hide();
	}
	
	public bool Update(object o, ProgressAction action)
	{
		if (action==ProgressAction.Hide) {
			this.Visible=false;
			return false; 
		}
		else if (action==ProgressAction.Show) {
			this.Visible=true;
			return false;
		}
		else if (action==ProgressAction.Message) {
			this.Title=(string)o;
			MessageLabel.Markup="<span weight=\"bold\" size=\"larger\">"+(string)o+"</span>";
			return false;
		}
		else if (action==ProgressAction.Details) {
			DetailsLabel.Text=(string)o;
			return false;
		}
		else if (action==ProgressAction.Destroy) {
			this.Destroy();
			return false;
		}
		
		ProgressBar.Fraction=(double)o;
		
      	if (cancelClicked==true) {
      		cancelClicked=false;
      		return true;
      	}
      	else
      		return false;
	}	
		
	public void OnCancelButtonClicked(object o, EventArgs args)
	{
		cancelClicked=true;
	}
	
	protected override bool OnDeleteEvent(Gdk.Event e)
	{
		cancelClicked=true;
		return true;
	}
}


}// end namespace
