// created on 4/30/2006 at 11:49 AM
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
namespace Bless.Gui
{

public class Services
{
	static FileService fileService;
	static SessionService sessionService;
	static UIService uiService;

	public static FileService File {
		get { return fileService; }
		set { fileService = value; }
	}

	public static SessionService Session {
		get { return sessionService; }
		set { sessionService = value; }
	}

	public static UIService UI {
		get { return uiService; }
		set { uiService = value; }
	}
}

}