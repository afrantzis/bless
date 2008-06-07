// created on 6/14/2004 at 10:35 PM
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
using System.Collections;
using System;
using Gtk;
using Bless.Buffers;
using Bless.Gui.Areas;
using Bless.Gui.Drawers;
using Bless.Util;
using Bless.Tools;

namespace Bless.Gui {

///<summary>An interface to a widget that displays data from a buffer</summary>
public class DataView {
	Layout layout;
	ByteBuffer byteBuffer;
	Gtk.VScrollbar vscroll;
	bool widgetRealized;
	string prefID;

	DataViewControl dvControl;
	DataViewDisplay dvDisplay;

	// clipboard related
	byte[] clipdata;
	Gtk.Clipboard clipboard;

	static TargetEntry[] clipboardTargets = new TargetEntry[]{
												new TargetEntry("application/octet-stream", 0, 0),
												new TargetEntry("UTF8_STRING", 0, 0)
											};

	bool overwrite;
	bool notification;

	//undo-redo related
	Deque<CursorState> cursorUndoDeque;
	Deque<CursorState> cursorRedoDeque;

	public Deque<CursorState> CursorUndoDeque {
		get { return cursorUndoDeque; }
	}

	public Deque<CursorState> CursorRedoDeque {
		get { return cursorRedoDeque; }
	}

	public DataViewControl Control {
		get { return dvControl; }
	}

	public DataViewDisplay Display {
		get { return dvDisplay; }
	}

	public ByteBuffer Buffer {
		get { return byteBuffer; }
		set {
			if (value != null)
				SetupByteBuffer(value);
			else
				CleanupByteBuffer();
		}
	}

	public bool Overwrite {
		set {
			overwrite = value;
			if (OverwriteChanged != null)
				OverwriteChanged(this);
		}

		get { return overwrite; }
	}

	public Area FocusedArea {
		get { return dvDisplay.Layout.AreaGroup.FocusedArea; }
	}

	///<summary>
	/// Whether the DataView should notify the user
	/// that something important is going on
	///</summary>
	public bool Notification  {
		get { return notification; }
		set {
			notification = value;
			if (NotificationChanged != null)
				NotificationChanged(this);
		}
	}

	private void OnByteBufferChanged(ByteBuffer bb)
	{
		Gtk.Application.Invoke(delegate {
			if (byteBuffer.ReadAllowed)
				dvDisplay.Layout.AreaGroup.RedrawNow();
		});
	}

	private void OnByteBufferFileChanged(ByteBuffer bb)
	{
		Gtk.Application.Invoke(delegate {
			dvDisplay.ShowFileChangedBar();
			Notification = true;
			byteBuffer.FileOperationsAllowed = false;
		});
	}


	void OnPreferencesChanged(Preferences prefs)
	{
		if (byteBuffer == null)
			return;

		string undoLimited = prefs["Undo.Limited"];
		string undoActions = prefs["Undo.Actions"];

		if (undoLimited == "True") {
			byteBuffer.MaxUndoActions = Int32.Parse(undoActions);
			while (cursorUndoDeque.Count > byteBuffer.MaxUndoActions)
				cursorUndoDeque.RemoveEnd();
		}
		else
			byteBuffer.MaxUndoActions = -1;

		// temp dir
		byteBuffer.TempDir = prefs["ByteBuffer.TempDir"];
	}

	void AddUndoCursorState(CursorState state)
	{
		if (byteBuffer.MaxUndoActions != -1)
			while (cursorUndoDeque.Count >= byteBuffer.MaxUndoActions)
				cursorUndoDeque.RemoveEnd();

		cursorUndoDeque.AddFront(state);
	}

	private void DeleteSelectionInternal()
	{
		AreaGroup areaGroup = dvDisplay.Layout.AreaGroup;
		
		//
		// set cursor and selection before deleting so that the view remains in a consistent state
		// (eg the selection isn't beyond the EOF)
		//
		Util.Range prevSelection = this.Selection;
		AddUndoCursorState(new CursorState(areaGroup.CursorOffset, 0, areaGroup.Selection.Start, 0));
		cursorRedoDeque.Clear();

		this.MoveCursor(areaGroup.Selection.Start, 0);
		this.SetSelection(-1, -1);
		
		byteBuffer.Delete(prevSelection.Start, prevSelection.End);		
	}

