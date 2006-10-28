// created on 10/24/2006 at 12:59 PM
/*
 *   Copyright (c) 2006, Alexandros Frantzis (alf82 [at] freemail [dot] gr)
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
using Bless.Buffers;
using Bless.Gui;
using Bless.Plugins;
using Bless.Gui.Dialogs;
using Bless.Tools;
using System;
using System.IO;
using Gtk;

namespace Bless.Gui.Plugins {
	
public class SelectLayoutPlugin : GuiPlugin
{
	Gtk.MenuItem LayoutsMenuItem;
		
	const string uiXml=
	"<menubar>"+
	"	<menu action=\"View\">"+
	"		<menuitem name=\"Layouts\" action=\"LayoutsAction\" position=\"top\" />"+
	"		<separator/>"+
	"	</menu>"+
	"</menubar>";
	
	DataBook dataBook;
	Window mainWindow;
	UIManager uiManager;
	
	public SelectLayoutPlugin(Window mw, UIManager uim)
	{
		mainWindow=mw;
		uiManager=uim;
		
		name="SelectLayout";
		author="Alexandros Frantzis";
		description="Change the layout of a view";
	}
	
	public override bool Load()
	{
		dataBook=(DataBook)GetDataBook(mainWindow);
		
		AddMenuItems(uiManager);
		
		loaded=true;
		return true;
	}
	
	private void AddMenuItems(UIManager uim)
	{
		ActionEntry[] actionEntries = new ActionEntry[] {
			new ActionEntry ("LayoutsAction", null, "_Layouts...", "<shift><ctrl>L", null,
			                    new EventHandler(OnLayoutsActivated)),
		};
		
		ActionGroup group = new ActionGroup ("SelectLayoutActions");
		group.Add (actionEntries);
		
		uim.InsertActionGroup(group, 0);
		uim.AddUiFromString(uiXml);
		
		LayoutsMenuItem=(MenuItem)uim.GetWidget("/menubar/View/Layouts");
		
		uim.EnsureUpdate();
		
	}
	
	///<summary>Handle view->layouts command from menu</summary>
	public void OnLayoutsActivated(object o, EventArgs args) 
	{
		LayoutSelectionDialog lsd=new LayoutSelectionDialog(dataBook);
		lsd.Show();
	}
}



} //end namespace
