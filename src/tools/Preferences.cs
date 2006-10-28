// created on 5/24/2005 at 1:14 PM
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
using System.Xml;
using System.Collections;

namespace Bless.Tools {

public delegate void PreferencesChangedHandler(Preferences prefs);

///<summary>
/// A class that holds the application preferences (using singletons)
///</summary>
public class Preferences 
{
	Hashtable prefs;
	string autoSavePath;
	
	static Preferences instance=null;
	static Preferences defaultPrefs=null;
	static PreferencesProxy proxy=null;
	
	///<summary>
	/// The current preferences
	///</summary>
	static public Preferences Instance {
		get { 
			if (instance==null)
				instance=new Preferences();
			return instance;
		}
	}
	
	///<summary>
	/// The default preferences
	///</summary>
	static public Preferences Default {
		get { 
			if (defaultPrefs==null)
				defaultPrefs=new Preferences();
			return defaultPrefs;
		}
	}

	static public PreferencesProxy Proxy {
		get { 
			if (proxy==null)
				proxy=new PreferencesProxy(Preferences.Instance);
			return proxy;
		}
	}
	
	private Preferences() 
	{ 
		prefs=new Hashtable();
		autoSavePath=null;
		notifyWhenSetting=true;
	}
	
	///<summary>
	/// Get or Set the value of a preference 
	///</summary>
	public string this[string key] {
		get { 
			string s=(string)prefs[key];
			if (s==null)
				s=string.Empty;
			return s;
		}
		
		set {
			Console.WriteLine("Setting pref {0} <= {1}", key, value);
			prefs[key]=value;
			// save the preferences if autoSavePath is set
			// ignore exceptions
			try {
				if (autoSavePath!=null)
					Save(autoSavePath);
			}
			catch(Exception ex) { }
			
			if (notifyWhenSetting)
				Preferences.Proxy.Change(key, value, "__Preferences__");
			
		}
	}
	
	public string AutoSavePath {
		get { return autoSavePath; }
		set { autoSavePath=value; }	
	}
	
	public IEnumerator GetEnumerator() 
	{
		return prefs.GetEnumerator();
	}
	
	public void SetWithoutNotify(string pref, string val)
	{
		notifyWhenSetting=false;
		this[pref]=val;
		notifyWhenSetting=true;
	}
	
	///<summary>
	/// Save preferences to an Xml file 
	///</summary>
	public void Save(string path)
	{
		XmlTextWriter xml=new XmlTextWriter(path, null);
		xml.Formatting=Formatting.Indented;
		xml.Indentation=1;
		xml.IndentChar='\t';
		
		xml.WriteStartElement(null, "preferences", null);
		
		foreach (DictionaryEntry entry in prefs) {
			xml.WriteStartElement(null, "pref", null);
			xml.WriteStartAttribute(null, "name", null);
			xml.WriteString((string)entry.Key);
			xml.WriteEndAttribute();
			xml.WriteString((string)entry.Value);
			xml.WriteEndElement();
		}
        	
		
		xml.WriteEndElement();
		xml.WriteEndDocument();
		xml.Close();
	}
	
	///<summary>
	/// Load preferences from an Xml file 
	///</summary>
	public void Load(string path)
	{
		XmlDocument xmlDoc=new XmlDocument();
		xmlDoc.Load(path);
		
		XmlNodeList prefList = xmlDoc.GetElementsByTagName("pref");
		
		foreach(XmlNode prefNode in prefList) {
			XmlAttributeCollection attrColl = prefNode.Attributes;
			string name=attrColl["name"].Value;
			prefs[name]=prefNode.InnerText;
		}
	}

	///<summary>
	/// Load preferences from another Preferences instance 
	///</summary>
	public void Load(Preferences p)
	{
		if (p!=null) {
			foreach (DictionaryEntry entry in p)
				prefs[entry.Key]=entry.Value;
		}
	}
	
	///<summary>
	/// Display preferences 
	///</summary>
	public void Display()
	{
		foreach (DictionaryEntry entry in prefs) {
			System.Console.WriteLine("[{0}]: {1}", entry.Key, entry.Value);
		}
	}
	
	private bool notifyWhenSetting;
}

public class PreferencesProxy
{
	Hashtable prefSubscribers;
	Preferences prefs;
	Hashtable currentlyHandling;
	
	public PreferencesProxy(Preferences prefs)
	{
		prefSubscribers=new Hashtable();
		currentlyHandling=new Hashtable();
		this.prefs=prefs;
		this.enable=true;
	}
	
	
	public void Subscribe(string pref, string id, PreferencesChangedHandler handler)
	{
		if (!prefSubscribers.Contains(pref))
			prefSubscribers[pref] = new Hashtable();
			
		(prefSubscribers[pref] as Hashtable).Add(id, handler);
	}

	public void Unsubscribe(string pref, string id)
	{
		if (!prefSubscribers.Contains(pref))
			return;
			
		(prefSubscribers[pref] as Hashtable).Remove(id);
		
	}
	
	public void Change(string pref, string val, string id)
	{
		if (enable==false)
			return;
			
		Console.WriteLine("Change 1: ");
		foreach (DictionaryEntry hand in (currentlyHandling as Hashtable)) {
			Console.Write("{0} ", hand.Key);
		}
		Console.WriteLine("");
		
		
		Console.WriteLine("Change 2");
		
		
		if (currentlyHandling.Contains(pref))
			return;
		
		if (id!="__Preferences__") {
			prefs.SetWithoutNotify(pref, val);	
		}
		
		if (!prefSubscribers.Contains(pref))
			return;
		
		
		currentlyHandling.Add(pref, null);
		
		foreach(DictionaryEntry subscriber in (prefSubscribers[pref] as Hashtable))
			if (subscriber.Key!=id) {
				Console.WriteLine("Sending pref {0}:{1} to {2} ({3})", pref, val, subscriber.Key, id);
				(subscriber.Value as PreferencesChangedHandler)(prefs);}
		
		currentlyHandling.Remove(pref);	
	}
	
	public void NotifyAll()
	{
		if (enable==false)
			return;
		
		foreach(DictionaryEntry prefSub in prefSubscribers) {
			currentlyHandling.Add(prefSub.Key, null);
			foreach(DictionaryEntry subscriber in (prefSub.Value as Hashtable)) 
				(subscriber.Value as PreferencesChangedHandler)(prefs);
			currentlyHandling.Remove(prefSub.Key);
		}
	}
	
	bool enable;
		
	///<summary>
	/// Enable or disable emission of the Changed event 
	///</summary>
	public bool Enable {
		get { return enable; }
		set { enable=value; }
	} 
	
}

} // end namespace