	//<summary>Called when an application wants to get our clipboard data</summary>
	private void OnClipboardGet(Clipboard cb, SelectionData sd, uint n)
	{
		// if application accepts text
		if (sd.Target.Name == "UTF8_STRING") {
			switch (this.FocusedArea.Type) {
				case "hexadecimal":
					sd.Text = ByteArray.ToString(clipdata, 16);
					break;
				case "decimal":
					sd.Text = ByteArray.ToString(clipdata, 10);
					break;
				case "octal":
					sd.Text = ByteArray.ToString(clipdata, 8);
					break;
				case "binary":
					sd.Text = ByteArray.ToString(clipdata, 2);
					break;
				case "ascii":
					sd.Set(sd.Target, 8, clipdata);
					break;
				default:
					break;
			}

		} // if application accepts raw bytes
		else if (sd.Target.Name == "application/octet-stream") {
			sd.Set(sd.Target, 8, clipdata);
		}
	}

	private void OnClipboardClear(Clipboard cb)
	{

	}

	///<summary>Returns any clipboard data we can accept</summary>
	private byte[] GetPasteData()
	{
		byte[] data = null;

		SelectionData sd = clipboard.WaitForContents(Gdk.Atom.Intern("application/octet-stream", false));

		// if there are no octet-stream data, try UTF8_STRING
		// and try to convert it to bytes
		if (sd == null) {
			sd = clipboard.WaitForContents(Gdk.Atom.Intern("UTF8_STRING", false));
			if (sd != null) {
				try {
					// parse string according to currently focused area
					switch (this.FocusedArea.Type) {
						case "hexadecimal":
							data = ByteArray.FromString(sd.Text, 16);
							break;
						case "decimal":
							data = ByteArray.FromString(sd.Text, 10);
							break;
						case "octal":
							data = ByteArray.FromString(sd.Text, 8);
							break;
						case "binary":
							data = ByteArray.FromString(sd.Text, 2);
							break;
						case "ascii":
							data = sd.Data;
							break;
						default:
							break;
					}
				}
				catch (FormatException) {
					// if string cannot be parsed, consider it as plain text
					data = sd.Data;
				}
			}
		}
		else // found octet-stream data
			data = sd.Data;

		return data;
	}

	///<summary>Create a DataView</summary>
	public DataView()
	{
		dvControl = new DataViewControl(this);
		dvDisplay = new DataViewDisplay(this);

		dvDisplay.Control = dvControl;
		dvControl.Display = dvDisplay;

		// initialize clipdata buffer
		// and clipboard
		clipdata = new byte[0];
		clipboard = Gtk.Clipboard.Get(Gdk.Atom.Intern("CLIPBOARD", true));

		// initialize undo/redo cursor offset stacks
		cursorUndoDeque = new Deque<CursorState>();
		cursorRedoDeque = new Deque<CursorState>();

		prefID = "dv" + this.GetHashCode().ToString();
		PreferencesChangedHandler handler = new PreferencesChangedHandler(OnPreferencesChanged);
		Preferences.Proxy.Subscribe("Undo.Limited", prefID, handler);
		Preferences.Proxy.Subscribe("Undo.Actions", prefID, handler);
		Preferences.Proxy.Subscribe("Highlight.PatternMatch", prefID, handler);
		Preferences.Proxy.Subscribe("ByteBuffer.TempDir", prefID, handler);
	}

	public void Copy()
	{
		AreaGroup areaGroup = dvDisplay.Layout.AreaGroup;
		
		if (areaGroup.Areas.Count <= 0)
			return;

		if (!byteBuffer.ReadAllowed)
			return;

		byte[] ba = byteBuffer.RangeToByteArray(areaGroup.Selection);

		// if no selection, do nothing (keep old clipboard data)
		if (ba == null)
			return;

		clipdata = ba;

		// set clipboard
		clipboard.SetWithData(clipboardTargets, new ClipboardGetFunc(OnClipboardGet),
							  new ClipboardClearFunc(OnClipboardClear));

		dvDisplay.MakeOffsetVisible(areaGroup.CursorOffset, DataViewDisplay.ShowType.Closest);
	}

