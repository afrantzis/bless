// created on 4/5/2007 at 2:06 PM
/*
 *   Copyright (c) 2007, Alexandros Frantzis (alf82 [at] freemail [dot] gr)
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
using Glade;
using Gtk;
using System;
using Bless.Util;
using Bless.Gui;
using Bless.Plugins;
using Bless.Buffers;
using Mono.Unix;

namespace Bless.Gui.Plugins {

///<summary>
/// Plugin to display progress of various events in a uniform way
///</summary>
public class ProgressDisplayPlugin : GuiPlugin
{
	ProgressDisplayWidget widget;
	Window mainWindow;
	
	
	public ProgressDisplayPlugin(Window mw, UIManager uim)
	{
		mainWindow = mw;
		
		name = "ProgressDisplay";
		author = "Alexandros Frantzis";
		description = "Progress Display Bar";
		loadAfter.Add("Infobar");
	}
	
	public override bool Load()
	{	
		widget = new ProgressDisplayWidget();
		widget.Visible = true;
		
		// register the service
		Services.UI.Progress = widget;
		
		((VBox)mainWindow.Child).PackEnd(widget, false, false, 0);
		
		loaded = true;
		return true;
	}
	
}

///<summary>
/// Widget that displays a series of progress bars
///</summary>
public class ProgressDisplayWidget : Gtk.VBox, IProgressDisplay
{

	public ProgressDisplayWidget()
	{
		
	}
	
	///<summary>
	/// Get a callback for a new progress bar
	///</summary>
	public ProgressCallback NewCallback()
	{
		ProgressDisplayBar pdb = new ProgressDisplayBar();
		
		this.PackStart(pdb);
		pdb.DestroyEvent += OnProgressDisplayBarDestroyed;
		
		return pdb.Update;
	}
	
	void OnProgressDisplayBarDestroyed (object o, DestroyEventArgs args)
	{
		ProgressDisplayBar pdb = (ProgressDisplayBar)o;
		
		pdb.DestroyEvent -= OnProgressDisplayBarDestroyed;
		
		this.Remove(pdb);
	}

}

public class ProgressDisplayBar : Gtk.HBox {
	
	[Glade.Widget]Gtk.HBox ProgressBarHBox;
	[Glade.Widget]Gtk.Button CancelButton;
	[Glade.Widget]Gtk.ProgressBar ProgressBar;
	
	bool cancelClicked;
	
	public ProgressDisplayBar()
	{
		Glade.XML gxml = new Glade.XML (FileResourcePath.GetSystemPath("..","data","bless.glade"), "ProgressBarHBox", "bless");
		gxml.Autoconnect (this);
			
		this.Add(ProgressBarHBox);
		this.Hide();
	}
	
	///<summary>
	/// Handles the various progress actions
	///</summary>
	public bool Update(object o, ProgressAction action)
	{
		if (action == ProgressAction.Hide) {
			this.Visible = false;
			return false; 
		}
		else if (action == ProgressAction.Show) {
			this.Visible = true;
			return false;
		}
		else if (action == ProgressAction.Message) {
			ProgressBar.Text = (string)o;
			return false;
		}
		else if (action == ProgressAction.Destroy) {
			this.Destroy();
			return false;
		}
		
		ProgressBar.Fraction = (double)o;
		
      	if (cancelClicked == true) {
      		cancelClicked = false;
      		return true;
      	}
      	else
      		return false;
	}	
		
	public void OnCancelButtonClicked(object o, EventArgs args)
	{
		cancelClicked = true;
	}
}



} // end namespace
