// created on 20/25/2006 at 3:24 PM
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
using System.Collections;
using Gtk;
using Cairo;
using Bless.Tools;
using Bless.Util;
using Bless.Gui;
using Bless.Gui.Dialogs;
using Bless.Buffers;
using Bless.Plugins;

namespace Bless.Gui.Plugins {

public class StatisticsPlugin : GuiPlugin
{
	DataBook dataBook;
	Window mainWindow;
	ToggleAction statisticsAction;
	UIManager uiManager;
	StatisticsWidget sw;

	const string uiXml =
		"<menubar>" +
		"	<menu action=\"Tools\">" +
		"		<menuitem name=\"Statistics\" action=\"StatisticsAction\" />" +
		"	</menu>" +
		"</menubar>";

	public StatisticsPlugin(Window mw, UIManager uim)
	{
		mainWindow = mw;
		uiManager = uim;

		name = "File Statistics";
		author = "Alexandros Frantzis";
		description = "File statistics";
	}

	public override bool Load()
	{
		dataBook = (DataBook)GetDataBook(mainWindow);
		WidgetGroup wg = (WidgetGroup)GetWidgetGroup(mainWindow, 1);
		sw = new StatisticsWidget(dataBook);

		wg.Add(sw);

		AddMenuItems(uiManager);

		Preferences.Proxy.Subscribe("Tools.Statistics.Show", "stats1", new PreferencesChangedHandler(OnPreferencesChanged));

		loaded = true;
		return true;
	}



	private void AddMenuItems(UIManager uim)
	{
		ToggleActionEntry[] toggleActionEntries = new ToggleActionEntry[] {
					new ToggleActionEntry ("StatisticsAction", null, "File Statistics", null, null,
										   new EventHandler(OnToolsStatisticsActivated), false),
				};

		ActionGroup group = new ActionGroup ("StatisticsActions");
		group.Add (toggleActionEntries);

		uim.InsertActionGroup(group, 0);
		uim.AddUiFromString(uiXml);
		statisticsAction = (ToggleAction)uim.GetAction("/menubar/Tools/Statistics");

		uim.EnsureUpdate();
	}

	///<summary>Handle the View->Conversion Table command</summary>
	public void OnToolsStatisticsActivated(object o, EventArgs args)
	{
		Preferences.Proxy.Change("Tools.Statistics.Show", statisticsAction.Active.ToString(), "stats1");
	}

	void OnPreferencesChanged(Preferences prefs)
	{
		Console.WriteLine("Prefs changed stat1");
		if (prefs["Tools.Statistics.Show"] == "True")
			statisticsAction.Active = true;
		else
			statisticsAction.Active = false;
	}
}

public class StatisticsInfo
{
	public int[] Freqs;
	public bool Changed;

	public StatisticsInfo()
	{
		Freqs = new int[256];
		Changed = true;
	}
}

public class StatisticsWidget : Gtk.HBox
{
	StatisticsDrawWidget  sdw;
	DataBook dataBook;
	Hashtable info;

	public StatisticsWidget(DataBook db)
	{
		info = new Hashtable();
		dataBook = db;
		dataBook.SwitchPage += new SwitchPageHandler(OnSwitchPage);
		dataBook.PageAdded += new DataView.DataViewEventHandler(OnDataViewAdded);
		dataBook.Removed += new RemovedHandler(OnDataViewRemoved);

		foreach(DataViewDisplay dvd in dataBook.Children) {
			dvd.View.Buffer.Changed += new ByteBuffer.ChangedHandler(OnBufferContentsChanged);
			dvd.View.BufferChanged += new DataView.DataViewEventHandler(OnBufferChanged);
			info[dvd.View] = new StatisticsInfo();
		}

		Preferences.Proxy.Subscribe("Tools.Statistics.Show", "stats2", new PreferencesChangedHandler(OnPreferencesChanged));

		sdw = new StatisticsDrawWidget();
		this.Add(sdw);
		this.ShowAll();
	}

	void OnDialogResponse(object o, Gtk.ResponseArgs args)
	{
		dataBook.SwitchPage -= new SwitchPageHandler(OnSwitchPage);
		dataBook.PageAdded -= new DataView.DataViewEventHandler(OnDataViewAdded);
		dataBook.Removed -= new RemovedHandler(OnDataViewRemoved);

		foreach(DataViewDisplay dvd in dataBook.Children) {
			dvd.View.Buffer.Changed -= new ByteBuffer.ChangedHandler(OnBufferContentsChanged);
			dvd.View.BufferChanged -= new DataView.DataViewEventHandler(OnBufferChanged);
		}

		this.Destroy();
	}

