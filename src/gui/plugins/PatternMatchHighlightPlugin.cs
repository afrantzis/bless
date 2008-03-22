// created on 20/3/2008 at 9:02 PM
/*
 *   Copyright (c) 2008, Alexandros Frantzis (alf82 [at] freemail [dot] gr)
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
using Bless.Util;
using Bless.Tools.Find;
using Bless.Tools;
using Bless.Gui;
using Bless.Buffers;
using Bless.Plugins;
using Bless.Gui.Areas;
using Bless.Gui.Drawers;

namespace Bless.Gui.Plugins {

public class PatternMatchHighlightPlugin : GuiPlugin
{
	DataBook dataBook;
	PatternHighlighter patternHighlighter;
	Window mainWindow;
	ToggleAction conversionTableAction;
	UIManager uiManager;

	public PatternMatchHighlightPlugin(Window mw, UIManager uim)
	{
		mainWindow = mw;
		uiManager = uim;

		name = "PatternMatchHighlight";
		author = "Alexandros Frantzis";
		description = "Highlights all matches of the current selection";
	}

	public override bool Load()
	{
		dataBook = (DataBook)GetDataBook(mainWindow);

		patternHighlighter = new PatternHighlighter(dataBook);

		Preferences.Proxy.Subscribe("Highlight.PatternMatch", "ph", new PreferencesChangedHandler(OnPreferencesChanged));

		loaded = true;
		return true;
	}

	void OnPreferencesChanged(Preferences prefs)
	{
		if (prefs["Highlight.PatternMatch"] == "True")
			patternHighlighter.Active = true;
		else
			patternHighlighter.Active = false;
	}
}


class PatternHighlighter
{
	DataBook dataBook;
	bool active;
	IFindStrategy findStrategy;
	
	public bool Active {
		get { return active; }
		set { 
			active = value;
			DataViewDisplay dvd = (DataViewDisplay)dataBook.CurrentPageWidget;
			dvd.Redraw();
		}
	}
	
	public PatternHighlighter(DataBook db)
	{
		dataBook = db;
		findStrategy = new BMFindStrategy();

		foreach(DataViewDisplay dvd in dataBook.Children) {
			OnDataViewAdded(dvd.View);
		}

		dataBook.PageAdded += new DataView.DataViewEventHandler(OnDataViewAdded);
		dataBook.Removed += new RemovedHandler(OnDataViewRemoved);
	}

	void OnDataViewAdded(DataView dv)
	{
		dv.Display.Layout.AreaGroup.PreRenderEvent +=  new AreaGroup.PreRenderHandler(BeforeRender);
	}

	void OnDataViewRemoved(object o, RemovedArgs args)
	{
		DataViewDisplay dvd = (DataViewDisplay)args.Widget;
		
		dvd.Layout.AreaGroup.PreRenderEvent -=  new AreaGroup.PreRenderHandler(BeforeRender);
	}

	void BeforeRender(AreaGroup ag)
	{
		if (!active)
			return;
			
		Util.Range sel = ag.Selection;
		
		if (sel.IsEmpty())
			return;
		
		int patLen = (int)sel.Size;

		int nrows;
		Util.Range view = ag.GetViewRange(out nrows);

		findStrategy.Buffer = ag.Buffer;
		findStrategy.Position = view.Start;
		findStrategy.Pattern = ag.Buffer.RangeToByteArray(sel);

		Util.Range match;
		
		while ((match = findStrategy.FindNext(view.End)) != null) {
			ag.AddHighlight(match.Start, match.End, Drawer.HighlightType.PatternMatch);
		}
		
	}	
}

} // end namespace
