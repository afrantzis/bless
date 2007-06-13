// created on 10/27/2004 at 3:13 PM
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
using System.IO;

namespace Bless.Util {

public class TempFile
{

	private TempFile() { }

	static public string CreateName(string dir)
	{
		string str;
		Random rand = new Random();

		do {
			str = string.Empty;
			for (int i = 0; i < 8; i++) {
				str += Convert.ToChar(rand.Next() % 26 + Convert.ToInt32('a'));
			}
		} while (File.Exists(dir + Path.DirectorySeparatorChar + str + ".bless") == true);

		//System.Console.WriteLine("Created random: {0}",str);
		return Path.Combine(dir, str + ".bless");
	}
}

}//end namespace