	void OnDataViewAdded(DataView dv)
	{
		dv.Buffer.Changed += new ByteBuffer.ChangedHandler(OnBufferContentsChanged);
		dv.BufferChanged += new DataView.DataViewEventHandler(OnBufferChanged);
		info[dv] = new StatisticsInfo();
	}

	void OnDataViewRemoved(object o, RemovedArgs args)
	{
		DataView dv = ((DataViewDisplay)args.Widget).View;
		dv.Buffer.Changed -= new ByteBuffer.ChangedHandler(OnBufferContentsChanged);
		dv.BufferChanged -= new DataView.DataViewEventHandler(OnBufferChanged);
		info.Remove(dv);
	}

	void OnBufferChanged(DataView dv)
	{
		if (info.Contains(dv))
			(info[dv] as StatisticsInfo).Changed = true;
		else {
			StatisticsInfo si = new StatisticsInfo();
			info[dv] = si;
		}

		UpdateStatistics(dv);
	}

	void OnBufferContentsChanged(ByteBuffer bb)
	{
		DataView dv = null;

		// find DataView that owns bb
		foreach (DataViewDisplay dvtemp in dataBook.Children) {
			if (dvtemp.View.Buffer == bb) {
				dv = dvtemp.View;
				break;
			}
		}
		if (dv == null)
			return;

		OnBufferChanged(dv);
	}

	void OnSwitchPage(object o, SwitchPageArgs args)
	{
		DataView dv = ((DataViewDisplay)dataBook.GetNthPage((int)args.PageNum)).View;
		if (dv == null)
			return;
		if (!info.Contains(dv))
			info[dv] = new StatisticsInfo();

		UpdateStatistics(dv);
	}

	void UpdateStatistics(DataView dv)
	{
		DataView curdv = ((DataViewDisplay)dataBook.CurrentPageWidget).View;
		if (curdv != dv)
			return;
		StatisticsInfo si = (info[dv] as StatisticsInfo);
		if (si.Changed == true) {
			ByteBuffer bb = dv.Buffer;

			for (int i = 0;i < si.Freqs.Length;i++) {
				si.Freqs[i] = 0;
			}

			for (int i = 0;i < bb.Size;i++) {
				++si.Freqs[bb[i]];
			}

			si.Changed = false;
		}

		sdw.Update(si.Freqs);
	}

	protected override void  OnHidden()
	{
		Console.WriteLine("Stats Hide");
		Preferences.Proxy.Change("Tools.Statistics.Show", "False", "stats2");
		base.OnHidden();
	}

	protected override void OnShown()
	{
		Preferences.Proxy.Change("Tools.Statistics.Show", "True", "stats2");
		base.OnShown();
	}

	void OnPreferencesChanged(Preferences prefs)
	{
		Console.WriteLine("Prefch STats2");

		if (prefs["Tools.Statistics.Show"] == "True")
			this.Visible = true;
		else
			this.Visible = false;
	}
}

///<summary> A widget to convert the data at the current offset to various types</summary>
public class StatisticsDrawWidget: Gtk.DrawingArea
{
	int[] freqs;
	int[] dummyFreqs;

	PointD[] barStart;
	PointD[] barEnd;

	double freqWidth;
	int previousHighlight;
	int currentHighlight;

	const double BARS_X_OFFSET = 0.1;
	const double BARS_X_RHS_SPACE = 0.02;
	const double BAR_WIDTH_FRACTIONAL = 1.0;
	const double BAR_HEIGHT_SCALE_FRACTIONAL = 0.95;

	readonly Color BAR_NORMAL_COLOR = new Color(0.0, 0.0, 1.0);  // blue
	readonly Color BAR_HIGHLIGHT_COLOR = new Color(1.0, 0.0, 0.0);  // red
	readonly Color INTER_BAR_COLOR = new Color(1.0, 1.0, 1.0);  // white

	void DrawBar(Cairo.Context gr, int b)
	{
		if ((b >= 0) && (b <= 255))  // prevent exception if user moves mouse-pointer to right of rightmost bar
		{
			//gr.MoveTo(barStart[b]);
			//gr.LineTo (barEnd[b]);
			//gr.Stroke();

			gr.LineWidth = 0.1 / freqs.Length;
			double width = 1.0 / freqs.Length * BAR_WIDTH_FRACTIONAL;
			double height = (barEnd[b].Y - barStart[b].Y) * BAR_HEIGHT_SCALE_FRACTIONAL;
			gr.Rectangle(barStart[b].X - ((0.5 / freqs.Length) * BAR_WIDTH_FRACTIONAL), barStart[b].Y, 
				width, height);
			gr.FillRule = FillRule.Winding;  // or FillRule.EvenOdd, makes no difference to red stripes issue
			gr.Fill();
		}
	}

