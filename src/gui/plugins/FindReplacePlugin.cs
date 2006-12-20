// created on 6/15/2005 at 1:29 PM
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
using Bless.Tools.Find;
using Bless.Util;
using Bless.Gui;
using Bless.Gui.Areas;
using Bless.Buffers;
using Bless.Gui.Dialogs;
using System.Text;
using Bless.Plugins;
using Mono.Unix;

namespace Bless.Gui.Plugins {
	
public class FindReplacePlugin : GuiPlugin
{	
	DataBook dataBook;
	FindReplaceWidget widget;
	
	Action FindAction;
	Action FindNextAction;
	Action FindPreviousAction;
	Action ReplaceAction;
	
	IFinder finder;
	Window mainWindow;
	UIManager uiManager;
	
	const string uiXml=
	"<menubar>"+
	"	<menu action=\"Search\">"+
	"		<menuitem name=\"Find\" action=\"FindAction\" />"+
	"		<menuitem name=\"FindNext\" action=\"FindNextAction\" />"+
	"		<menuitem name=\"FindPrevious\" action=\"FindPreviousAction\" />"+
	"		<menuitem name=\"Replace\" action=\"ReplaceAction\" />"+
	"		<separator/>"+
	"	</menu>"+
	"</menubar>"+
	"<toolbar>"+
	"	<placeholder name=\"SearchItems\">"+
	"		<toolitem name=\"Find\" action=\"FindAction\" />"+
	"		<toolitem name=\"Replace\" action=\"ReplaceAction\" />"+
	"	</placeholder>"+
	"</toolbar>";
	
	public FindReplacePlugin(Window mw, UIManager uim)
	{
		mainWindow = mw;
		uiManager=uim;
		
		name="FindReplace";
		author="Alexandros Frantzis";
		description="Adds a firefox like find/replace bar";
	}
	
	public override bool Load()
	{
		dataBook=(DataBook)GetDataBook(mainWindow);
		
		// create the finder object
		ProgressDialog findProgressDialog=new ProgressDialog("", mainWindow);
		finder=new DataBookFinder(dataBook, findProgressDialog.Update);
		finder.Strategy=new BMFindStrategy();
		finder.FirstFind+= OnFirstFind;
		
		// create the FindReplaceWidget (hidden)
		widget=new FindReplaceWidget(dataBook, finder);
		widget.Visible=false;
		
		WidgetGroup wgroup=(WidgetGroup)GetWidgetGroup(mainWindow, 0);
		wgroup.Add(widget);
		
		// add the menu items
		AddActions(uiManager);
		
		dataBook.PageAdded += new DataView.DataViewEventHandler(OnDataViewAdded);
		dataBook.PageRemoved += new DataView.DataViewEventHandler(OnDataViewRemoved);
		dataBook.SwitchPage += new SwitchPageHandler(OnSwitchPage);

		loaded=true;
		return true;
	}
	
	void OnDataViewAdded(DataView dv)
	{
		dv.Buffer.Changed += new ByteBuffer.ChangedHandler(OnBufferContentsChanged);
		dv.BufferChanged += new DataView.DataViewEventHandler(OnBufferChanged);
	}
	
	void OnDataViewRemoved(DataView dv)
	{
		dv.Buffer.Changed -= new ByteBuffer.ChangedHandler(OnBufferContentsChanged);
		dv.BufferChanged -= new DataView.DataViewEventHandler(OnBufferChanged);
		// if there are no pages left update the menu
		if (dataBook.NPages==0)
			UpdateActions(null);
	}
	
	void OnBufferChanged(DataView dv)
	{
		UpdateActions(dv);
	}
	
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
		
		UpdateActions(dv);
	}
	
