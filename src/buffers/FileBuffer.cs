// created on 6/5/2004 at 5:04 PM
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
using System.IO;
using System;

namespace Bless.Buffers {

public class FileBuffer: IBuffer {

	long winOffset;
	int winOccupied;
	byte[] window;
	BinaryReader reader; 
	long FileLength;
	const int default_size=4096; 
	
	
	public FileBuffer(string fn): this(fn,default_size) { }
	public FileBuffer(string fn, int size) 
	{
		window=new byte[size];
		Load(fn);
	}

	private bool InWindow(long pos) 
	{
		return (pos >= winOffset && pos < winOffset + winOccupied);
	}
	
	public void Insert(long pos, byte[] data) { /*read only buffer*/}
	
	public int Read(byte[] ba, long pos, int len) 
	{
		// bounds checking
		if (pos >= FileLength || pos<0)
			return 0;
		if (pos + len > FileLength)
			len = (int)(FileLength-pos+1); 

		reader.BaseStream.Seek(pos, SeekOrigin.Begin);
		reader.Read(ba, 0, len);
		
		// seek back to previous position
		//reader.BaseStream.Seek(winOffset, SeekOrigin.Begin);
		return len;
	}
	
	public int GetIndex(long pos, int len) { return 0;}
	
	public void Append(byte[] data) { /*read only buffer*/}
	
	public void Append(byte data){ /*read only buffer*/}
	
	public byte this[long index] {
		set {/*read only buffer*/ }
		get {
			
			if (!InWindow(index)) {
				if (index >= FileLength) {
					throw new IndexOutOfRangeException();
				}	
				winOffset=index-window.Length/2;
				if (winOffset<0)
					winOffset=0;
				reader.BaseStream.Seek(winOffset, SeekOrigin.Begin);
				winOccupied=reader.Read(window, 0, window.Length);
			}
			
			return window[index-winOffset];
			
		}
	}
	

	public long Size {
		get { return FileLength; }
	}
	
	public void Load(string filename) 
	{
		FileStream fs = new FileStream(filename, FileMode.Open, FileAccess.Read);
		FileLength=fs.Length;
		reader=new BinaryReader(fs);
		
		winOccupied=reader.Read(window, 0, window.Length);
		winOffset=0;
	}

	public string Filename {
		get {	if (reader != null) 
					return (reader.BaseStream as FileStream).Name;
				else
					return null;
			}
	}
	
	public void Close()
	{
		reader.Close();
		reader=null;
	}
	
}

} // end namespace
