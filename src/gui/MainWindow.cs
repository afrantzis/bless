// project created on 6/3/2004 at 8:34 PM
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
using System.Collections;
using System.Threading;
using System.IO;
using Gtk;
using Glade;
using Bless.Buffers;
using Bless.Gui;
using Bless.Gui.Dialogs;
using Bless.Tools.Find;
using Bless.Tools;
using Bless.Util;
using Bless.Plugins;
using Mono.Unix;

public class BlessMain
{
	[Glade.Widget] Gtk.HBox DataViewBox;
	[Glade.Widget] Gtk.Toolbar MainToolbar;
	[Glade.Widget] Gtk.Window MainWindow;
	[Glade.Widget] Gtk.VBox MainVBox;
	
	[Glade.Widget] Gtk.ToolButton NewToolButton;
	[Glade.Widget] Gtk.ToolButton OpenToolButton;
	[Glade.Widget] Gtk.ToolButton SaveToolButton;
	[Glade.Widget] Gtk.ToolButton UndoToolButton;
	[Glade.Widget] Gtk.ToolButton RedoToolButton;
	[Glade.Widget] Gtk.ToolButton CutToolButton;
	[Glade.Widget] Gtk.ToolButton CopyToolButton;
	[Glade.Widget] Gtk.ToolButton PasteToolButton;
	
	const string uiXml=
	"<menubar>"+
	"	<menu action=\"File\" />"+
	"	<menu action=\"Edit\" />"+
	"	<menu action=\"View\">"+
	"		<menuitem name=\"Toolbar\" action=\"ToolbarAction\" />"+
	"	</menu>"+
	"	<menu action=\"Search\" />"+
	"	<menu action=\"Tools\" />"+
	"	<menu action=\"Help\" />"+
	"</menubar>"+
	"<toolbar>"+
	"	<placeholder name=\"FileItems\" />"+
	"	<separator/>"+
	"	<placeholder name=\"EditItems\" />"+
	"	<separator/>"+
	"	<placeholder name=\"SearchItems\" />"+	
	"</toolbar>";
	
	ActionEntry[] actionEntries = new ActionEntry[] {
			new ActionEntry ("File", null, Catalog.GetString("_File"), null, null, null),
			new ActionEntry ("Edit", null, Catalog.GetString("_Edit"), null, null, null),
			new ActionEntry ("View", null, Catalog.GetString("_View"), null, null, null),
			new ActionEntry ("Search", null, Catalog.GetString("_Search"), null, null, null),
			new ActionEntry ("Tools", null, Catalog.GetString("_Tools"), null, null, null),
			new ActionEntry ("Help", null, Catalog.GetString("_Help"), null, null, null)
	};
	
	UIManager uiManager;
	
	Gtk.AccelGroup editAccelGroup; // the group of accelerators for the edit menu
	DataBook dataBook;
	ProgressDialog findProgressDialog;
	
	// the kinds of MIME type targets we are accepting
	static TargetEntry[] dropTargets= new TargetEntry[]{
		new TargetEntry("text/uri-list", 0, 0)
		};
	
	public static void Main (string[] args)
	{
		new BlessMain(args);
	}

	public BlessMain (string[] args) 
	{
		Application.Init();
		
		// 
		Catalog.Init("bless", FileResourcePath.GetSystemPath("locale"));
		
		// load main window from glade XML
		Glade.XML gxml = new Glade.XML (FileResourcePath.GetSystemPath("..","data","bless.glade"), "MainWindow", "bless");
		gxml.Autoconnect (this);
		
		// set the application icon
		MainWindow.Icon=new Gdk.Pixbuf(FileResourcePath.GetSystemPath("..","data","bless-48x48.png"));
		
		string blessConfDir=Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "bless");
		
		// make sure local configuration directory exists
		try {
			if (!Directory.Exists(blessConfDir)) {
				Directory.CreateDirectory(blessConfDir);
			}
		}
		catch (Exception ex){
			ErrorAlert ea=new ErrorAlert(Catalog.GetString("Cannot create user configuration directory"), ex.Message+Catalog.GetString("\n\nSome features of Bless may not work properly."), MainWindow);
			ea.Run();
			ea.Destroy();
		}
		
		Preferences.Proxy.Enable=false;
		// load default preferences
		Preferences.Default.Load(FileResourcePath.GetSystemPath("..","data","default-preferences.xml"));
		Preferences.Default["Default.Layout.File"]=FileResourcePath.GetSystemPath("..","data","bless-default.layout");
		
		// load user preferences
		LoadPreferences(Path.Combine(blessConfDir,"preferences.xml"));
		Preferences.Instance.AutoSavePath=Path.Combine(blessConfDir,"preferences.xml");
		
		// add the (empty) Menubar and toolbar
		uiManager=new UIManager();
		MainWindow.AddAccelGroup(uiManager.AccelGroup);
		uiManager.AddUiFromString(uiXml);
		