	void OnSwitchPage(object o, SwitchPageArgs args)
	{
		DataView dv=((DataViewDisplay)dataBook.GetNthPage((int)args.PageNum)).View;
		
		UpdateActions(dv);
	}
	private void AddActions(UIManager uim)
	{
		ActionEntry[] actionEntries = new ActionEntry[] {
			new ActionEntry ("FindAction", Stock.Find, null, "<control>F", "Find",
			                    new EventHandler(OnFindActivated)),
			new ActionEntry ("FindNextAction", null, Catalog.GetString("Find _Next"), "F3", null,
			                    new EventHandler(OnFindNextActivated)),
			new ActionEntry ("FindPreviousAction", null, Catalog.GetString("Find _Previous"), "<shift>F3", null,
			                    new EventHandler(OnFindPreviousActivated)),
			new ActionEntry ("ReplaceAction", Stock.FindAndReplace, null, "<control>R", "Replace",
			                    new EventHandler(OnReplaceActivated))
		};
		
		ActionGroup group = new ActionGroup ("SearchActions");
		group.Add (actionEntries);
		
		uim.InsertActionGroup(group, 0);
		uim.AddUiFromString(uiXml);
		
		FindAction=(Action)uim.GetAction("/menubar/Search/Find");
		FindNextAction=(Action)uim.GetAction("/menubar/Search/FindNext");
		FindPreviousAction=(Action)uim.GetAction("/menubar/Search/FindPrevious");
		ReplaceAction=(Action)uim.GetAction("/menubar/Search/Replace");
		
		uim.EnsureUpdate();
		
	}
	
	private void OnFindActivated(object o, EventArgs args)
	{
		// get pattern from current selection...
		if (dataBook.NPages > 0) {
			DataView dv=((DataViewDisplay)dataBook.CurrentPageWidget).View;
			widget.LoadWithSelection(dv);
		}
		
		widget.SearchVisible=true;
		widget.ReplaceVisible=false;
		
		widget.Show();
	}
	
	private void OnReplaceActivated(object o, EventArgs args)
	{
		// get pattern from current selection...
		if (dataBook.NPages > 0) {
			DataView dv=((DataViewDisplay)dataBook.CurrentPageWidget).View;
			widget.LoadWithSelection(dv);
		}
		
		widget.SearchVisible=true;
		widget.ReplaceVisible=true;
		
		widget.Show();
	}
	
	///<summary>Handle the Search->Find Next command</summary>
	public void OnFindNextActivated(object o, EventArgs args)
	{
		finder.FindNext(FindNextAsyncCallback);
	}
	
	void FindNextAsyncCallback(IAsyncResult ar)
	{
		ThreadedAsyncResult tar=(ThreadedAsyncResult) ar;
		
		FindNextOperation state=(FindNextOperation)tar.AsyncState;
		if (state.Result==FindNextOperation.OperationResult.Finished && state.Match==null) {
			InformationAlert ia=new InformationAlert(Catalog.GetString("The pattern you requested was not found."), Catalog.GetString("End of file reached."), mainWindow);
			ia.Run();
			ia.Destroy();
		}
	}
	
	///<summary>Handle the Search->Find Previous command</summary>
	public void OnFindPreviousActivated(object o, EventArgs args)
	{
		finder.FindPrevious(FindPreviousAsyncCallback);
	}
	
	void FindPreviousAsyncCallback(IAsyncResult ar)
	{
		ThreadedAsyncResult tar=(ThreadedAsyncResult) ar;
		
		FindPreviousOperation state=(FindPreviousOperation)tar.AsyncState;
		if (state.Result==FindPreviousOperation.OperationResult.Finished && state.Match==null) {
			InformationAlert ia=new InformationAlert(Catalog.GetString("The pattern you requested was not found."), Catalog.GetString("Beginning of file reached."), mainWindow);
			ia.Run();
			ia.Destroy();
		}
	}
	
	//
	// called the first time a pattern is sought
	//
	void OnFirstFind()
	{
		//SearchFindNextAction.Sensitive=true;
		//SearchFindPreviousAction.Sensitive=true;
	}
	
