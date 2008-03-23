// created on 1/21/2005 at 2:32 PM
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
using Gdk;
using Bless.Util;
using Mono.Unix;

namespace Bless.Gui.Dialogs {

public class AboutDialog: Gtk.AboutDialog
{
	public AboutDialog()
	{
		Artists = new string[] {"Michael Iatrou"};
		Authors = new string[] {"Alexandros Frantzis"};
		Copyright =  Catalog.GetString("Copyright 2004 - 2008 Alexandros Frantzis");
		ProgramName = "Bless";
		Version = ConfigureDefines.VERSION;
		Comments = Catalog.GetString("Bless is a Hex Editor for Gtk#");
		Website = "http://home.gna.org/bless";
		Logo =  new Gdk.Pixbuf(FileResourcePath.GetDataPath("bless-about.png"));
	}
}

} //end namespace
