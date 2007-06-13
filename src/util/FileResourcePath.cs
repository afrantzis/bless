// created on 3/11/2005 at 12:42 PM
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

using System.Reflection;
using System.IO;
using System;

namespace Bless.Util {

///<summary>Path of a file resource</summary>
public class FileResourcePath
{
	private FileResourcePath() { }

	///<summary>
	/// Gets the full path of a file resource
	/// which is given relatively to the
	/// assembly path
	///</summary>
	public static string GetSystemPath(params string[] dirs)
	{
		// Get assembly path
		string assemblyPath = Assembly.GetCallingAssembly().Location;
		// Get assembly dir
		string assemblyDir = Path.GetDirectoryName(assemblyPath);

		string resourcePath = assemblyDir;

		foreach (string s in dirs)
		resourcePath = Path.Combine(resourcePath, s);

		return resourcePath;
	}

	public static string GetUserPath(params string[] dirs)
	{
		string resourcePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "bless");

		foreach (string s in dirs)
		resourcePath = Path.Combine(resourcePath, s);

		return resourcePath;
	}


}

} // end namespace