	void UpdateHighlight()
	{
		Gdk.Window win = this.GdkWindow;

		Cairo.Context g = Gdk.CairoHelper.Create(win);

		int x, y, w, h, d;
		win.GetGeometry(out x, out y, out w, out h, out d);

		g.Scale (w, h);
		//g.LineWidth = (1.0 / freqs.Length) * BAR_WIDTH_FRACTIONAL;

		if (previousHighlight != -1) {
			g.SetSourceColor(BAR_NORMAL_COLOR);
			DrawBar(g, previousHighlight);
		}

		// Brute-force fix for red-lines issue: redraw bars either side of previously-highlighted bar
		if (previousHighlight > 0) {
			g.SetSourceColor(BAR_NORMAL_COLOR);
			DrawBar(g, (previousHighlight - 1));
		}
		if (previousHighlight < 255) {
			g.SetSourceColor(BAR_NORMAL_COLOR);
			DrawBar(g, (previousHighlight + 1));
		}

		if (currentHighlight != -1) {
			g.SetSourceColor(BAR_HIGHLIGHT_COLOR);
			DrawBar(g, currentHighlight);
		}

	}

	void Draw (Cairo.Context gr, int width, int height)
	{
		gr.Scale (width, height);
		gr.SetSourceColor(INTER_BAR_COLOR);
		gr.Rectangle(0.0, 0.0, 1.0, 1.0);
		gr.Stroke();
		gr.SetSourceColor(BAR_NORMAL_COLOR);

		//gr.LineWidth = (1.0 / freqs.Length) * BAR_WIDTH_FRACTIONAL;

		for (int i = 0; i < freqs.Length; i++) {
			if (previousHighlight == i)
				gr.SetSourceColor(BAR_HIGHLIGHT_COLOR);
			DrawBar(gr, i);
			if (previousHighlight == i)
				gr.SetSourceColor(BAR_NORMAL_COLOR);
		}

	}

	public StatisticsDrawWidget()
	{
		dummyFreqs = new int[0];
		barStart = new PointD[0];
		barEnd = new PointD[0];
		freqs = dummyFreqs;

		this.AddEvents((int)Gdk.EventMask.PointerMotionMask);
		this.AddEvents((int)Gdk.EventMask.PointerMotionHintMask);

		this.MotionNotifyEvent += OnMotionNotify;
		previousHighlight = -1;
		currentHighlight = -1;
	}

	protected override bool OnExposeEvent (Gdk.EventExpose args)
	{
		Gdk.Window win = args.Window;

		Cairo.Context g = Gdk.CairoHelper.Create(win);

		int x, y, w, h, d;
		win.GetGeometry(out x, out y, out w, out h, out d);
		this.HeightRequest = w / 5;

		Draw (g, w, h);

		return true;
	}

	///<summary>Update all conversion entries</summary>
	public void Update(int[] freqs)
	{
		if (freqs == null)
			this.freqs = dummyFreqs;
		else {
			this.freqs = freqs;
			if (freqs.Length != barStart.Length) {
				barStart = new PointD[freqs.Length];
				barEnd = new PointD[freqs.Length];
			}
		}

		DoDrawingCalculations();
		previousHighlight = -1;
		currentHighlight = -1;
		this.QueueDraw();
	}

	void DoDrawingCalculations()
	{
		freqWidth = ((1.0 - BARS_X_OFFSET - BARS_X_RHS_SPACE) / freqs.Length);

		int max = 0;

		for (int i = 0; i < freqs.Length; i++) {
			if (freqs[i] > max) max = freqs[i];
		}

		for (int i = 0; i < barStart.Length; i++) {
			barStart[i].X = (i * freqWidth) + BARS_X_OFFSET;
			barStart[i].Y = 1.0;
			barEnd[i].X = barStart[i].X;
			barEnd[i].Y = 1.0 - ((double)freqs[i] / max);
		}

	}

	void OnMotionNotify(object o, MotionNotifyEventArgs args)
	{
		Gdk.EventMotion e = args.Event;
		int x, y;
		Gdk.ModifierType state;

		if (e.IsHint)
			this.GdkWindow.GetPointer(out x, out y, out state);
		else {
			x = (int)e.X;
			y = (int)e.Y;
			state = e.State;
		}
		Gdk.Rectangle alloc = this.Allocation;
		//Console.WriteLine("x {0} freq {1} width{2}", x, freqWidth, alloc.Width);
		currentHighlight = (int)((x / (freqWidth * alloc.Width)) - (BARS_X_OFFSET / freqWidth) + 0.5);
		//currentHighlight = (int)(((x / alloc.Width) - BARS_X_OFFSET) / freqWidth) + 1;
		Console.WriteLine(currentHighlight);  // debug output to console, TEMPORARY

		if (previousHighlight != currentHighlight) {
			UpdateHighlight();
			previousHighlight = currentHighlight;
		}
	}
}

} // end namespace
