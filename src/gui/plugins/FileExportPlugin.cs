// created on 12/4/2006 at 1:50 AM
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
using Mono.Unix;

namespace Bless.Gui.Plugins {

public class FileExportPlugin : GuiPlugin
{
	Gtk.Action ExportAction;

	const string uiXml =
		"<menubar>" +
		"	<menu action=\"File\">" +
		"		<placeholder name=\"Extra\">" +
		"			<menuitem name=\"Export\" action=\"ExportAction\"/>" +
		"		</placeholder>" +
		"	</menu>" +
		"</menubar>";

	DataBook dataBook;
	Window mainWindow;
	UIManager uiManager;
	ExportDialog exportDialog;

	public FileExportPlugin(Window mw, UIManager uim)
	{
		mainWindow = mw;
		uiManager = uim;

		name = "FileExport";
		author = "Alexandros Frantzis";
		description = "Export a file to various formats";
		loadAfter.Add("FileOperations");
	}

	public override bool Load()
	{
		dataBook = (DataBook) GetDataBook(mainWindow);

		AddMenuItems(uiManager);

		dataBook.PageRemoved += new DataView.DataViewEventHandler(OnDataViewRemoved);
		dataBook.SwitchPage += new SwitchPageHandler(OnSwitchPage);

		exportDialog = new ExportDialog(dataBook, mainWindow);

		loaded = true;
		return true;
	}

	void OnDataViewRemoved(DataView dv)
	{
		// if there are no pages left update the menu
		if (dataBook.NPages == 0)
			UpdateActions(null);
	}

	void OnSwitchPage(object o, SwitchPageArgs args)
	{
		DataView dv = ((DataViewDisplay)dataBook.GetNthPage((int)args.PageNum)).View;

		UpdateActions(dv);
	}

	private void AddMenuItems(UIManager uim)
	{
		ActionEntry[] actionEntries = new ActionEntry[] {
										  new ActionEntry ("ExportAction", null, Catalog.GetString("_Export..."), null, null,
														   new EventHandler(OnExportActivated)),
									  };

		ActionGroup group = new ActionGroup ("ExportActions");
		group.Add (actionEntries);

		uim.InsertActionGroup(group, 0);
		uim.AddUiFromString(uiXml);

		ExportAction = uim.GetAction("/menubar/File/Extra/Export");

		uim.EnsureUpdate();

	}

	///<summary>Handle file->export command from menu</summary>
	public void OnExportActivated(object o, EventArgs args)
	{
		exportDialog.Show();
	}

	void UpdateActions(DataView dv)
	{
		if (dv == null)
			ExportAction.Sensitive = false;
		else
			ExportAction.Sensitive = true;
	}
}



} //end namespace
