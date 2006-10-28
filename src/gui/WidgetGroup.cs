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
using Gtk;

namespace Bless.Gui
{
	
public class WidgetGroup : HBox
{
	
	public WidgetGroup()
	{
	}
	
	protected override void OnAdded(Widget w)
	{
		Console.WriteLine("Onadded {0}", w);
		w.Hide();
		w.Shown += OnWidgetShown;
		w.Hidden += OnWidgetHidden;
		
		base.OnAdded(w);
	}
	
	protected override void OnRemoved(Widget w)
	{
		Console.WriteLine("Onremoved {0}", w);
		w.Shown -= OnWidgetShown;
		w.Hidden -= OnWidgetHidden;
		base.OnRemoved(w);
	}
	
	
	void OnWidgetShown(object sender, EventArgs e)
	{
		Widget w=(Widget)sender;
		Console.WriteLine("Onwidgetshown {0}", w);
		// Make sure only one widget is visible
		foreach(Widget child in Children) {
			if (child!=w)
				child.Hide();
		}
		
		// don't use ShowAll(): causes loop
		this.Show();
	}
	
	void OnWidgetHidden(object sender, EventArgs e)
	{
		/*Console.WriteLine("Onwidgethidden {0}", (Widget)sender);
		foreach(Widget child in Children) {
			if (child.Visible==true)
				return;
		}
		this.Hide();*/
	}
}

}
