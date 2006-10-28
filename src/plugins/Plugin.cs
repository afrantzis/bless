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

namespace Bless.Plugins
{

public class Plugin
{
	protected string name;
	protected string author;
	protected string description;
	protected bool loaded;
	
	public Plugin()
	{
	}
	
	public virtual bool Load()
	{
		return false;
	}
	
	public virtual bool UnLoad()
	{
		return false;	
	}
	
	public string Name {
		get { return name; }
		set { name=value; }
	}
	
	public string Author {
		get { return author;}
		set { author=value; }
	}
	
	public string Description {
		get { return description; }
		set { description=value; }
	}
	
	public bool Loaded {
		get { return loaded; }
	}
}
	
}
