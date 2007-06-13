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

using System;
using System.Text;
using Gtk;
using Bless.Buffers;
using Bless.Plugins;
using Bless.Gui;
using Bless.Tools;

namespace Bless.Gui.Plugins {

public class HistoryPlugin : GuiPlugin
{
	UIManager uiManager;
	uint mergeId;
	ActionGroup historyActionGroup;

	public HistoryPlugin(Window mw, UIManager uim)
	{
		uiManager = uim;

		name = "HistoryPlugin";
		author = "Alexandros Frantzis";
		description = "Provides access to recent files";
		loadAfter.Add("FileOperations");
	}

	public override bool Load()
	{
		History.Instance.Changed += OnHistoryChanged;
		historyActionGroup = new ActionGroup ("HistoryActions");
		uiManager.InsertActionGroup(historyActionGroup, 0);
		OnHistoryChanged(History.Instance);

		loaded = true;
		return true;
	}

	///<summary>Handle additions to history</summary>
	void OnHistoryChanged(History h)
	{
		// clear previous list
		uiManager.RemoveUi(mergeId);
		uiManager.RemoveActionGroup(historyActionGroup);
		foreach(Action action in historyActionGroup.ListActions()) {
			historyActionGroup.Remove(action);
		}

		StringBuilder sb = new StringBuilder("<menubar><menu action=\"File\"><placeholder name=\"HistoryItems\">");

		ActionEntry[] actionEntries = new ActionEntry[h.Files.Count];

		// add recent files
		int i = 1;
		foreach (string file in h.Files) {
			string actionStr = string.Format("{0}RecentFileAction", i);
			string menuItemStr = string.Format("_{0}. {1}", i, System.IO.Path.GetFileName(file));
			string uiMenuStr = string.Format("<menuitem name=\"RecentFile{0}\" action=\"{1}\" />", i, actionStr);

			sb.Append(uiMenuStr);
			actionEntries[i-1] = new ActionEntry (actionStr, null, menuItemStr, null, null,
												  new EventHandler(OnHistoryMenuItemActivated));
			i++;
		}

		sb.Append("</placeholder></menu></menubar>");

		historyActionGroup.Add (actionEntries);
		uiManager.InsertActionGroup(historyActionGroup, 0);

		mergeId = uiManager.AddUiFromString(sb.ToString());
		uiManager.EnsureUpdate();
	}

	///<summary>Handle Activated event on a HistoryMenuItem</summary>
	void OnHistoryMenuItemActivated(object o, EventArgs args)
	{
		Gtk.Action action = (Gtk.Action)o;
		int i = Convert.ToInt32(action.Name.Substring(0, 1));
		string filePath = History.Instance.Files[i-1];

		Services.File.LoadFiles(new string[]{filePath});
	}

}

} // end namespace
