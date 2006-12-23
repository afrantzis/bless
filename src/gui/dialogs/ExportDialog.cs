// created on 12/4/2006 at 1:25 AM
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
using System.Threading;
using System.IO;
using Gtk;
using Bless.Plugins;
using Bless.Tools.Export;
using Bless.Gui;
using Bless.Util;
using Bless.Buffers;
using Mono.Unix;

namespace Bless.Gui.Dialogs
{

public class ExportDialog : Dialog
{
	PluginManager pluginManager;
	DataBook dataBook;
	Gtk.Window mainWindow;
	
	[Glade.Widget] Gtk.VBox ExportDialogVBox;
	[Glade.Widget] Gtk.ComboBox ExportAsCombo;
	[Glade.Widget] Gtk.ComboBoxEntry ExportPatternComboEntry;
	[Glade.Widget] Gtk.ProgressBar ExportProgressBar;
	[Glade.Widget] Gtk.Entry ExportFileEntry;
	[Glade.Widget] Gtk.RadioButton WholeFileRadio;
	[Glade.Widget] Gtk.RadioButton CurrentSelectionRadio;
	[Glade.Widget] Gtk.RadioButton RangeRadio;
	[Glade.Widget] Gtk.Entry RangeFromEntry;
	[Glade.Widget] Gtk.Entry RangeToEntry;
	[Glade.Widget] Gtk.HBox ProgressHBox;
	Gtk.Button CloseButton;
	Gtk.Button ExportButton;
	
	AutoResetEvent exportFinishedEvent;
	readonly public object LockObj=new object();
	
	bool cancelClicked;
	
	public ExportDialog(DataBook db, Gtk.Window mw)
	: base(Catalog.GetString("Export Bytes"), null, 0) 
	{
		Glade.XML gxml = new Glade.XML (FileResourcePath.GetSystemPath("..","data","bless.glade"), "ExportDialogVBox", "bless");
		gxml.Autoconnect (this);
		
		dataBook = db;
		mainWindow = mw;
		pluginManager = new PluginManager(typeof(ExportPlugin), new object[0]);
		exportFinishedEvent = new AutoResetEvent(false);
		
		SetupExportPlugins();
		
		ExportPatternComboEntry.Model = new ListStore (typeof (string));
		ExportPatternComboEntry.TextColumn = 0;
		LoadFromPatternFile((ListStore)ExportPatternComboEntry.Model);
		
		ProgressHBox.Visible = false;
		cancelClicked = false;
		
		this.Modal = false;
		this.BorderWidth = 6;
		this.HasSeparator = false;
		CloseButton = (Gtk.Button)this.AddButton(Gtk.Stock.Close, ResponseType.Close);
		ExportButton = (Gtk.Button)this.AddButton(Catalog.GetString("Export"), ResponseType.Ok);
		this.Response += new ResponseHandler(OnDialogResponse);
		this.VBox.Add(ExportDialogVBox);
	}
	
	private void SetupExportPlugins()
	{
		ListStore model = new ListStore(typeof(string), typeof(ExportPlugin));
		 
		foreach(ExportPlugin plugin in pluginManager.Plugins) {
			model.AppendValues(plugin.Description, plugin);
		}
		
		Gtk.CellRenderer renderer = new Gtk.CellRendererText();
		
		ExportAsCombo.PackStart(renderer, false);
		ExportAsCombo.AddAttribute(renderer, "text", 0);
		ExportAsCombo.Model = model;
		ExportAsCombo.Active = 0;
	}
	
	private Util.Range GetCurrentRange(DataView dv)
	{
		if (WholeFileRadio.Active == true)
			return new Util.Range(0, dv.Buffer.Size - 1);
		else if (CurrentSelectionRadio.Active == true)
			return dv.Selection;
		else if (RangeRadio.Active == true)
			return new Util.Range(BaseConverter.Parse(RangeFromEntry.Text), BaseConverter.Parse(RangeToEntry.Text));
	
		return new Util.Range();
	}
	
