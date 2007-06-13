// created on 4/29/2005 at 1:36 PM
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
using Bless.Gui.Dialogs;
using Mono.Unix;

namespace Bless.Gui
{

///<summary>
/// A widget that notifies the user that the file has changed
/// and prompts them to reload or ignore.
///</summary>
public class FileChangedBar : Gtk.HBox
{
	DataView dataView;

	public FileChangedBar(DataView dv)
	{
		dataView = dv;

		this.BorderWidth = 3;

		Gtk.Image img = new Gtk.Image(Gtk.Stock.DialogWarning, Gtk.IconSize.SmallToolbar);

		Gtk.Label label = new Gtk.Label(Catalog.GetString("This file has been changed on disk. You may choose to ignore the changes but reloading is the only safe option."));
		label.LineWrap = true;
		label.Wrap = true;

		Gtk.Button buttonIgnore = new Gtk.Button(Catalog.GetString("Ignore"));
		buttonIgnore.Clicked += OnFileChangedIgnore;

		Gtk.Button buttonReload = new Gtk.Button(Catalog.GetString("Reload"));
		buttonReload.Clicked += OnFileChangedReload;

		this.PackStart(img, false, false, 4);
		this.PackStart(label, false, false, 10);
		this.PackStart(buttonIgnore, false, false, 10);
		this.PackStart(buttonReload, false, false, 10);
	}

	void OnFileChangedIgnore(object o, EventArgs args)
	{
		WarningAlert wa = new WarningAlert(Catalog.GetString("Are you sure you want to ignore the changes?"), Catalog.GetString("Due to the way Bless handles files, ignoring these changes may corrupt your data."), null);
		ResponseType res = (ResponseType)wa.Run();
		wa.Destroy();

		if (res == ResponseType.Ok) {
			this.Visible = false;
			dataView.Notification = false;
			dataView.Buffer.FileOperationsAllowed = true;
		}
	}

	void OnFileChangedReload(object o, EventArgs args)
	{
		this.Visible = false;
		dataView.Buffer.FileOperationsAllowed = true;
		dataView.Notification = false;

		dataView.Revert();
	}
}


} // end namespace