	void UpdateActions(DataView dv)
	{
		if (dv==null) {
			FindAction.Sensitive=false;
			FindNextAction.Sensitive=false;
			FindPreviousAction.Sensitive=false;
			ReplaceAction.Sensitive=false;
			return;
		}
		
		FindAction.Sensitive=true;
		FindNextAction.Sensitive=true;
		FindPreviousAction.Sensitive=true;
		ReplaceAction.Sensitive=true;	
	}
}
	
///<summary>
/// A widget for find and replace operations 
///</summary>
public class FindReplaceWidget : Gtk.HBox
{
	DataBook dataBook;
	IFinder finder;
	
	bool searchPatternChanged;
	bool replacePatternChanged;
	
	byte[] replacePattern;
	
	[Glade.Widget] Gtk.Table FindReplaceTable;
	[Glade.Widget] Gtk.Button FindNextButton;
	[Glade.Widget] Gtk.Button FindPreviousButton;
	[Glade.Widget] Gtk.Button ReplaceButton;
	[Glade.Widget] Gtk.Button ReplaceAllButton;
	
	[Glade.Widget] Gtk.Label SearchLabel;
	[Glade.Widget] Gtk.Entry SearchPatternEntry;
	[Glade.Widget] Gtk.Label SearchAsLabel;
	[Glade.Widget] Gtk.ComboBox SearchAsComboBox;
	
	[Glade.Widget] Gtk.Label ReplaceLabel;
	[Glade.Widget] Gtk.Entry ReplacePatternEntry;
	[Glade.Widget] Gtk.Label ReplaceAsLabel;
	[Glade.Widget] Gtk.ComboBox ReplaceAsComboBox;

	[Glade.Widget] Gtk.Button CloseButton;
	
	Gtk.Widget previouslyFocused;
	
	enum ComboIndex { Hexadecimal, Decimal, Octal, Binary, Text }
	
	///<summary>
	/// Whether search-related widgets are visible  
	///</summary>
	public bool SearchVisible {
		set {
			SearchLabel.Visible=value;
			SearchPatternEntry.Visible=value;
			SearchAsLabel.Visible=value;
			SearchAsComboBox.Visible=value;
			FindNextButton.Visible=value;
			FindPreviousButton.Visible=value;
			if (value)
				SearchPatternEntry.GrabFocus();
		}
	}
	
	///<summary>
	/// Whether replace related widgets are visible
	///</summary>
	public bool ReplaceVisible {
		set {
			ReplaceLabel.Visible=value;
			ReplacePatternEntry.Visible=value;
			ReplaceAsLabel.Visible=value;
			ReplaceAsComboBox.Visible=value;
			ReplaceButton.Visible=value;
			ReplaceAllButton.Visible=value;
			if (value && SearchPatternEntry.Text.Length>0)
				ReplacePatternEntry.GrabFocus();
		}
	}
	
	public FindReplaceWidget(DataBook db, IFinder iFinder)
	{
		finder=iFinder;
		dataBook=db;
		
		Glade.XML gxml = new Glade.XML (FileResourcePath.GetSystemPath("..","data","bless.glade"), "FindReplaceTable", "bless");
		gxml.Autoconnect (this);
		
		this.Shown+=OnWidgetShown;
		SearchPatternEntry.Activated+=OnSearchPatternEntryActivated;
		ReplacePatternEntry.Activated+=OnReplacePatternEntryActivated;
		
		SearchPatternEntry.FocusGrabbed+=OnFocusGrabbed;
		ReplacePatternEntry.FocusGrabbed+=OnFocusGrabbed;
		
		SearchAsComboBox.Active=0;
		ReplaceAsComboBox.Active=0;
		
		SearchPatternEntry.Completion=new EntryCompletion();
		SearchPatternEntry.Completion.Model=new ListStore (typeof (string));
		SearchPatternEntry.Completion.TextColumn = 0;
		
		ReplacePatternEntry.Completion=new EntryCompletion();
		ReplacePatternEntry.Completion.Model=new ListStore (typeof (string));
		ReplacePatternEntry.Completion.TextColumn = 0;
		
		// initialize replace pattern
		replacePattern=new byte[0];
		
		this.Add(FindReplaceTable);
		this.ShowAll();
	}
	