	public void Cut()
	{
		AreaGroup areaGroup = dvDisplay.Layout.AreaGroup;
		
		if (areaGroup.Areas.Count <= 0)
			return;
		
		// if we can't modify the buffer...
		if (!byteBuffer.ModifyAllowed)
			return;
		
		// if the buffer isn't resizable...
		if (!byteBuffer.IsResizable) {
			return;
		}

		byte[] ba = byteBuffer.RangeToByteArray(areaGroup.Selection);

		// if no selection, do nothing (keep old clipboard data)
		if (ba == null)
			return;

		clipdata = ba;

		// set clipboard
		clipboard.SetWithData(clipboardTargets, new ClipboardGetFunc(OnClipboardGet),
							  new ClipboardClearFunc(OnClipboardClear));

		this.DeleteSelectionInternal();
				
		// Make sure dataView.Redraw() is called before dvDisplay.MakeOffsetVisible()
		// so that the Scrollbar has the correct range
		// when calling dataView.Goto().
		// dataView.Redraw();
		dvDisplay.MakeOffsetVisible(areaGroup.CursorOffset, DataViewDisplay.ShowType.Closest);
	}

	public void Paste()
	{
		AreaGroup areaGroup = dvDisplay.Layout.AreaGroup;
		
		if (areaGroup.Areas.Count <= 0)
			return;

		// if we can't modify the buffer...
		if (!byteBuffer.ModifyAllowed)
			return;
			
		// if the buffer isn't resizable, ignore non-overwriting paste
		if (!byteBuffer.IsResizable && !overwrite) {
			return;
		}

		// get data from clipboard
		byte[] pasteData = GetPasteData();

		// if there wasn't any suitable data, return
		if (pasteData == null || pasteData.Length == 0)
			return;

		// if no range is selected insert/overtwrite the data and move
		// cursor to the end of inserted data
		if (areaGroup.Selection.IsEmpty()){
			// if user wants to overwrite and there is something to overwrite...
			// There is something to overwrite if we are not pasting at the end
			// of the file.
			if (overwrite == true && areaGroup.CursorOffset < byteBuffer.Size) {
				long endPos = areaGroup.CursorOffset + pasteData.Length - 1;
				if (endPos >= byteBuffer.Size)
					endPos = byteBuffer.Size - 1;
				byteBuffer.Replace(areaGroup.CursorOffset, endPos, pasteData);
			}
			else // if user doesn't want to overwrite or there is nothing to overwrite
				byteBuffer.Insert(areaGroup.CursorOffset, pasteData, 0, pasteData.LongLength);

			AddUndoCursorState(new CursorState(areaGroup.CursorOffset, 0, areaGroup.CursorOffset + pasteData.Length, 0));
			cursorRedoDeque.Clear();

			this.MoveCursor(areaGroup.CursorOffset + pasteData.Length, 0);
		}
		else {
			// if a range is selected, replace the range with
			// the data and move the cursor to the end of
			// the new data
			byteBuffer.Replace(areaGroup.Selection.Start, areaGroup.Selection.End, pasteData);
			AddUndoCursorState(new CursorState(areaGroup.Selection.Start, 0, areaGroup.Selection.Start + pasteData.Length, 0));
			cursorRedoDeque.Clear();

			this.MoveCursor(areaGroup.Selection.Start + pasteData.Length, 0);
			this.SetSelection(-1, -1);
		}

		// Make sure dataView.Redraw() is called before dvDisplay.MakeOffsetVisible()
		// so that the Scrollbar has the correct range
		// when calling dvDisplay.MakeOffsetVisible().
		// dataView.Redraw();
		dvDisplay.MakeOffsetVisible(areaGroup.CursorOffset, DataViewDisplay.ShowType.Closest);
	}

