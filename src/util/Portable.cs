/*
 *   Copyright (c) 2008, Alexandros Frantzis (alf82 [at] freemail [dot] gr)
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

using Mono.Unix.Native;

public class Portable
{
	public static long GetAvailableMemory()
	{
#if ENABLE_UNIX_SPECIFIC
		long availPages = Syscall.sysconf(SysconfName._SC_AVPHYS_PAGES);
		long pageSize = Syscall.sysconf(SysconfName._SC_PAGESIZE);
		long freeMem = availPages * pageSize;
			
		return freeMem;
#else
		throw new NotImplementedException();
#endif
	}

	public static long GetAvailableDiskSpace(string path)
	{
#if ENABLE_UNIX_SPECIFIC
		// get info about the device the file will be saved on
		Mono.Unix.Native.Statvfs stat = new Mono.Unix.Native.Statvfs();
		Mono.Unix.Native.Syscall.statvfs(path, out stat);
		long freeSpace = (long)(stat.f_bavail * stat.f_bsize);
			
		return freeSpace;
#else
		throw new NotImplementedException();
#endif
	}
}