	///<summary>
	/// Whether a widget in the FindReplaceWidget has the focus 
	///</summary>
	bool IsFocusInWidget()
	{
		foreach (Gtk.Widget child in  FindReplaceTable.Children) {
			Widget realChild=child;
			
			/*if (child.GetType()==typeof(Gtk.Combo))
				realChild=(child as Gtk.Combo).Entry;
			*/
			
			if (realChild.HasFocus)
				return true;
		}
		
		return false;
	}
	
	///<summary>Load the widget with data from the DataView's selection</summary>
	public void LoadWithSelection(DataView dv)
	{
		ByteBuffer bb=dv.Buffer;
		
		// load selection only if it isn't very large 
		if (this.Sensitive==true && dv.Selection.Size <= 1024 && dv.Selection.Size > 0) {
			// make sure selection is sorted
			Bless.Util.Range sel=new Bless.Util.Range(dv.Selection);
			sel.Sort();
			
			byte[] ba=new byte[sel.Size];
			for (int i=0;i<sel.Size;i++) {
				ba[i]=bb[sel.Start+i];
			}
		
			bool decodingOk=false;
			Area focusedArea=dv.FocusedArea;
			
			if (focusedArea!=null) { 
				switch (focusedArea.Type) {
					case "ascii": 
						string result=Encoding.ASCII.GetString(ba);
						// if the byte sequence cannot be displayed correctly 
						// as ascii, eg it contains a zero byte, use hexadecimal
						if (result.IndexOf('\x00')!=-1)
							decodingOk=false;
						else {	
							SearchAsComboBox.Active=(int)ComboIndex.Text;
							SearchPatternEntry.Text=result;
							decodingOk=true;
						}
						break;				
					case "decimal":
						SearchAsComboBox.Active=(int)ComboIndex.Decimal;
						SearchPatternEntry.Text=ByteArray.ToString(ba, 10);
						decodingOk=true;
						break;
					case "octal":
						SearchAsComboBox.Active=(int)ComboIndex.Octal;
						SearchPatternEntry.Text=ByteArray.ToString(ba, 8);
						decodingOk=true;
						break;
					case "binary":
						SearchAsComboBox.Active=(int)ComboIndex.Binary;
						SearchPatternEntry.Text=ByteArray.ToString(ba, 2);
						decodingOk=true;
						break;
				}// end switch
			}
			
			if (!decodingOk) {
				SearchAsComboBox.Active=(int)ComboIndex.Hexadecimal;
				SearchPatternEntry.Text=ByteArray.ToString(ba, 16);
			}
		}
	}
	
	///<summary>Update the search pattern from the text entry</summary>
	void UpdateSearchPattern()
	{
		if (!searchPatternChanged)
			return;
			
		try {
			byte[] ba;
			switch((ComboIndex)SearchAsComboBox.Active) {
				case ComboIndex.Hexadecimal:
					ba=ByteArray.FromString(SearchPatternEntry.Text, 16);
					break;
				case ComboIndex.Decimal:
					ba=ByteArray.FromString(SearchPatternEntry.Text, 10);
					break;
				case ComboIndex.Octal:
					ba=ByteArray.FromString(SearchPatternEntry.Text, 8);
					break;
				case ComboIndex.Binary:
					ba=ByteArray.FromString(SearchPatternEntry.Text, 2);
					break;
				case ComboIndex.Text:
					ba=Encoding.ASCII.GetBytes(SearchPatternEntry.Text);
					break;
				default:
					ba=new byte[0];
					break;
			} //end switch
			
			// if there is something in the text entry but nothing is parsed
			// it means it is full of spaces
			if (ba.Length==0 && SearchPatternEntry.Text.Length!=0)
				throw new FormatException(Catalog.GetString("Strings representing numbers cannot consist of whitespace characters only."));
			else
				finder.Strategy.Pattern=ba;
			
			// append string to drop-down list
			ListStore ls=(ListStore)SearchPatternEntry.Completion.Model;
			ls.AppendValues(SearchPatternEntry.Text);
		}	
		catch (FormatException e) {
			ErrorAlert ea=new ErrorAlert(Catalog.GetString("Invalid Search Pattern"), e.Message, null);
			ea.Run();
			ea.Destroy();
			throw;
		}
		searchPatternChanged=false;
	}
	