	public void Delete()
	{
		AreaGroup areaGroup = dvDisplay.Layout.AreaGroup;
		
		if (areaGroup.Areas.Count <= 0)
			return;

		// if we can't modify the buffer...
		if (!byteBuffer.ModifyAllowed)
			return;

		// if nothing is selected, delete the byte at the current offset
		if (areaGroup.Selection.IsEmpty() == true) {
			if (areaGroup.CursorOffset < byteBuffer.Size) {
				byteBuffer.Delete(areaGroup.CursorOffset, areaGroup.CursorOffset);
				AddUndoCursorState(new CursorState(areaGroup.CursorOffset, areaGroup.CursorDigit, areaGroup.CursorOffset, areaGroup.CursorDigit));
				cursorRedoDeque.Clear();
			}
		}
		else { // delete the selection
			DeleteSelectionInternal();
		}

		dvDisplay.MakeOffsetVisible(areaGroup.CursorOffset, DataViewDisplay.ShowType.Closest);
	}

	public void DeleteBackspace()
	{
		AreaGroup areaGroup = dvDisplay.Layout.AreaGroup;
		
		if (areaGroup.Areas.Count <= 0)
			return;

		if (!byteBuffer.ModifyAllowed)
			return;

		// if nothing is selected, delete the byte at the previous offset
		if (areaGroup.Selection.IsEmpty() == true) {
			long cOffset = areaGroup.CursorOffset;

			if (cOffset > 0) {
				this.MoveCursor(cOffset - 1, areaGroup.CursorDigit);
				byteBuffer.Delete(cOffset - 1, cOffset - 1);
				AddUndoCursorState(new CursorState(cOffset, areaGroup.CursorDigit, cOffset - 1, areaGroup.CursorDigit));
				cursorRedoDeque.Clear();
			}
		}
		else { // delete the selection
			DeleteSelectionInternal();
		}

		dvDisplay.MakeOffsetVisible(areaGroup.CursorOffset, DataViewDisplay.ShowType.Closest);
	}

	public void Undo()
	{
		// if we can't modify the buffer...
		if (!byteBuffer.ModifyAllowed)
			return;

		// undo buffer changes
		byteBuffer.Undo();

		// restore cursor position
		if (cursorUndoDeque.Count > 0) {
			CursorState ch = (CursorState)cursorUndoDeque.RemoveFront();
			cursorRedoDeque.AddFront(ch);

			// clear the selection
			this.SetSelection(-1, -1);

			dvDisplay.MakeOffsetVisible(ch.UndoOffset, DataViewDisplay.ShowType.Closest);
			this.MoveCursor(ch.UndoOffset, ch.UndoDigit);
		}
	}

	public void Redo()
	{
		// if we can't modify the buffer...
		if (!byteBuffer.ModifyAllowed)
			return;

		// redo buffer changes
		byteBuffer.Redo();

		// restore cursor position
		if (cursorRedoDeque.Count > 0) {
			CursorState ch = (CursorState)cursorRedoDeque.RemoveFront();
			AddUndoCursorState(ch);

			// clear the selection
			this.SetSelection(-1, -1);

			dvDisplay.MakeOffsetVisible(ch.RedoOffset, DataViewDisplay.ShowType.Closest);
			this.MoveCursor(ch.RedoOffset, ch.RedoDigit);
		}
	}

	public void Revert()
	{
		// if we can't modify the buffer...
		if (!byteBuffer.ModifyAllowed)
			return;

		byteBuffer.Revert();

		cursorRedoDeque.Clear();
		cursorUndoDeque.Clear();

		MoveCursor(0, 0);
	}

	private void SetupByteBuffer(ByteBuffer bb)
	{
		byteBuffer = bb;
		byteBuffer.Changed += OnByteBufferChanged;
		byteBuffer.FileChanged += OnByteBufferFileChanged;

		
		AreaGroup areaGroup = dvDisplay.Layout.AreaGroup;
		
		areaGroup.Buffer = byteBuffer;
		areaGroup.SetCursor(0, 0);
		areaGroup.Selection = new Util.Range();
		
		dvDisplay.Redraw();
		dvDisplay.VScroll.Adjustment.Value = 0;

		OnPreferencesChanged(Preferences.Instance);
		if (BufferChanged != null)
			BufferChanged(this);
	}

