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
using System.Collections;
using System.Reflection;
using System.IO;
using Bless.Util;

namespace Bless.Plugins
{

public class PluginManager
{
	ArrayList plugins;
	Type[] ctorArgTypes;
	object[] ctorArgs;
	Type pluginType;
	
	public PluginManager(Type pluginType, object[] args)
	{
		plugins=new ArrayList();
		this.pluginType=pluginType;
		
		ctorArgTypes=new Type[args.Length];
		ctorArgs=args;
		
		for (int i=0; i < ctorArgs.Length; i++)
			ctorArgTypes[i]= ctorArgs[i].GetType();
		
		// find system-wide plugins
		string systemPluginDir=FileResourcePath.GetSystemPath("");
		string[] systemPluginFiles=Directory.GetFiles(systemPluginDir);
		
		foreach (string file in systemPluginFiles) {
			//Console.WriteLine("Searching File {0}", file);
			AddPluginFile(file);
		}
		
		try {
			// find local user plugins
			string userPluginDir=FileResourcePath.GetUserPath("plugins");
			string[] userPluginFiles=Directory.GetFiles(userPluginDir);
			
			foreach (string file in userPluginFiles) {
				//Console.WriteLine("Searching File {0}", file);
				AddPluginFile(file);
			}
		}
		catch(DirectoryNotFoundException e) { }
	}
	
	private void AddPluginFile(string file)
	{
		try {
			Assembly asm=Assembly.LoadFile(file);
			Type[] types = asm.GetTypes();
			
			foreach(Type t in types) {
				if (t.BaseType==pluginType) {
					//Console.WriteLine("    Found Type {0}", t.FullName);
					ConstructorInfo ctor=t.GetConstructor(ctorArgTypes);
					plugins.Add(ctor.Invoke(ctorArgs));
				}
			}
		}
		catch(Exception e) { }
	
	}
	
	public Plugin[] Plugins {
		get {
			Plugin[] pa=new Plugin[plugins.Count];
			int i=0;
			foreach (Plugin p in plugins)
				pa[i++]=p;
		
			return pa;
		}
			
	}
	
	public Plugin GetPlugin()
	{
		return null;	
	}
}
	
}