		ActionGroup group = new ActionGroup ("MainMenuActions");
		group.Add (actionEntries);
		group.Add ( new ToggleActionEntry[] { 
			new ToggleActionEntry ("ToolbarAction", null, "Toolbar", null, null,
			                    new EventHandler(OnViewToolbarToggled), false)
		});
		
		uiManager.InsertActionGroup(group, 0);
		
		Widget mb=uiManager.GetWidget("/menubar");
		MainVBox.PackStart(mb, false, false, 0);
		MainVBox.ReorderChild(mb, 0);
		Widget tb=uiManager.GetWidget("/toolbar");
		tb.Visible=false;
		MainVBox.PackStart(tb, false, false, 0);
		MainVBox.ReorderChild(tb, 1);
		
		// create the DataBook
		dataBook=new DataBook();
		dataBook.PageAdded += new DataView.DataViewEventHandler(OnDataViewAdded);
		dataBook.Removed += new RemovedHandler(OnDataViewRemoved);
		dataBook.SwitchPage += new SwitchPageHandler(OnSwitchPage);
		
		DataViewBox.PackStart(dataBook);
		
		
		// create the widget groups that hold utility widgets
		WidgetGroup widgetGroup0=new WidgetGroup();
		WidgetGroup widgetGroup1=new WidgetGroup();
		WidgetGroup sideWidgetGroup0=new WidgetGroup();
		WidgetGroup sideWidgetGroup1=new WidgetGroup();
		widgetGroup0.Show();
		widgetGroup1.Show();
		sideWidgetGroup0.Show();
		sideWidgetGroup1.Show();
		
		MainVBox.PackStart(widgetGroup0, false, false, 0);
		MainVBox.ReorderChild(widgetGroup0, 3);
		MainVBox.PackStart(widgetGroup1, false, false, 0);
		MainVBox.ReorderChild(widgetGroup1, 4);
		
		DataViewBox.PackStart(sideWidgetGroup0, false, false, 0);
		DataViewBox.ReorderChild(sideWidgetGroup0, 0);
		DataViewBox.PackEnd(sideWidgetGroup1, false, false, 0);
		//MainVBox.ReorderChild(widgetGroup1, 4);
		
		
		Services.File=new FileService(dataBook, MainWindow);
		Services.Session=new SessionService(dataBook, MainWindow);
		//Services.Info=new InfoService(infobar);
		
		PluginManager guiPlugins=new PluginManager(typeof(GuiPlugin), new object[]{MainWindow, uiManager});
		foreach (Plugin p in guiPlugins.Plugins) {
			guiPlugins.LoadPlugin(p);
			//Console.WriteLine("Loaded Plugin: {0}", p.Name);
		}
		
		// load recent file history
		try {
			History.Instance.Load(Path.Combine(blessConfDir,"history.xml"));
		}
		catch (Exception e) {
			System.Console.WriteLine(e.Message);
		}
		
		// if user specified files on the command line
		// try to load them
		if (args.Length>0) {
			Services.File.LoadFiles(args);
		}
		else if (Preferences.Instance["Session.LoadPrevious"]=="True") {
			bool loadIt=true;
			string prevSessionFile=Path.Combine(blessConfDir,"last.session");
			
			if (Preferences.Instance["Session.AskBeforeLoading"]=="True"
				&& File.Exists(prevSessionFile)) {
				MessageDialog md = new MessageDialog (MainWindow, 
					DialogFlags.DestroyWithParent,
					MessageType.Question, 
					ButtonsType.YesNo, Catalog.GetString("Do you want to load your previous session?"));
     
					ResponseType result = (ResponseType)md.Run ();
					md.Destroy();
					
					if (result==ResponseType.Yes)
						loadIt=true;
					else
						loadIt=false;

			}
			// try to load previous session
			if (loadIt)
				Services.Session.Load(prevSessionFile);
		}
		
		// if nothing has been loaded, create a new file
		if (dataBook.NPages == 0) {
			ByteBuffer bb = Services.File.NewFile();
			
			// create and setup a  DataView
			DataView dv=Services.File.CreateDataView(bb);
			
			// append the DataView to the DataBook
			dataBook.AppendView(dv, new CloseViewDelegate(Services.File.CloseFile), Path.GetFileName(bb.Filename));
		}
		
		PreferencesChangedHandler handler=new PreferencesChangedHandler(OnPreferencesChanged);
		Preferences.Proxy.Subscribe("View.Toolbar.Show", "mainwin", handler);
		
		
		// register drag and drop of files
		MainWindow.DragDataReceived+=OnDragDataReceived;
		Gtk.Drag.DestSet(MainWindow, DestDefaults.Motion|DestDefaults.Drop, dropTargets, Gdk.DragAction.Copy|Gdk.DragAction.Move);
		
		DataViewBox.ShowAll();
		
		Preferences.Proxy.Enable=true;
		// fire the preferences changed event
		// so things are setup according to the preferences
		Preferences.Proxy.NotifyAll();
		
