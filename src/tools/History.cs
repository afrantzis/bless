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
using System.Collections;
using System.Xml;
using System.IO;

namespace Bless.Tools {

///<summary>
/// A class that handles recently used files history.
///</summary>
public class History
{

	Queue files;
	int maxSize;
	
	///<summary>The file in the history</summary>
	public Queue Files {
		get { return files; }
	}
	
	///<summary>The maximum size of the history</summary>
	public int MaxSize {
		get { return maxSize; }
	}
	
	///<summary>Create history with maximum size 'n'</summary>
	public History(int n)
	{
		files=new Queue(n);
		maxSize=n;
	}
	
	///<summary>Add a file to the history</summary>
	public void Add(string path)
	{
		if (files.Contains(path)) {
			Queue tmp=new Queue(maxSize);
			foreach(string s in files) {
				if (s!=path)
					tmp.Enqueue(s);
			}
			tmp.Enqueue(path);
			files=tmp;
		}
		else {
			if (files.Count==maxSize)
				files.Dequeue();
			files.Enqueue(path);
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
		
		foreach(string file in files) {
			xml.WriteStartElement(null, "file", null);
				xml.WriteString(file);
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