	private void OnSelectFileButtonClicked(object o, EventArgs args)
	{
		FileChooserDialog fcd = new FileChooserDialog(Catalog.GetString("Select file"), mainWindow, FileChooserAction.Save,  Catalog.GetString("Cancel"), ResponseType.Cancel,
                                      Catalog.GetString("Select"), ResponseType.Accept);
		if ((ResponseType)fcd.Run() == ResponseType.Accept)
			ExportFileEntry.Text = fcd.Filename;
		fcd.Destroy();
	}
	
	private void OnRangeRadioToggled(object o, EventArgs args)
	{
		RangeFromEntry.Sensitive = RangeRadio.Active;
		RangeToEntry.Sensitive = RangeRadio.Active;
	}
	
	private void OnExportCancelClicked(object o, EventArgs args)
	{
		cancelClicked = true;
	}
	
	
	private void OnDeletePatternButtonClicked(object o, EventArgs args)
	{
		string pattern = ExportPatternComboEntry.Entry.Text;
		
		if (pattern != "") {
			ListStore ls = (ListStore)ExportPatternComboEntry.Model;
			TreeIter iter;
			int index = FindPattern(ls, pattern, out iter);
			if (index >= 0) {
				ls.Remove(ref iter);
				UpdatePatternFile(ls);
			}
		}
	}
	
	private void OnSavePatternButtonClicked(object o, EventArgs args)
	{
		string pattern = ExportPatternComboEntry.Entry.Text;
		
		if (pattern != "") {
			ListStore ls = (ListStore)ExportPatternComboEntry.Model;
			TreeIter iter;
			if (FindPattern(ls, pattern, out iter) < 0) {
				ls.AppendValues(pattern);
				UpdatePatternFile(ls);
			}
		}
		
	}
	
	private int FindPattern(ListStore ls, string pattern, out TreeIter ti)
	{
		ti = new TreeIter();
		TreeIter iter;
		if (ls.GetIterFirst(out iter) == false)
			return -1;
		
		string val;
		int i = 0;
		
		while((val = (ls.GetValue(iter, 0) as string)) != null) {
			if (pattern == val) {
				ti = iter;
				return i;
			}
			ls.IterNext(ref iter);
			i++;
		}
		
		return -1;
	}
	
	private FileStream GetPatternFile(FileMode mode, FileAccess access)
	{
		string patternDir = FileResourcePath.GetUserPath();
		
		FileStream fs = new FileStream(System.IO.Path.Combine(patternDir, "export_patterns"), mode, access);	
		
		return fs;
	}
	
	private void LoadFromPatternFile(ListStore ls)
	{
		StreamReader reader;
		
		try {
			FileStream fs = GetPatternFile(FileMode.Open, FileAccess.Read);
			reader = new StreamReader(fs);
		}
		catch(Exception e) {
			System.Console.WriteLine(e.Message);
			return;
		}
		
		string pattern;
		while ((pattern = reader.ReadLine()) != null) {
			ls.AppendValues(pattern);
		}
		
		
		reader.BaseStream.Close();
	}
	
	private void UpdatePatternFile(ListStore ls)
	{
		StreamWriter writer;
		
		try {
			FileStream fs = GetPatternFile(FileMode.Create, FileAccess.Write);
			writer = new StreamWriter(fs);
		}
		catch(Exception e) {
			System.Console.WriteLine(e.Message);
			return;
		}
		
		foreach (object[] row in ls)
			writer.WriteLine(row[0] as string);
		
		writer.Flush();
		writer.BaseStream.Close();
	}
	
	
	private IAsyncResult BeginExport(IExporter exporter, IBuffer buf, long start, long end)
	{
		exportFinishedEvent.Reset();
		
		ExportOperation eo = new ExportOperation(exporter, buf, start, end, ExportProgressCallback, OnExportFinished);
		
		CloseButton.Sensitive = false;
		ExportButton.Sensitive = false;
		
		// start export thread
		Thread exportThread=new Thread(eo.OperationThread);
		exportThread.IsBackground=true;
		exportThread.Start();
				
		return new ThreadedAsyncResult(eo, exportFinishedEvent, false);
	}
	