	void OnSearchPatternEntryActivated(object o, EventArgs  args)
	{
		if (FindNextButton.Sensitive==true)
			FindNextButton.Click();
	}
	
	void OnSearchPatternEntryChanged(object o, EventArgs args)
	{
		string pat=SearchPatternEntry.Text;
		if (pat.Length==0) {
			FindNextButton.Sensitive=false;
			FindPreviousButton.Sensitive=false;
			ReplaceButton.Sensitive=false;
			ReplaceAllButton.Sensitive=false;
		}
		else {
			FindNextButton.Sensitive=true;
			FindPreviousButton.Sensitive=true;
			ReplaceButton.Sensitive=true;
			ReplaceAllButton.Sensitive=true;
		}
		searchPatternChanged=true;
	}
	
	void OnSearchAsComboBoxChanged(object o, EventArgs args)
	{
		searchPatternChanged=true;
	}
	
	void OnFindNextButtonClicked(object o, EventArgs args)
	{
		try {
			UpdateSearchPattern();
			
			this.Sensitive=false;
			finder.FindNext(FindNextAsyncCallback);
		}
		catch(FormatException) { }
	}
	
	void FindNextAsyncCallback(IAsyncResult ar)
	{
		ThreadedAsyncResult tar=(ThreadedAsyncResult) ar;
		
		FindNextOperation state=(FindNextOperation)tar.AsyncState;
		if (state.Result==FindNextOperation.OperationResult.Finished && state.Match==null) {
			InformationAlert ia=new InformationAlert("The pattern you requested was not found.", "End of file reached.", null);
			ia.Run();
			ia.Destroy();
		}
		
		this.Sensitive=true;
		this.Visible=false;
		this.Visible=true;
	}
	
	void OnFindPreviousButtonClicked(object o, EventArgs args)
	{
		try {
			UpdateSearchPattern();
			
			this.Sensitive=false;
			finder.FindPrevious(FindPreviousAsyncCallback);
		}
		catch(FormatException) { }
	}
	
	void FindPreviousAsyncCallback(IAsyncResult ar)
	{
		ThreadedAsyncResult tar=(ThreadedAsyncResult) ar;
		
		FindPreviousOperation state=(FindPreviousOperation)tar.AsyncState;
		if (state.Result==FindPreviousOperation.OperationResult.Finished && state.Match==null) {
			InformationAlert ia=new InformationAlert(Catalog.GetString("The pattern you requested was not found."), Catalog.GetString("Beginning of file reached."), null);
			ia.Run();
			ia.Destroy();
		}
		
		this.Sensitive=true;
		this.Visible=false;
		this.Visible=true;
	}
	
	///
	//
	// Replace related methods
	//
	
