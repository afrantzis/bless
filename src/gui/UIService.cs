// created on 4/1/2007 at 2:12 PM
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

using Gtk;
using Bless.Util;

namespace Bless.Gui
{

public interface IInfoDisplay
{
	void DisplayMessage(string message);
	void ClearMessage();
}

public interface IProgressDisplay
{
	ProgressCallback NewCallback();
}


///<summary>
/// Provides services related to the UI
///</summary>
public class UIService
{
	UIManager uiManager;
	IInfoDisplay infoDisplay;
	IProgressDisplay progressDisplay;

	public UIService(UIManager uim)
	{
		uiManager = uim;
	}

	///<summary>
	/// Service for showing status messages
	///</summary>
	public IInfoDisplay Info {
		get { return infoDisplay; }
		set { infoDisplay = value; }
	}

	///<summary>
	/// Service for displaying the progress of various actions
	///</summary>
	public IProgressDisplay Progress {
		get { return progressDisplay; }
		set { progressDisplay = value; }
	}

	///<summary>
	/// The global UIManager
	///</summary>
	public UIManager Manager {
		get { return uiManager;}
	}

}

} // end namespace