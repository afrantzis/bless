// created on 4/3/2007 at 6:30 PM
/*
 *   Copyright (c) 2007, Alexandros Frantzis (alf82 [at] freemail [dot] gr)
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
using System.Threading;
using Gtk;
using Glade;
using Bless.Util;
using Bless.Gui.Dialogs;
using Bless.Gui;
using Bless.Plugins;
using Bless.Buffers;
using Mono.Unix;

namespace Bless.Gui.Plugins {

///<summary>
/// A plugin to perform bytewise operation on data
///</summary>
public class BitwiseOperationsPlugin : GuiPlugin
{	
	BitwiseOperationsWidget widget;
	DataBook dataBook;
	
	const string uiXml=
	"<menubar>"+
	"	<menu action=\"Tools\">"+
	"		<menuitem name=\"BitwiseOperations\" action=\"BitwiseOperationsAction\" />"+
	"	</menu>"+
	"</menubar>"+
	"<popup name=\"DefaultAreaPopup\">"+
	"	<placeholder name=\"ExtraAreaPopupItems\" >"+
	"		<menuitem name=\"PerformBitwiseOperation\" action=\"PerformBitwiseOperationAction\" />"+
	"	</placeholder>"+
	"</popup>";
	
	Window mainWindow;
	UIManager uiManager;
	
	public BitwiseOperationsPlugin(Window mw, UIManager uim)
	{
		mainWindow = mw;
		uiManager = uim;
		
		dataBook = (DataBook)GetDataBook(mw);
		
		name = "BitwiseOperations";
		author = "Alexandros Frantzis";
		description = "Bitwise operations on data";
	}
	
	public override bool Load()
	{
		widget = new BitwiseOperationsWidget((DataBook)GetDataBook(mainWindow));
		widget.Visible = false;
		
		WidgetGroup wgroup = (WidgetGroup)GetWidgetGroup(mainWindow, 0);
		wgroup.Add(widget);
		
		AddMenuItems(uiManager);
		dataBook.PageAdded += new DataView.DataViewEventHandler(OnDataViewAdded);
		dataBook.Removed += new RemovedHandler(OnDataViewRemoved);
		dataBook.SwitchPage += new SwitchPageHandler(OnSwitchPage);
		
		loaded = true;
		return true;
	}
	
	private void AddMenuItems(UIManager uim)
	{
		ActionEntry[] actionEntries = new ActionEntry[] {
			new ActionEntry ("BitwiseOperationsAction", Stock.Execute, Catalog.GetString("_Bitwise Operations"), "<control>B", null,
			                    new EventHandler(OnBitwiseOperationsActivated)),
			new ActionEntry ("PerformBitwiseOperationAction", null, Catalog.GetString("Perform Operation"), "<control><shift>B", null,
			                    new EventHandler(OnPerformBitwiseOperation)),			                    
		};
		
		ActionGroup group = new ActionGroup ("BitwiseOperationsActions");
		group.Add (actionEntries);
		
		uim.InsertActionGroup(group, 0);
		uim.AddUiFromString(uiXml);
		
		uim.EnsureUpdate();
		
	}
	
	void OnDataViewAdded(DataView dv)
	{
		dv.Buffer.Changed += new ByteBuffer.ChangedHandler(OnBufferContentsChanged);
		dv.BufferChanged += new DataView.DataViewEventHandler(OnBufferChanged);
		dv.CursorChanged += new DataView.DataViewEventHandler(OnCursorChanged);
		dv.SelectionChanged += new DataView.DataViewEventHandler(OnSelectionChanged);
	}
	
	void OnDataViewRemoved(object o, RemovedArgs args)
	{
		DataView dv=((DataViewDisplay)args.Widget).View;
		dv.Buffer.Changed -= new ByteBuffer.ChangedHandler(OnBufferContentsChanged);
		dv.BufferChanged -= new DataView.DataViewEventHandler(OnBufferChanged);
		dv.CursorChanged -= new DataView.DataViewEventHandler(OnCursorChanged);
		dv.SelectionChanged -= new DataView.DataViewEventHandler(OnSelectionChanged);
	}
	
	void OnBufferChanged(DataView dv)
	{
		UpdateBitwiseOperationsWidget(dv);
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
		
		UpdateBitwiseOperationsWidget(dv);
	}
	
	void OnSwitchPage(object o, SwitchPageArgs args)
	{
		DataView dv=((DataViewDisplay)dataBook.GetNthPage((int)args.PageNum)).View;
		
		UpdateBitwiseOperationsWidget(dv);
	}
	
	void OnCursorChanged(DataView dv)
	{
		UpdateBitwiseOperationsWidget(dv);
	}
	
	void OnSelectionChanged(DataView dv)
	{
		UpdateBitwiseOperationsWidget(dv);
	}
	
	void UpdateBitwiseOperationsWidget(DataView dv)
	{
		if (dataBook.NPages == 0)
			return;
			
		DataView curdv=((DataViewDisplay)dataBook.CurrentPageWidget).View;
		if (curdv!=dv)
			return;
		
		widget.Update(dv);
	}
	
	///<summary>Handle the Tools -> Bitwise Operations command</summary>
	public void OnBitwiseOperationsActivated(object o, EventArgs args)
	{
		widget.Show();
	}
	
	public void OnPerformBitwiseOperation(object o, EventArgs args)
	{
		widget.PerformOperation();
	}
}
	
///<summary>
/// A widget for bitwise operations
///</summary>
public class BitwiseOperationsWidget : Gtk.HBox
{
	[Glade.Widget] Gtk.HBox BitwiseOperationsHBox;
	[Glade.Widget] Gtk.Label SourceLabel;
	[Glade.Widget] Gtk.EventBox SourceLabelEB;
	[Glade.Widget] Gtk.Button DoOperationButton;
	[Glade.Widget] Gtk.ComboBox OperationComboBox;
	[Glade.Widget] Gtk.ComboBox OperandAsComboBox;
	[Glade.Widget] Gtk.Entry OperandEntry;
	[Glade.Widget] Gtk.Button CloseButton;
	
	
	DataBook dataBook;
	int numberBase;
	
	enum OperandAsComboIndex { Hexadecimal, Decimal, Octal, Binary, Text }
	public enum OperationType { And, Or, Xor, Not }
	
	///<summary>
	/// The number base to use for displaying the source range
	///</summary>
	public int SourceLabelNumberBase {
		set { 
			numberBase = value;
			
			if (dataBook.NPages > 0) {
				DataView curdv = ((DataViewDisplay)dataBook.CurrentPageWidget).View;
			
				Update(curdv);
			}
		}
		
		get {
			return numberBase;
		}
	}
	
	public BitwiseOperationsWidget(DataBook db)
	{
		dataBook = db;
		
		Glade.XML gxml = new Glade.XML (FileResourcePath.GetSystemPath("..","data","bless.glade"), "BitwiseOperationsHBox", "bless");
		gxml.Autoconnect (this);
		
		OperationComboBox.Active = 0;
		OperandAsComboBox.Active = 0;
		numberBase = 16;
		
		// set button sensitivity 
		OnOperandEntryChanged(null, null);
		
		this.Shown += OnWidgetShown;
		
		this.Add(BitwiseOperationsHBox);
		this.ShowAll();
	}
	
	///<summary>
	/// Whether a widget in the Widget has the focus 
	///</summary>
	bool IsFocusInWidget()
	{
		foreach (Gtk.Widget child in  BitwiseOperationsHBox.Children) {
			Widget realChild = child;
			
			if (child.GetType() == typeof(Gtk.Alignment))
				realChild = (child as Gtk.Alignment).Child;
				
			if (realChild.HasFocus)
				return true;
		}
		
		return false;
	}
	
	void OnOperandEntryChanged(object o, EventArgs args)
	{
		if (OperandEntry.Text.Length > 0)
			DoOperationButton.Sensitive = true;
		else
			DoOperationButton.Sensitive = false;
	}
	
	void OnOperandEntryActivated(object o, EventArgs args)
	{
		if (DoOperationButton.Sensitive == true)
			DoOperationButton.Click();
	}
	
	void OnOperationComboBoxChanged(object o, EventArgs args)
	{
		if ((OperationType)OperationComboBox.Active == OperationType.Not) {
			OperandEntry.Sensitive = false;
			OperandAsComboBox.Sensitive = false;
			DoOperationButton.Sensitive = true;	
		}
		else {
			OperandEntry.Sensitive = true;
			OperandAsComboBox.Sensitive = true;
			OnOperandEntryChanged(null, null);
		}
	}
	
	
	///<summary>
	/// Parse the operand according to the selected type
	///</summary>
	byte[] ParseOperand()
	{
		byte[] ba;
		string str = OperandEntry.Text;
		
		switch((OperandAsComboIndex)OperandAsComboBox.Active) {
			case OperandAsComboIndex.Hexadecimal:
				ba = ByteArray.FromString(str, 16);
				break;
			case OperandAsComboIndex.Decimal:
				ba = ByteArray.FromString(str, 10);
				break;
			case OperandAsComboIndex.Octal:
				ba = ByteArray.FromString(str, 8);
				break;
			case OperandAsComboIndex.Binary:
				ba = ByteArray.FromString(str, 2);
				break;
			case OperandAsComboIndex.Text:
				ba = Encoding.ASCII.GetBytes(str);
				break;
			default:
				ba = new byte[0];
				break;
		}
	
		return ba;
	}
	
	
	void OnDoOperationClicked(object o, EventArgs args)
	{
		if (dataBook.NPages == 0)
			return;
			
		DataView dv = ((DataViewDisplay)dataBook.CurrentPageWidget).View;
		
		// get the operands as a byte array
		byte[] byteArray = null;
		
		try {
			byteArray = ParseOperand();
		}
		catch(FormatException e) {
			ErrorAlert ea = new ErrorAlert(Catalog.GetString("Error in Operand"), e.Message, null);
			ea.Run();
			ea.Destroy();
			return;
		}
		
		/// get the range to apply the operation to
		Util.Range range = dv.Selection;
			
		if (range.IsEmpty()) {
			Util.Range bbRange = dv.Buffer.Range;
			
			range.Start = dv.CursorOffset;
			range.End = range.Start + byteArray.Length - 1;
			range.Intersect(bbRange);
		}
		
		// don't allow buffer modification while the operation is perfoming
		dv.Buffer.ModifyAllowed = false;
		dv.Buffer.FileOperationsAllowed = false;
		
		BitwiseOperation bo = new BitwiseOperation(dv.Buffer, byteArray, range,
			(OperationType)OperationComboBox.Active,
			Services.UI.Progress.NewCallback(), BitwiseOperationAsyncCallback, true);
		
		// start operation thread
		Thread boThread = new Thread(bo.OperationThread);
		boThread.IsBackground = true;
		boThread.Start();
	}
	
	///<summary>
	/// Called when an asynchronous operation finishes.
	///</summary>
	void BitwiseOperationAsyncCallback(IAsyncResult ar)
	{
		BitwiseOperation state = (BitwiseOperation)ar.AsyncState;
		ThreadedAsyncOperation.OperationResult result = state.Result;
		
		// allow changes to the buffer
		state.Buffer.ModifyAllowed = true;
		state.Buffer.FileOperationsAllowed = true;
		
		switch (result) {
			case ThreadedAsyncOperation.OperationResult.Finished:
				break;
			case ThreadedAsyncOperation.OperationResult.Cancelled:
				state.Buffer.Undo();
				break;
			case ThreadedAsyncOperation.OperationResult.CaughtException:
				break;
			default:
				break;
		
		}
	}
	
	///<summary>
	/// Update the source range label 
	///</summary>
	public void Update(DataView dv)
	{
		if (dv == null)
			return;
			
		string str;
		
		if (dv.Selection.IsEmpty()) {
			string off1 = BaseConverter.ConvertToString(dv.CursorOffset, numberBase, true, true, 1);
			str = string.Format("({0},{1})", off1, off1);
		}
		else {
			string off1 = BaseConverter.ConvertToString(dv.Selection.Start, numberBase, true, true, 1);
			string off2 = BaseConverter.ConvertToString(dv.Selection.End, numberBase, true, true, 1);
			str = string.Format("({0},{1})", off1, off2);
		}
		
		SourceLabel.Text = str;
	}
	
	public void PerformOperation()
	{
		DoOperationButton.Click();
	}
	
	void OnWidgetShown(object o, EventArgs args)
	{
		OperandEntry.GrabFocus();
	}
	
	protected override bool OnKeyPressEvent(Gdk.EventKey e)
	{
		if (e.Key == Gdk.Key.Escape) {
			CloseButton.Click();
			return true;
		}
		else
			return base.OnKeyPressEvent(e);
	}
	
	///<summary>
	/// Handle the button press events on the source range label (cycles number bases) 
	///</summary>
	void OnSourceLabelButtonPress(object o, ButtonPressEventArgs args)
	{
		Gdk.EventButton e=args.Event;
		// ignore double and triple-clicks
		if (e.Type!=Gdk.EventType.ButtonPress)
			return;
		
		// cycle 8, 10 and 16 number bases
		if (this.SourceLabelNumberBase == 8)
			this.SourceLabelNumberBase = 10;
		else if (this.SourceLabelNumberBase == 10)
			this.SourceLabelNumberBase = 16;
		else if (this.SourceLabelNumberBase == 16)
			this.SourceLabelNumberBase = 8;
	}
	
	void OnCloseButtonClicked(object o, EventArgs args)
	{
		// give focus to active dataview if the widget has it
		if (dataBook.NPages > 0 && IsFocusInWidget()) {
			DataViewDisplay curdvd=(DataViewDisplay)dataBook.CurrentPageWidget;
			curdvd.GrabKeyboardFocus();
		}
		
		this.Hide();
	}
}

///<summary>
/// Performs a bitwise operation using an asynchronous threaded model
///</summary>
public class BitwiseOperation : ThreadedAsyncOperation
{
	protected ByteBuffer byteBuffer;
	byte[] byteArray;
	Util.Range range;
	long currentOffset;
	BitwiseOperationsWidget.OperationType operationType;
	
	public ByteBuffer Buffer {
		get { return byteBuffer; }
	}
	
	public BitwiseOperation(ByteBuffer bb, byte[] ba, Util.Range range, BitwiseOperationsWidget.OperationType ot, ProgressCallback pc,
							AsyncCallback ac, bool glibIdle): base(pc, ac, glibIdle)
	{
		byteBuffer = bb;
		byteArray = ba;
		this.range = range;
		operationType = ot;
		currentOffset = range.Start;
	}
	
	protected override bool StartProgress()
	{
		progressCallback(string.Format("Applying operation"), ProgressAction.Message);
		return progressCallback(((double)currentOffset - range.Start)/range.Size, ProgressAction.Show);
	}
	
	protected override bool UpdateProgress()
	{
		return progressCallback(((double)currentOffset - range.Start)/range.Size, ProgressAction.Update);
	}
	
	protected override bool EndProgress()
	{
		return progressCallback(((double)currentOffset - range.Start)/range.Size, ProgressAction.Destroy);
	}
	
	protected override void IdleHandlerEnd()
	{
		byteBuffer.EndActionChaining();
	}
	
	
	protected override void DoOperation()
	{
		byteBuffer.BeginActionChaining();
		
		if (range.IsEmpty())
			return;
		
		// create the operand array
		// this is either the original byte array
		// or a new array containing multiple copies of the original
		byte[] operandArray;
		
		if (byteArray.Length > 0) {
			long numberOfRepetitions = range.Size / byteArray.Length;
		
			// too many repetitions... create new array
			if (numberOfRepetitions > 1024) {
				int len = ((4096 / byteArray.Length) + 1) * byteArray.Length;
				operandArray = new byte[len];
				FillWithPattern(operandArray, byteArray);
			}
			else // use old array
				operandArray = byteArray;
		}
		else { // we have no operand eg when the operation is a NOT
			operandArray = new byte[4096];
		}
		
		// the array to keep the results of the operation
		byte[] resultArray = new byte[operandArray.Length];
		
		long left = range.Size;
		currentOffset = range.Start;
		
		while (left > 0 && !cancelled) {
			int min = (int)((left < resultArray.Length) ? left : resultArray.Length);
			// fill result array
			// ...more efficient way needed, perhaps implement bb.Read
			for (int i = 0; i < min; i++)
				resultArray[i] = byteBuffer[currentOffset + i];
			
			// perform the operation on the data
			DoOperation(resultArray, operandArray, min);
			
			lock(byteBuffer.LockObj) {
				byteBuffer.ModifyAllowed = true;
				// write the changed data back
				byteBuffer.Replace(currentOffset, currentOffset + min - 1, resultArray, 0, min);
				byteBuffer.ModifyAllowed = false;
			}
			
			currentOffset += min;
			left -= min;
		}
	}
	
	protected override void EndOperation()
	{
		
	}
	
	///<summary>
	/// Fill the dest array with the data found in the pattern array 
	///</summary>
	void FillWithPattern(byte[] dest, byte[] pattern)
	{
		if (pattern.Length == 0)
			return;
		
		int left = dest.Length;
		int offset = 0;
		
		while (left > 0) {
			int min = left < pattern.Length ? left : pattern.Length;
			
			Array.Copy(pattern, 0, dest, offset, min);
			
			offset += min;
			left -= min;
		}
	
	}
	
	///<summary>
	/// Perform the operation ba1[i] = ba1[i] op ba2[i] 
	///</summary>
	void DoOperation(byte[] ba1, byte[] ba2, int length)
	{
		switch(operationType) {
			case BitwiseOperationsWidget.OperationType.And:
				for (int i = 0; i < length; i++)
					ba1[i] = (byte)(ba1[i] & ba2[i]);
				break;
			case BitwiseOperationsWidget.OperationType.Or:
				for (int i = 0; i < length; i++)
					ba1[i] = (byte)(ba1[i] | ba2[i]);
				break;
			case BitwiseOperationsWidget.OperationType.Xor:
				for (int i = 0; i < length; i++)
					ba1[i] = (byte)(ba1[i] ^ ba2[i]);
				break;
			case BitwiseOperationsWidget.OperationType.Not:
				for (int i = 0; i < length; i++)
					ba1[i] = (byte)(~ba1[i]);
				break;
			default:
				break;
		}
	}
}

} // namespace
