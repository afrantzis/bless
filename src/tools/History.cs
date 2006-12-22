// created on 4/20/2005 at 2:14 PM
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
using System.Collections.Generic;
using System.Xml;
using System.IO;

namespace Bless.Tools {

///<summary>
/// A class that handles recently used files history (singleton)
///</summary>
public class History
{

	List<string> files;
	int maxSize;
	static History instance;
	
	///<summary>
	/// The current history
	///</summary>
	static public History Instance {
		get { 
			if (instance==null)
				instance=new History(5);
			return instance;
		}
	}
	
	///<summary>The file in the history</summary>
	public List<string> Files {
		get { return files; }
	}
	
	///<summary>The maximum size of the history</summary>
	public int MaxSize {
		get { return maxSize; }
	}
	
	///<summary>Create history with maximum size 'n'</summary>
	private History(int n)
	{
		files=new List<string>(n);
		maxSize=n;
	}
	
	///<summary>Add a file to the history</summary>
	public void Add(string path)
	{
		if (files.Remove(path) == true) {
			files.Insert(0, path);
		}
		else {
			if (files.Count == maxSize)
				files.RemoveAt(files.Count - 1);
			files.Insert(0, path);
		}
		
		if (Changed!=null)
			Changed(this);
	}
	
	///<summary>Load history from an xml file</summary>
	public void Load(string path)
	{
		XmlDocument xmlDoc=new XmlDocument();
		xmlDoc.Load(path);
		
		XmlNodeList fileList = xmlDoc.GetElementsByTagName("file");
		
		foreach(XmlNode fileNode in fileList) {
			this.Add(fileNode.InnerText);	
		}
	}
	
	///<summary>Save history to an xml file</summary>
	public void Save(string path)
	{
		XmlTextWriter xml=new XmlTextWriter(path, null);
		xml.Formatting=Formatting.Indented;
		xml.Indentation=1;
		xml.IndentChar='\t';
		
		xml.WriteStartDocument(true);
		
		xml.WriteStartElement(null, "history", null);
		
		// save them in reverse order
		// so that they are loaded normally
		for(int i = files.Count - 1; i>=0 ; i--) {
			xml.WriteStartElement(null, "file", null);
				xml.WriteString(files[i]);
			xml.WriteEndElement();
		}
		
		xml.WriteEndElement();
		xml.WriteEndDocument();
		xml.Close();
	}
	
	public delegate void HistoryChangedHandler(History his);
	
	//<summary>Emitted when history changes</summary>
	public event HistoryChangedHandler Changed;
}

} // end namespace
