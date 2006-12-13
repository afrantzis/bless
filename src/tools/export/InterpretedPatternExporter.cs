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
using System.Text;
using System.Collections.Generic;
using System.Collections;
using Bless.Buffers;
using Bless.Util;

namespace Bless.Tools.Export
{

///<summary>
/// Exports a range of data to various formats
/// using a specific pattern which is constantly
/// interpreted
///</summary>	
public class InterpretedPatternExporter : IPatternExporter
{
	IExportBuilder builder;
	
	string pattern;
	int patternIndex;
	
	char characterData;
	string stringData;
	
	Stack<long> positionStack;
	
	long rangeStart;
	long rangeEnd;
	long bufPos;
	IBuffer buffer;
	
	enum Token { LeftBracket, RightBracket, Percent, Character, String, Error, End }
	
	public InterpretedPatternExporter(IExportBuilder ieb)
	{
		builder = ieb;
		positionStack = new Stack<long>();
	}

	public void Export(IBuffer buf, long start, long end, ref bool cancelled)
	{
		buffer = buf;
		rangeStart = start;
		rangeEnd = end;
		bufPos = rangeStart;
		
		long prevLoop = bufPos;
		bool finished = false;
		builder.BuildAlignment(1);
		Hashtable cmds = new Hashtable();
		
		builder.BuildPrologue();
		
		while (!finished && !cancelled) {
			
			Token tok = NextToken();
			switch(tok) {
				case Token.LeftBracket:
					SavePosition(bufPos);
					break;
				case Token.RightBracket:
					bufPos = RestorePosition();
					break;
				case Token.Percent:
					cmds.Clear();
					// try to parse a command
					tok = ParseCommand(cmds);
					// if we ended at another '%' parse was succesful
					if (tok == Token.Percent)
						bufPos += ExecuteCommand(cmds);
					else // otherwise mark an error
						tok = Token.Error;
					break;
				case Token.Character:
					builder.BuildCharacter(characterData);
					break;
				case Token.String:
					builder.BuildString(stringData);
					break;
				case Token.End:
					if (bufPos <= rangeEnd && prevLoop < bufPos) {
						prevLoop = bufPos;
						RestartInterpreter();
					}
					// if we reached the end of the pattern and we haven't moved forwards
					// in the file, this is never going to end!
					else if (prevLoop >= bufPos && bufPos <= rangeEnd) 
						throw new FormatException(string.Format("Pattern causes infinite loop"));
					else 
						finished = true;
					break;
				default:
					break;
			}
			
			if (tok == Token.Error)
				throw new FormatException(string.Format("Error at format position {0}", patternIndex));
		}
		
		if (!cancelled)
			builder.BuildEpilogue();
	}
	
	public IExportBuilder Builder {
		get { return builder; }
	}
	
	public long CurrentPosition {
		get { return bufPos; }
	}
	
	public string Pattern {
		get { return pattern; }
		set { pattern = value; }
	}
	
	
	
	///<summary>
	/// Execute the command described by the cmds hashtable
	///</summary>
	private int ExecuteCommand(Hashtable cmds)
	{
		
		// Export command
		if (cmds.Contains('E')) {
			BuildBytesInfo bbi = new BuildBytesInfo();
			bbi.Count = Convert.ToInt32(cmds['E'] as string);
			if (cmds.Contains('p'))
				bbi.Prefix = cmds['p'] as string;
			if (cmds.Contains('s'))
				bbi.Suffix = cmds['s'] as string;
			if (cmds.Contains('t'))
				bbi.Type = (cmds['t'] as string)[0];
			if (cmds.Contains('x'))
				bbi.Separator = cmds['x'] as string;
			if (cmds.Contains('e'))
				bbi.Empty = cmds['e'] as string;
			bbi.Commands = cmds;
			return builder.BuildBytes(buffer, bufPos, bbi);
		}
		
		// Ignore command
		if (cmds.Contains('I')) {
			return Convert.ToInt32(cmds['I'] as string);
		}
		
		// Offset command
		if (cmds.Contains('O')) {
			builder.BuildOffset(bufPos, Convert.ToInt32(cmds['O'] as string));
			return 0;
		}
		
		// Alignment command
		if (cmds.Contains('A')) {
			builder.BuildAlignment(Convert.ToInt32(cmds['A'] as string));
			return 0;
		}
		
		
		return 0;
	}
	
	private void SavePosition(long cur)
	{
		positionStack.Push(cur);
	}
	
	private long RestorePosition()
	{
		return positionStack.Pop();
	}
	
	
	private void RestartInterpreter()
	{
		patternIndex = 0;
	}
	
	private Token NextToken()
	{
		if (patternIndex >= pattern.Length)
			return Token.End;
		
		Token tok = Token.Error;
		
		switch(pattern[patternIndex]) {
			case '[':
				patternIndex++;
				tok = Token.LeftBracket;
				break;
			case ']':
				patternIndex++;
				tok = Token.RightBracket;
				break;
			case '%':
				patternIndex++;
				tok = Token.Percent;
				break;
			case '"':
				tok = ParseString();
				break;
			case '\\':
				tok = ParseEscapedCharacter();
				break;
			default:
				characterData = pattern[patternIndex];
				patternIndex++;
				tok = Token.Character;
				break;
		}
		
		return tok;
	}
	
	private Token ParseString()
	{
		StringBuilder sb = new StringBuilder();
		
		patternIndex++;
		bool finished = false;
		while (patternIndex < pattern.Length && !finished) {
			
			switch(pattern[patternIndex]) {
				case '\\':
					Token tok = ParseEscapedCharacter();
					if (tok == Token.Error)
						return Token.Error;
					else
						sb.Append(characterData);
					break;
				case '"':
					finished = true;
					patternIndex++;
					break;
				default:
					sb.Append(pattern[patternIndex]);
					patternIndex++;
					break;
			}
			
		}
		
		if (!finished)
			return Token.Error;
			
		stringData = sb.ToString();
		
		return Token.String;
	}
	
	private Token ParseEscapedCharacter()
	{
		Token ret = Token.Error;
		
		if (patternIndex + 1 < pattern.Length) {
			switch (pattern[patternIndex + 1]) {
				case 'n':
					characterData = '\n';
					ret = Token.Character;
					break;
				case 't':
					characterData = '\t';
					ret = Token.Character;
					break;
				default:
					characterData = pattern[patternIndex + 1];
					ret = Token.Character;
					break;
			}
		}
		else
			ret = Token.Error;
		
		patternIndex += 2;
		
		return ret;
	}
	
	///<summary>
	/// Parse a pattern command eg %E"4"p"0x"% 
	///</summary>
	private Token ParseCommand(Hashtable cmds)
	{
		bool finished = false;
		
		Token tok = Token.Error;
		char currentChar = ' ';
		
		while (!finished) {
			
			tok = NextToken();
			switch(tok) {
				case Token.Percent:
					finished = true;
					break;
				case Token.Character:
					currentChar = characterData;
					cmds[currentChar] = string.Empty;
					break;
				case Token.String:
					cmds[currentChar] = stringData;
					break;
				case Token.End:
					finished = true;
					break;
				default:
					tok = Token.Error;
					finished = true;
					break;
			}
			
		}
		
		return tok;
	}
	
}

}
