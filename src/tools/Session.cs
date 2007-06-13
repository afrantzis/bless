// created on 4/15/2005 at 11:44 AM
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
using System.Collections;
using System.Xml;
using System.IO;

namespace Bless.Tools {

///<summary>
/// Handles the saving and loading of sessions
/// (files that describe the current workspace)
///</summary>
public class Session
{
	int windowWidth;
	int windowHeight;
	string activeFile;

	ArrayList files;

	public ArrayList Files {
		get { return files; }
	}

	public int WindowWidth {
		get { return windowWidth;}
		set { windowWidth = value;}
	}

	public int WindowHeight {
		get { return windowHeight;}
		set { windowHeight = value;}
	}

	public string ActiveFile {
		get { return activeFile; }
		set { activeFile = value; }
	}

	public Session()
	{
		files = new ArrayList();
		activeFile = string.Empty;
	}

	public void AddFile(string name, long offset, long cursorOffset, int cursorDigit, string layout, int focusedArea)
	{
		string path = Path.GetFullPath(name);
		SessionFileInfo sfi = new SessionFileInfo(path, offset, cursorOffset, cursorDigit, layout, focusedArea);
		files.Add(sfi);
	}

	public void Save(string path)
	{
		XmlTextWriter xml = new XmlTextWriter(path, null);
		xml.Formatting = Formatting.Indented;
		xml.Indentation = 1;
		xml.IndentChar = '\t';

		xml.WriteStartElement(null, "session", null);

		xml.WriteStartElement(null, "windowheight", null);
		xml.WriteString(windowHeight.ToString());
		xml.WriteEndElement();

		xml.WriteStartElement(null, "windowwidth", null);
		xml.WriteString(windowWidth.ToString());
		xml.WriteEndElement();

		xml.WriteStartElement(null, "activefile", null);
		xml.WriteString(activeFile);
		xml.WriteEndElement();

		foreach(SessionFileInfo sfi in files) {
			xml.WriteStartElement(null, "file", null);
			xml.WriteStartElement(null, "path", null);
			xml.WriteString(sfi.Path);
			xml.WriteEndElement();
			xml.WriteStartElement(null, "offset", null);
			xml.WriteString(sfi.Offset.ToString());
			xml.WriteEndElement();
			xml.WriteStartElement(null, "cursoroffset", null);
			xml.WriteString(sfi.CursorOffset.ToString());
			xml.WriteEndElement();
			xml.WriteStartElement(null, "cursordigit", null);
			xml.WriteString(sfi.CursorDigit.ToString());
			xml.WriteEndElement();
			xml.WriteStartElement(null, "layout", null);
			xml.WriteString(sfi.Layout);
			xml.WriteEndElement();
			xml.WriteStartElement(null, "focusedarea", null);
			xml.WriteString(sfi.FocusedArea.ToString());
			xml.WriteEndElement();
			xml.WriteEndElement();
		}

		xml.WriteEndElement();
		xml.WriteEndDocument();
		xml.Close();
	}

	public void Load(string path)
	{
		XmlDocument xmlDoc = new XmlDocument();
		xmlDoc.Load(path);

		XmlNodeList fileList = xmlDoc.GetElementsByTagName("file");

		foreach(XmlNode fileNode in fileList) {
			XmlNodeList childNodes = fileNode.ChildNodes;
			SessionFileInfo sfi = new SessionFileInfo();
			foreach(XmlNode node in childNodes) {
				switch (node.Name) {
					case "path":
						sfi.Path = node.InnerText;
						break;
					case "offset":
						sfi.Offset = Convert.ToInt64(node.InnerText);
						break;
					case "cursoroffset":
						sfi.CursorOffset = Convert.ToInt64(node.InnerText);
						break;
					case "cursordigit":
						sfi.CursorDigit = Convert.ToInt32(node.InnerText);
						break;
					case "layout":
						sfi.Layout = node.InnerText;
						break;
					case "focusedarea":
						sfi.FocusedArea = Convert.ToInt32(node.InnerText);
						break;
					default:
						break;
				}
			}
			files.Add(sfi);
		}

		XmlNodeList heightList = xmlDoc.GetElementsByTagName("windowheight");
		foreach(XmlNode heightNode in heightList) {
			windowHeight = Convert.ToInt32(heightNode.InnerText);
		}

		XmlNodeList widthList = xmlDoc.GetElementsByTagName("windowwidth");
		foreach(XmlNode widthNode in widthList) {
			windowWidth = Convert.ToInt32(widthNode.InnerText);
		}

		XmlNodeList activeList = xmlDoc.GetElementsByTagName("activefile");
		foreach(XmlNode activeNode in activeList) {
			activeFile = activeNode.InnerText;
		}

	}
}

///<summary>
/// Holds information about a file as part of a session
///</summary>
public class SessionFileInfo
{
	public string Path;
	public long Offset;
	public long CursorOffset;
	public int CursorDigit;
	public string Layout;
	public int FocusedArea;

	public SessionFileInfo() { }

	public SessionFileInfo(string path, long offset, long cursorOffset, int cursorDigit, string layout, int focusedArea)
	{
		Path = path; Offset = offset; CursorOffset = cursorOffset; CursorDigit = cursorDigit; Layout = layout; FocusedArea = focusedArea;
	}
}

} // end namespace