	private void OnExportFinished(IAsyncResult ar)
	{
	lock (LockObj) {
		ExportOperation eo=(ExportOperation)ar.AsyncState;
		
		if (eo.Result == ExportOperation.OperationResult.Finished) {
			Services.Info.DisplayMessage(string.Format(Catalog.GetString("Exported data to '{0}'"),  (eo.Exporter.Builder.OutputStream as FileStream).Name));
			this.Hide();
		}
		else if (eo.Result == ExportOperation.OperationResult.CaughtException) {
			ErrorAlert ea;
			if (eo.ThreadException.GetType() == typeof(FormatException))
				ea = new ErrorAlert(Catalog.GetString("Export Pattern Error"), eo.ThreadException.Message, mainWindow);
			else
				ea = new ErrorAlert(Catalog.GetString("Exporting Error"), eo.ThreadException.Message, mainWindow);
				
			ea.Run();
			ea.Destroy();
		}
		
		if (eo.Exporter.Builder.OutputStream != null)
			eo.Exporter.Builder.OutputStream.Close();
			
		CloseButton.Sensitive = true;
		ExportButton.Sensitive = true;
	}
	}
	
	private bool ExportProgressCallback(object o, ProgressAction action)
	{
		if (action == ProgressAction.Hide) {
			ProgressHBox.Visible = false;
			return false; 
		}
		else if (action == ProgressAction.Show) {
			ProgressHBox.Visible = true;
			return false;
		}
		
		
		if ((double)o > 1.0)
			o = 1.0;
		
		ExportProgressBar.Fraction=(double)o;
		
		return cancelClicked;
	}
	
	void OnDialogResponse(object o, Gtk.ResponseArgs args)
	{
		lock (LockObj) {
		if (args.ResponseId == ResponseType.Ok && dataBook!=null && dataBook.NPages > 0) {
			DataView dv=((DataViewDisplay)dataBook.CurrentPageWidget).View;
			
			IExportBuilder builder = null;
			TreeIter iter;
			ExportAsCombo.GetActiveIter(out iter);
			ExportPlugin plugin = (ExportPlugin) ExportAsCombo.Model.GetValue(iter, 1);
			
			
			Util.Range range;
			
			try {
				range = GetCurrentRange(dv);
			}
			catch(FormatException ex) {
				ErrorAlert ea = new ErrorAlert(Catalog.GetString("Error in custom range"), ex.Message, mainWindow);
				ea.Run();
				ea.Destroy();
				return;
			}
			
			Util.Range bufferRange;
			if (dv.Buffer.Size == 0)
				bufferRange = new Util.Range();
			else
				bufferRange = new Util.Range(0, dv.Buffer.Size - 1);
			
			if (!bufferRange.Contains(range.Start) || !bufferRange.Contains(range.End)) {
				ErrorAlert ea = new ErrorAlert(Catalog.GetString("Error in range"), Catalog.GetString("Range is out of file's limits"), mainWindow);
				ea.Run();
				ea.Destroy();
				return;
			}
			
			Stream stream = null;
			try {
				stream = new FileStream(ExportFileEntry.Text, FileMode.Create, FileAccess.Write);
				builder = plugin.CreateBuilder(stream);
						
				InterpretedPatternExporter exporter = new InterpretedPatternExporter(builder);
				exporter.Pattern = ExportPatternComboEntry.Entry.Text;
				
				cancelClicked = false;
				BeginExport(exporter, dv.Buffer, range.Start, range.End);
				
			}
			catch(Exception ex) {
				if (stream != null)
					stream.Close();
					
				ErrorAlert ea = new ErrorAlert(Catalog.GetString("Error saving to file"), ex.Message, mainWindow);
				ea.Run();
				ea.Destroy();
				return;
			}
		}
		else if (args.ResponseId == ResponseType.Close)
			this.Hide();
		}
	}
	
}


} // end namespace