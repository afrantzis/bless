// created on 12/25/2004 at 6:33 PM
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
using Gtk;
using Mono.Unix;

namespace Bless.Gui.Dialogs {

public class SaveFileItem
{
	public bool Save;
	public string Name;
	public int Page;
	public SaveFileItem(bool s, string n, int p) {Save = s; Name = n; Page = p;}
}

///<summary>An alert dialog box as recommended in the Gnome HIG</summary>
public class SaveConfirmationMultiAlert : Alert
{
	TreeView treeView;
	SaveFileItem[] fileList;

	public SaveConfirmationMultiAlert(SaveFileItem[] list, Gtk.Window parent)
			: base(string.Format(Catalog.GetString("There are {0} files with unsaved changes. Save changes before closing?"), list.Length),
				   Catalog.GetString("If you don't save, all changes made since the last save will be lost."), parent)
	{
		fileList = list;

		image.SetFromStock(Gtk.Stock.DialogWarning, Gtk.IconSize.Dialog);

		Label label = new Label(Catalog.GetString("\nSelect the files you want to save:\n"));
		label.Xalign = 0.0f;

		VBox vb = new VBox();
		vb.PackStart(label);
		treeView = CreateView(list);
		vb.PackStart(treeView);

		labelBox.PackStart(vb);
		labelBox.ReorderChild(vb, 1);

		this.AddButton(Catalog.GetString("Close without Saving"), ResponseType.No);
		this.AddButton(Gtk.Stock.Cancel, ResponseType.Cancel);
		this.AddButton(Gtk.Stock.Save, ResponseType.Ok);

		this.DefaultResponse = ResponseType.Cancel;

		this.ShowAll();
	}

	private TreeView CreateView(SaveFileItem[] list)
	{
		ListStore store = new ListStore (typeof (bool), typeof (string));
		TreeView tv = new TreeView();

		tv.Model = store;
		tv.HeadersVisible = false;

		CellRendererToggle crt = new CellRendererToggle();
		crt.Activatable = true;
		crt.Toggled += OnItemToggled;

		tv.AppendColumn ("Save", crt, "active", 0);
		tv.AppendColumn ("Name", new CellRendererText(), "text", 1);

		foreach (SaveFileItem item in list) {
			item.Save = true;
			store.AppendValues(item.Save, item.Name);
		}

		return tv;
	}

	private void OnItemToggled (object o, ToggledArgs args)
	{
		ListStore store = (ListStore)treeView.Model;

		Gtk.TreeIter iter;
		if (store.GetIterFromString (out iter, args.Path)) {
			bool val = (bool) store.GetValue (iter, 0);
			store.SetValue (iter, 0, !val);

			int row = Convert.ToInt32(args.Path);
			fileList[row].Save = !fileList[row].Save;
		}
	}

}



}