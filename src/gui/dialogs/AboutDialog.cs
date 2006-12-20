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
using Bless.Util;
using Mono.Unix;
 
namespace Bless.Gui.Dialogs {
 
///<summary>A singleton About Dialog</summary>
public class AboutDialog: Gtk.Window
{
		[Glade.Widget]
		Gtk.VBox AboutVBox;
		[Glade.Widget]
		Gtk.Image LogoImage;
		[Glade.Widget]
		Gtk.Button CloseButton;
		[Glade.Widget]
		Gtk.Label ReleaseLabel;

		const string release="Bless 0.4.9";

		static AboutDialog instance;

		private AboutDialog() : base (Catalog.GetString("About Bless"))
		{
				Glade.XML gxml = new Glade.XML (FileResourcePath.GetSystemPath("..","data","bless.glade"), "AboutVBox", "bless");
				gxml.Autoconnect (this);

				CloseButton.Clicked+=OnCloseClicked;

				LogoImage.FromFile=FileResourcePath.GetSystemPath("..","data","bless-about.png");
				ReleaseLabel.Markup="<span size=\"x-large\" weight=\"bold\">"+release+"</span>";

				this.TypeHint=Gdk.WindowTypeHint.Dialog;
				this.Add(AboutVBox);
				this.ShowAll();
		}

		private void OnCloseClicked(object o, EventArgs args)
		{
				LogoImage.Dispose();
				this.Destroy();
				instance=null;
		}

		protected override bool OnDeleteEvent(Gdk.Event e)
		{
				LogoImage.Dispose();
				this.Destroy();
				instance=null;
				return true;
		}

		///<summary>Show the dialog. Only one About dialog can be shown at a time.</summmary>
		public static new void Show()
		{
				if (instance==null) {
						instance=new AboutDialog();
				}
				instance.Present();
		}
}
 
} //end namespace
