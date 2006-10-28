// created on 1/13/2005 at 10:59 AM
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
using System.Text;
 
namespace Bless.Util {

///<summary>Create byte arrays from various kinds of strings</summary>
public class ByteArray
{
	// Don't allow instantiation
	private ByteArray() { }
		
	static public byte[] FromString(string s, int baseNum)
	{
		int i=0;
		int len=s.Length;
		byte[] ba=new byte[(len/BaseConverter.MinDigits[baseNum])+1];
		int j=0;
		
		// ignore leading whitespace
		while (i<len && s[i]==' ') i++;
		
		while (i < len) {
			int k=1;
			while (i+k < len &&  s[i+k]!=' ' && k < BaseConverter.MinDigits[baseNum]) k++;
			
			ba[j++]=(byte)BaseConverter.ConvertToNum(s, i, i+k-1, baseNum);
			
			// skip spaces
			i=i+k;		
			while (i<len && s[i]==' ') i++;
		}
		
		byte[] baRet=new byte[j];
		
		Array.Copy(ba, baRet, j);
		
		return baRet;
	}
	
	static public string ToString(byte[] ba, int baseNum)
	{
		if (ba.Length==0)
			return string.Empty;
		
		StringBuilder sb=new StringBuilder(ba.Length*(BaseConverter.MinDigits[baseNum]+1));
		
		foreach(byte b in ba) {
			sb.Append(BaseConverter.ConvertToString(b, baseNum));
			sb.Append(' ');
		}
		
		return sb.ToString(0, sb.Length-1);
	}
	
} //end ByteArray

} // end namespace