		Application.Run();
	}
	
	///<summary>
	/// Handles drag and drop of files
	///</summary>
	void OnDragDataReceived(object o, DragDataReceivedArgs args)
	{
		string uriStr=System.Text.Encoding.Default.GetString(args.SelectionData.Data);
		
		// get individual uris...
		// according to text/uri-list, uris are separated
		// by "\r\n"
		uriStr=uriStr.Trim();
		uriStr=uriStr.Replace("\r\n", "\n");
		string[] uris=uriStr.Split('\n');
		
		// we are done
		Gtk.Drag.Finish (args.Context, false, false, args.Time);
		
		// load the files
		Services.File.LoadFiles(uris);
	}
	
	 
	///<summary>
	/// Updates the window's title with data from the specified DataView,
	/// if the specified DataView is active
	///</summary>
	void UpdateWindowTitle(DataView dv)
	{
		if (dataBook.NPages == 0)
			return;
		
		DataView curdv=((DataViewDisplay)dataBook.CurrentPageWidget).View;
		ByteBuffer curbb=curdv.Buffer;
		
		ByteBuffer bb=dv.Buffer;
		
		// if DataView is active
		if (curbb==bb) {
			if (bb.HasChanged && !MainWindow.Title.EndsWith("* - Bless"))
				MainWindow.Title= bb.Filename + " * - Bless";
			else if (!bb.HasChanged)
				MainWindow.Title= bb.Filename + " - Bless";
		}
	}
	
	///<summary>
	/// Load the preferences from the specified file
	///</summary>
	void LoadPreferences(string path)
	{
		try {
			Preferences.Instance.Load(Preferences.Default);
			Preferences.Instance.Load(path);
		}
		catch (Exception e) {
			System.Console.WriteLine(e.Message);
		}
	}
	
	void OnPreferencesChanged(Preferences prefs)
	{	
		ToggleAction viewToolbarAction=(ToggleAction)uiManager.GetAction("/menubar/View/Toolbar");
		
		if (prefs["View.Toolbar.Show"]=="True")
			viewToolbarAction.Active=true;
		else
			viewToolbarAction.Active=false;
	}
	
	///<summary>
	/// Callback for SwitchPage DataBook event 
	///</summary>
	void OnSwitchPage(object o, SwitchPageArgs args)
	{
		DataView dv=((DataViewDisplay)dataBook.GetNthPage((int)args.PageNum)).View;
	
		UpdateWindowTitle(dv);
		
		dv.Display.GrabKeyboardFocus();
	}
	
	void OnDataViewAdded(DataView dv)
	{
		dv.Buffer.Changed += new ByteBuffer.ChangedHandler(OnBufferContentsChanged);
		dv.BufferChanged += new DataView.DataViewEventHandler(OnBufferChanged);
	}
	
	void OnDataViewRemoved(object o, RemovedArgs args)
	{
		DataView dv=((DataViewDisplay)args.Widget).View;
		dv.Buffer.Changed -= new ByteBuffer.ChangedHandler(OnBufferContentsChanged);
		dv.BufferChanged -= new DataView.DataViewEventHandler(OnBufferChanged);
	}
	
	void OnBufferChanged(DataView dv)
	{
		UpdateWindowTitle(dv);
	}
	
	///<summary>Handle ByteBuffer changes</summary>
	void OnBufferContentsChanged(ByteBuffer bb)
	{
		DataView dv=null;
		
		// find DataView that owns bb
		foreach (DataViewDisplay dvtemp in dataBook.Children) {
			if (dvtemp.View.Buffer==bb) {
				dv=dvtemp.View;
				break;	
			}
		}
		
		UpdateWindowTitle(dv);
	}
	
	
	///<summary>Handle the View->Toolbar command</summary>
	public void OnViewToolbarToggled(object o, EventArgs args)
	{
		ToggleAction viewToolbarAction=(ToggleAction)uiManager.GetAction("/menubar/View/Toolbar");
		
		Preferences.Proxy.Change("View.Toolbar.Show", viewToolbarAction.Active.ToString(), "mainwin");
		
		Widget mainToolbar = uiManager.GetWidget("/toolbar");
		
		if (viewToolbarAction.Active)
			mainToolbar.Visible=true;
		else
			mainToolbar.Visible=false;
	}
	
	///<summary>Handle the Benchmark Toolbar button command (Hidden)</summary>
	public void OnBenchmark(object o, EventArgs args)
	{
		if (dataBook.NPages > 0) {
			DataViewDisplay dvd=(DataViewDisplay)dataBook.CurrentPageWidget;
			dvd.Benchmark();
		}
	}
	
	///<summary>Handle Main Window Delete Event</summary>
	public void OnMainWindowDeleteEvent (object o, DeleteEventArgs args) 
	{
		// make sure we can quit safely:
		// 1. no file operations are currently going on (eg save, replace etc)
		// 2. user has saved or doesn't want to save their modified files
		MenuItem FileQuitMenuItem=(MenuItem)uiManager.GetWidget("/menubar/File/Quit");
		if (FileQuitMenuItem.Sensitive)
			Services.File.TryQuit();
		args.RetVal = true;
	}

} // end bless main

