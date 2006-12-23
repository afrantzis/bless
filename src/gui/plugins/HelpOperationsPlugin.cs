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
using Bless.Gui;
using Bless.Plugins;
using Bless.Gui.Dialogs;
using System;
using Gtk;
using Mono.Unix;

namespace Bless.Gui.Plugins {
	
public class HelpOperationsPlugin : GuiPlugin
{
	const string uiXml=
	"<menubar>"+
	"	<menu action=\"Help\">"+
	"		<menuitem name=\"Contents\" action=\"ContentsAction\" />"+
	"		<menuitem name=\"About\" action=\"AboutAction\" />"+
	"		<separator/>"+
	"	</menu>"+
	"</menubar>";
	
	DataBook dataBook;
	Window mainWindow;
	UIManager uiManager;
	
	public HelpOperationsPlugin(Window mw, UIManager uim)
	{
		//mainWindow=mw;
		uiManager=uim;
		
		name="HelpOperations";
		author="Alexandros Frantzis";
		description="Provides access to basic help operations";
	}
	
	public override bool Load()
	{
		AddMenuItems(uiManager);
		
		loaded=true;
		return true;
	}
	
	private void AddMenuItems(UIManager uim)
	{
		ActionEntry[] actionEntries = new ActionEntry[] {
			new ActionEntry ("ContentsAction", Stock.Help, Catalog.GetString("_Contents"), "F1", null,
			                    new EventHandler(OnContentsActivated)),
			new ActionEntry ("AboutAction", null, Catalog.GetString("_About"), null, null,
			                    new EventHandler(OnAboutActivated)),
		};
		
		ActionGroup group = new ActionGroup ("HelpActions");
		group.Add (actionEntries);
		
		uim.InsertActionGroup(group, 0);
		uim.AddUiFromString(uiXml);
		
		uim.EnsureUpdate();
		
	}
	///<summary>Handle edit->undo command from menu</summary>
	public void OnContentsActivated(object o, EventArgs args) 
	{
#if ENABLE_UNIX_SPECIFIC
		string helpScript=FileResourcePath.GetPath("..", "data", "help_script.sh");
		System.Diagnostics.Process.Start(helpScript);
#endif
	}
	
	///<summary>Handle edit->redo command from menu</summary>
	public void OnAboutActivated(object o, EventArgs args) 
	{
		Bless.Gui.Dialogs.AboutDialog.Show();
	}
		
}

} //end namespace
