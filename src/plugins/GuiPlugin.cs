// created on 4/28/2006 at 4:57 PM
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
using Gtk;
 
namespace Bless.Plugins
{
	
public class GuiPlugin : Plugin
{
	
	public GuiPlugin()
	{
		
	}
	
	protected Widget GetDataBook(Window win)
	{
		VBox vbox=(VBox)win.Child;
		foreach (Widget child in vbox.Children) {
			//System.Console.WriteLine("Child: {0}", child.GetType().ToString());
			if (child.GetType()==typeof(HBox)) {
				foreach (Widget child1 in ((HBox)child).Children) {
					if (child1.GetType().ToString()=="Bless.Gui.DataBook")
						return child1;
				}
			}
			
		} 
		return null;
	}
	
	protected Widget GetMenuBar(Window win)
	{
		VBox vbox=(VBox)win.Child;
		foreach (Widget child in vbox.Children) {
			//System.Console.WriteLine("Child: {0}", child.GetType().ToString());
			if (child.GetType()==typeof(MenuBar)) {
				return child;
			}
		}
		
		return null;
	}
	
	protected Widget GetWidgetGroup(Window win, int n)
	{
		VBox vbox=(VBox)win.Child;
		int i=0;
		foreach (Widget child in vbox.Children) {
			//System.Console.WriteLine("Child: {0}", child.GetType().ToString());
			if (child.GetType().ToString()=="Bless.Gui.WidgetGroup") {
				if (i==n)
					return child;
				else
					i++;
			}
		}
		
		return null;
	}

	protected Widget GetSideWidgetGroup(Window win, int n)
	{
		VBox vbox=(VBox)win.Child;
		int i=0;
		HBox hbox=null;
		
		foreach (Widget child in vbox.Children) {
			System.Console.WriteLine("Child: {0}", child.GetType().ToString());
			if (child.GetType().ToString()=="Gtk.HBox") {
				hbox=(HBox)child;
				break;
			}
		}
		if (hbox==null)
			return null;
		
		foreach (Widget child in hbox.Children) {
			System.Console.WriteLine("Child: {0}", child.GetType().ToString());
			if (child.GetType().ToString()=="Bless.Gui.WidgetGroup") {
				if (i==n)
					return child;
				else
					i++;
			}
		}
		
		return null;
	}
	
	
}
	
	
	
}
 
 
