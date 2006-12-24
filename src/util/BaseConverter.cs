// created on 2/19/2005 at 9:40 PM
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
using Mono.Unix;

namespace Bless.Util {

///<summary>Convert strings to numbers and vice-versa using a specified base</summary>
public class BaseConverter
{
	static public int[] DefaultMinDigits=new int[]{0, 0, 8, 6, 4, 4, 4, 3, 3, 3, 3, 3, 3, 3, 3, 3, 2};
	
	// Don't allow instantiation
	private BaseConverter() { }
	
	///<summary>Convinience method</summary>
	static public string ConvertToString(long num, int b)
	{
		return ConvertToString(num, b, false, false, 0);
	}
		
	///<summary>Convert a number to string using the specified base</summary>
	static public string ConvertToString(long num, int b, bool prependPrefix, bool lowercase, int minDigits)
	{
		// make sure base is valid
		if (b<2 || b>16)
			return null;
			
		StringBuilder sb=new StringBuilder(64);
		
		char alpha = lowercase ? 'a' : 'A';
		
		while (num > 0) {
			int rem=(int)(num%b);
			if (rem<10) 
				sb.Insert(0, (char)(rem+'0'));
			else
				sb.Insert(0, (char)(rem-10+alpha));
			num=num/b;
		}
		
		// if minDigits == 0 use a minimum number of digits
		// which depends on the base used
		if (minDigits == 0)
			minDigits = DefaultMinDigits[b];
		
		// pad number with zeroes, until it reaches the minimum length
		while (sb.Length < minDigits)
			sb.Insert(0, '0');
			
		// prepend a prefix to mark the number base
		// (for octal and hexadecimal bases)
		if (prependPrefix) {
			if (b==16)
				sb.Insert(0, "0x");
			else if (b==8 && sb[0]!='0')
				sb.Insert(0, '0');	
		}
		
		return sb.ToString();		
	}
	
	static private int CharToInt(char c, int b)
	{	
		int ret=-1;
		
		if (c>='0' && c <='9')
			ret=c-'0';
		else {
			c=Char.ToLower(c);
			if (c>='a' && c<='f')
				ret=c-'a'+10;
		}
		
		// if character is not valid or beyond base throw exception 
		if (ret >= b || ret==-1)
			throw new FormatException(String.Format(Catalog.GetString("Character '{0}' is not valid in a number of base {1}."), c, b));
		
		return ret;						
	}
	
	///<summary>Convert a string to a number using the specified base</summary>
	static public long ConvertToNum(string s, int startIndex, int endIndex, int b)
	{
		long val=0;
		int i=startIndex;
		
		while (i<=endIndex) {
			val = b*val + CharToInt(s[i], b);
			i++;	
		}
		
		return val;
	}
	
	static public long ConvertToNum(string s, int b)
	{
		return  ConvertToNum(s, 0, s.Length-1, b);
	}
	
	///<summary>Convert a string to a number guessing the base from the string format</summary>
	static public long Parse(string s)
	{
		// trim the spaces from the string
		string trimStr=s.Trim();
		
		int i=0;
		int len=trimStr.Length;
		
		if (len==0)
			throw new FormatException(Catalog.GetString("The string to parse is empty."));
			
		//decide if it is hex, octal or decimal
		
		// if it starts with '0x' suppose it is hex
		if (i+1<len && trimStr[i]=='0' && trimStr[i+1]=='x')
			return ConvertToNum(trimStr, i+2, trimStr.Length-1, 16);
		
		// if it starts with just '0' suppose it is octal 
		if (i<len && trimStr[i]=='0')
			return ConvertToNum(trimStr, 8);
		
		// else suppose it is decimal
		return ConvertToNum(trimStr, 10);
	}
}

} // end namespace