	///<summary>Update the replace pattern from the text entry</summary>
	void UpdateReplacePattern()
	{
		if (!replacePatternChanged)
			return;
			
		try {
			switch((ComboIndex)ReplaceAsComboBox.Active) {
				case ComboIndex.Hexadecimal:
					replacePattern=ByteArray.FromString(ReplacePatternEntry.Text, 16);
					break;
				case ComboIndex.Decimal:
					replacePattern=ByteArray.FromString(ReplacePatternEntry.Text, 10);
					break;
				case ComboIndex.Octal:
					replacePattern=ByteArray.FromString(ReplacePatternEntry.Text, 8);
					break;
				case ComboIndex.Binary:
					replacePattern=ByteArray.FromString(ReplacePatternEntry.Text, 2);
					break;
				case ComboIndex.Text:
					replacePattern=Encoding.ASCII.GetBytes(ReplacePatternEntry.Text);
					break;
				default:
					break;
			} //end switch
			
			// if there is something in the text entry but nothing is parsed
			// it means it is full of spaces
			if (replacePattern.Length==0 && ReplacePatternEntry.Text.Length!=0)
				throw new FormatException("Strings representing numbers cannot consist of only whitespace characters. Leave the text entry completely blank for deletion of matched pattern(s).");
			
			// append string to drop-down list
			ListStore ls=(ListStore)ReplacePatternEntry.Completion.Model;
			ls.AppendValues(ReplacePatternEntry.Text);	
		}	
		catch (FormatException e) {
			ErrorAlert ea=new ErrorAlert("Invalid Replace Pattern", e.Message, null);
			ea.Run();
			ea.Destroy();
			throw;
		}
		replacePatternChanged=false;
	}
	
	void OnReplacePatternEntryActivated(object o, EventArgs  args)
	{
		if (ReplaceButton.Sensitive==true)
			ReplaceButton.Click();
	}
	
	void OnReplacePatternEntryChanged(object o, EventArgs args)
	{
		replacePatternChanged=true;
	}
	
	void OnReplaceAsComboBoxChanged(object o, EventArgs args)
	{
		replacePatternChanged=true;
	}
	
	
	void OnReplaceButtonClicked(object o, EventArgs args)
	{
		try {
			UpdateSearchPattern();
			UpdateReplacePattern();
			
			this.Sensitive=false;
			finder.Replace(replacePattern);
			finder.FindNext(FindNextAsyncCallback);
		}
		catch(FormatException) { }	
	}
	
	void OnReplaceAllButtonClicked(object o, EventArgs args)
	{
		try {
			UpdateSearchPattern();
			UpdateReplacePattern();
			
			this.Sensitive=false;
			finder.ReplaceAll(replacePattern, ReplaceAllAsyncCallback);
		}
		catch(FormatException) { }	
	}
	
	void ReplaceAllAsyncCallback(IAsyncResult ar)
	{
		ThreadedAsyncResult tar=(ThreadedAsyncResult) ar;
		
		ReplaceAllOperation state=(ReplaceAllOperation)tar.AsyncState;
		
		if (state.Result==ReplaceAllOperation.OperationResult.Finished) {
			InformationAlert ia=new InformationAlert("Found and replaced "+state.NumReplaced+" occurences.", "", null);
			ia.Run();
			ia.Destroy();
		}
		
		this.Sensitive=true;
		this.Visible=false;
		this.Visible=true;
	}
	
	void OnCloseButtonClicked(object o, EventArgs args)
	{
		if (dataBook.NPages > 0 && IsFocusInWidget()) {
			DataViewDisplay curdvd=(DataViewDisplay)dataBook.CurrentPageWidget;
			curdvd.GrabKeyboardFocus();
		}
		
		this.Hide();
		// forget focus when hiding
		previouslyFocused=null;
	}
	
	void OnFocusGrabbed(object o, EventArgs args)
	{
		// remember which widget has the focus
		previouslyFocused=(Widget)o;
	}
	
	void OnWidgetShown(object o, EventArgs args)
	{
		// when the dialog is shown, select and give the focus 
		// to the previously focused widget
		if (previouslyFocused!=null)
			previouslyFocused.GrabFocus();
		else
			SearchPatternEntry.GrabFocus();
	}
	
	protected override bool OnKeyPressEvent(Gdk.EventKey e)
	{
		// Escape hides the widget
		if (e.Key==Gdk.Key.Escape) {
			CloseButton.Click();
			return true;
		}
		else
			return base.OnKeyPressEvent(e);
	}
}

} // end namespace
