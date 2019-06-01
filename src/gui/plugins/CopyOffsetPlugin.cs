/*
 *   Copyright (c) 2019, Alexandros Frantzis (alf82 [at] freemail [dot] gr)
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
using Bless.Plugins;
using Bless.Tools;
using Bless.Util;
using Mono.Unix;

namespace Bless.Gui.Plugins {

public class CopyOffsetPlugin : GuiPlugin
{
	Gtk.Action CopyOffsetAction;

	const string uiXml =
		"<menubar>" +
		"	<menu action=\"Edit\">" +
		"		<placeholder name=\"Extra\">" +
		"			<menuitem name=\"CopyOffset\" action=\"CopyOffsetAction\" />" +
		"		</placeholder>" +
		"	</menu>" +
		"</menubar>" +
		"<popup name=\"DefaultAreaPopup\">" +
		"	<placeholder name=\"ExtraAreaPopupItems\" >" +
		"		<menuitem name=\"CopyOffset\" action=\"CopyOffsetAction\" />" +
		"	</placeholder>" +
		"</popup>";

	DataBook dataBook;
	Window mainWindow;
	UIManager uiManager;
	IPluginPreferences pluginPreferences;
	int number_base;

	static TargetEntry[] clipboardTargets = new TargetEntry[] {
												new TargetEntry("UTF8_STRING", 0, 0)
											};
	Gtk.Clipboard clipboard;

	long saved_offset;
	Bless.Util.Range saved_selection;

	public CopyOffsetPlugin(Window mw, UIManager uim)
	{
		mainWindow = mw;
		uiManager = uim;
		pluginPreferences = new CopyOffsetPreferences();
		number_base = 10;
		saved_offset = 0;
		saved_selection = new Bless.Util.Range();

		name = "CopyOffset";
		author = "Alexandros Frantzis";
		description = "Adds to copy the current offset (or range)";
		loadAfter.Add("EditOperations");
	}

	public override bool Load()
	{
		clipboard = Gtk.Clipboard.Get(Gdk.Atom.Intern("CLIPBOARD", true));
		dataBook = (DataBook)GetDataBook(mainWindow);

		Preferences.Proxy.Subscribe("CopyOffset.NumberBase", "co",
									new PreferencesChangedHandler(OnPreferencesChanged));

		AddActions(uiManager);

		loaded = true;
		return true;
	}

	private void AddActions(UIManager uim)
	{
		ActionEntry[] actionEntries = new ActionEntry[] {
			new ActionEntry("CopyOffsetAction", Stock.Copy, Catalog.GetString("Copy Offset(s)"),
							 "<Shift><Ctrl>C", null, new EventHandler(OnCopyOffsetActivated))
			};

		ActionGroup group = new ActionGroup("CopyOffsetActions");
		group.Add (actionEntries);

		uim.InsertActionGroup(group, 0);
		uim.AddUiFromString(uiXml);

		uim.EnsureUpdate();
	}

	private void OnCopyOffsetActivated(object o, EventArgs args)
	{
		if (dataBook.NPages == 0)
			return;
		DataView dv = ((DataViewDisplay)dataBook.CurrentPageWidget).View;
		saved_offset = dv.CursorOffset;
		saved_selection = dv.Selection;

		clipboard.SetWithData(clipboardTargets, new ClipboardGetFunc(OnClipboardGet),
							  new ClipboardClearFunc(OnClipboardClear));
	}

	private void OnClipboardGet(Clipboard cb, SelectionData sd, uint n)
	{
		if (saved_selection.IsEmpty()) {
			sd.Text = BaseConverter.ConvertToString(saved_offset, number_base, true, true, 1);
		} else {
			string start = BaseConverter.ConvertToString(saved_selection.Start, number_base, true, true, 1);
			string end = BaseConverter.ConvertToString(saved_selection.End, number_base, true, true, 1);
			sd.Text = string.Format("{0},{1}", start, end);
		}
	}

	private void OnClipboardClear(Clipboard cb)
	{
	}

	public override IPluginPreferences PluginPreferences {
		get { return pluginPreferences; }
	}

	void OnPreferencesChanged(Preferences prefs)
	{
		try {
			number_base = (int) BaseConverter.ConvertToNum(prefs["CopyOffset.NumberBase"], 10);
		} catch {
			number_base = 10;
		}

		if (number_base != 2 && number_base != 8 && number_base != 10 && number_base != 16)
			number_base = 10;
	}
}

class CopyOffsetPreferences : IPluginPreferences
{
	CopyOffsetPreferencesWidget preferencesWidget;

	public Widget Widget {
		get {
			if (preferencesWidget == null)
				InitWidget();
			return preferencesWidget;
		}
	}

	public void LoadPreferences()
	{
		if (preferencesWidget == null)
			InitWidget();

		string base_str = Preferences.Instance["CopyOffset.NumberBase"];

		try {
			preferencesWidget.ActiveBase = (int) BaseConverter.ConvertToNum(base_str, 10);
		} catch {
			preferencesWidget.ActiveBase = 10;
		}
	}

	public void SavePreferences()
	{
	}

	void InitWidget()
	{
		preferencesWidget = new CopyOffsetPreferencesWidget();
		preferencesWidget.NumberBaseCombo.Changed += OnNumberBaseChanged;
	}

	void OnNumberBaseChanged(object o, EventArgs args)
	{
		Preferences.Instance["CopyOffset.NumberBase"] =
			preferencesWidget.NumberBaseCombo.ActiveText;
	}
}

class CopyOffsetPreferencesWidget : Gtk.VBox
{
	Gtk.ComboBox numberBaseCombo;

	int BaseToActiveIndex(int number_base) {
		switch (number_base) {
			case 2: return 0;
			case 8: return 1;
			case 10: return 2;
			case 16: return 3;
			default: throw new Exception();
		}
	}

	public Gtk.ComboBox NumberBaseCombo {
		get { return numberBaseCombo; }
	}

	public int ActiveBase
	{
		set {
			numberBaseCombo.Active = BaseToActiveIndex(value);
		}
	}

	public CopyOffsetPreferencesWidget()
	{
		// Use a hbox inside an vbox to avoid expanding vertically.
		Gtk.HBox hbox = new Gtk.HBox();
		Gtk.Label label = new Gtk.Label(Catalog.GetString("Number base:"));
		numberBaseCombo = Gtk.ComboBox.NewText();
		numberBaseCombo.AppendText("2");
		numberBaseCombo.AppendText("8");
		numberBaseCombo.AppendText("10");
		numberBaseCombo.AppendText("16");

		hbox.PackStart(label, false, false, 6);
		hbox.PackStart(numberBaseCombo, false, false, 6);

		this.PackStart(hbox, true, false, 6);

		this.ShowAll();
	}
}

} //end namespace