	private void CleanupByteBuffer()
	{
		if (byteBuffer != null) {
			byteBuffer.Changed -= OnByteBufferChanged;
			byteBuffer.FileChanged -= OnByteBufferFileChanged;
		}
		
		AreaGroup areaGroup = dvDisplay.Layout.AreaGroup;
		
		areaGroup.Buffer = null;
		areaGroup.SetCursor(0, 0);
		areaGroup.Selection.Clear();

		byteBuffer = null;
	}

	public void Cleanup()
	{
		this.Buffer = null;

		// all these to help the GC...
		dvDisplay.Cleanup();
		dvDisplay = null;
		dvControl.Cleanup();
		dvControl = null;

		Preferences.Proxy.Unsubscribe("Undo.Limited", prefID);
		Preferences.Proxy.Unsubscribe("Undo.Actions", prefID);
		Preferences.Proxy.Unsubscribe("Highlight.PatternMatch", prefID);
		Preferences.Proxy.Unsubscribe("ByteBuffer.TempDir", prefID);
	}
	
	/// <summary>
	/// Sets the current selection. It makes sure that the selection is sorted.
	/// </summary>
	public void SetSelection(long start, long end)
	{
		AreaGroup areaGroup = dvDisplay.Layout.AreaGroup;
		
		// check whether the selection has really
		// changed...
		if (areaGroup.Areas.Count <= 0)
			return;

		Bless.Util.IRange sel = areaGroup.Selection;

		// if there is no change, don't do anything
		if (sel.Start == start && sel.End == end)
			return;

		Bless.Util.Range newSel = new Bless.Util.Range(start, end);
		newSel.Sort();
		
		areaGroup.Selection = newSel;
		
		if (SelectionChanged != null)
			SelectionChanged(this);
	}

	public Bless.Util.Range Selection
	{
		get {
			AreaGroup areaGroup = dvDisplay.Layout.AreaGroup;
			
			if (areaGroup.Areas.Count <= 0)
				return new Bless.Util.Range();


			Bless.Util.Range r = areaGroup.Selection;
			return new Bless.Util.Range(r);
		}
		set {
			this.SetSelection(value.Start, value.End);
		}
	}


	public void MoveCursor(long offset, int digit)
	{
		AreaGroup areaGroup = dvDisplay.Layout.AreaGroup;
		
		areaGroup.SetCursor(offset, digit);

		if (CursorChanged != null)
			CursorChanged(this);
	}


	public long CursorOffset
	{
		get {
			AreaGroup areaGroup = dvDisplay.Layout.AreaGroup;
			
			return areaGroup.CursorOffset;
		}
	}

	public int CursorDigit
	{
		get {
			AreaGroup areaGroup = dvDisplay.Layout.AreaGroup;
			
			return areaGroup.CursorDigit;
		}
	}

	public long Offset
	{
		get {
			AreaGroup areaGroup = dvDisplay.Layout.AreaGroup;
			
			return areaGroup.Offset;
		}

		set {
			AreaGroup areaGroup = dvDisplay.Layout.AreaGroup;
			
			areaGroup.Offset = value;
		}
	}

	public delegate void DataViewEventHandler(DataView dv);

	///<summary>
	/// Fire the FocusChangedEvent
	///</summary>
	public void FireFocusChangedEvent()
	{
		if (FocusChanged != null)
			FocusChanged(this);
	}

	// events
	public event DataViewEventHandler BufferChanged;
	public event DataViewEventHandler SelectionChanged;
	public event DataViewEventHandler CursorChanged;
	public event DataViewEventHandler OverwriteChanged;
	public event DataViewEventHandler NotificationChanged;
	public event DataViewEventHandler FocusChanged;

}// end DataView


// class to store cursor position for undo/redo
public class CursorState {
	public long UndoOffset;
	public int UndoDigit;
	public long RedoOffset;
	public int RedoDigit;
	public CursorState(long uo, int ud, long ro, int rd)
	{ UndoOffset = uo; RedoOffset = ro; UndoDigit = ud; RedoDigit = rd;}
}


}//namespace
