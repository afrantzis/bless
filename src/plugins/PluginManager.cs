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
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Reflection;
using System.IO;
using System.Globalization;
using Bless.Util;

namespace Bless.Plugins
{

public class PluginDependencyException : Exception
{
	public PluginDependencyException(string msg)
			: base(msg)
	{ }
}

public class PluginManager
{
	Dictionary<string, Plugin> plugins;

	Type[] ctorArgTypes;
	object[] ctorArgs;
	Type pluginType;

	private PluginManager(Type pluginType, object[] args)
	{
		plugins = new Dictionary<string, Plugin>();
		this.pluginType = pluginType;

		ctorArgTypes = new Type[args.Length];
		ctorArgs = args;

		for (int i = 0; i < ctorArgs.Length; i++)
			ctorArgTypes[i] = ctorArgs[i].GetType();

		// find system-wide plugins
		string systemPluginDir = FileResourcePath.GetBinPath();
		string[] systemPluginFiles = Directory.GetFiles(systemPluginDir);
		CompareInfo compare = CultureInfo.InvariantCulture.CompareInfo;

		foreach (string file in systemPluginFiles) {
			if (compare.IndexOf(file, "plugin", CompareOptions.IgnoreCase) >= 0
					&& file.EndsWith(".dll")) {
				//Console.WriteLine("Searching File {0}", file);
				AddPluginFile(file);
			}
		}

		try {
			// find local user plugins
			string userPluginDir = FileResourcePath.GetUserPath("plugins");
			string[] userPluginFiles = Directory.GetFiles(userPluginDir);

			foreach (string file in userPluginFiles) {
				if (compare.IndexOf(file, "plugin", CompareOptions.IgnoreCase) >= 0
						&& file.EndsWith(".dll")) {
					//Console.WriteLine("Searching File {0}", file);
					AddPluginFile(file);
				}
			}
		}
		catch (DirectoryNotFoundException e) {
			System.Console.WriteLine(e.Message);
		}

	}

	private void AddPluginFile(string file)
	{
		try {
			Assembly asm = Assembly.LoadFile(file);
			Type[] types = asm.GetTypes();

			foreach(Type t in types) {
				if (t.BaseType == pluginType) {
					//Console.WriteLine("    Found Type {0}", t.FullName);
					ConstructorInfo ctor = t.GetConstructor(ctorArgTypes);
					AddToPluginCollection((Plugin)ctor.Invoke(ctorArgs));
				}
			}
		}
		catch (Exception e) {
			System.Console.WriteLine(e.Message);
		}

	}

	private void AddToPluginCollection(Plugin plugin)
	{
		plugins.Add(plugin.Name, plugin);
		//System.Console.WriteLine("Added plugin {0}", plugin.Name);
	}

	public bool LoadPlugin(Plugin plugin)
	{
		StringCollection visited = new StringCollection();
		return LoadPluginInternal(plugin, visited);
	}

	private bool LoadPluginInternal(Plugin plugin, StringCollection visited)
	{
		visited.Add(plugin.Name);
		if (plugin.Loaded)
			return true;

		foreach(string dep in plugin.Dependencies) {
			if (visited.Contains(dep))
				throw new PluginDependencyException("Cyclic dependency detected!");
			if (!plugins.ContainsKey(dep))
				throw new PluginDependencyException(string.Format("Cannot find plugin '{0}' needed by '{1}'", dep, plugin.Name));

			if (LoadPluginInternal(plugins[dep], visited) == false)
				return false;
		}

		foreach(string la in plugin.LoadAfter) {
			if (visited.Contains(la))
				throw new PluginDependencyException("Cyclic LoadAfter association detected!");
			if (plugins.ContainsKey(la))
				LoadPluginInternal(plugins[la], visited);
		}


		return plugin.Load();
	}

	public Plugin[] Plugins {
		get {
			Plugin[] pa = new Plugin[plugins.Count];
			int i = 0;
			foreach (Plugin p in plugins.Values)
				pa[i++] = p;

			return pa;
		}

	}

	static Dictionary<Type, PluginManager> pluginManagers = new Dictionary<Type, PluginManager>(); 
	
	public static void AddForType(Type pluginType, object[] args)
	{
		PluginManager pm = new PluginManager(pluginType, args);
		pluginManagers[pluginType] = pm;
	}
	
	public static PluginManager GetForType(Type pluginType)
	{
		PluginManager ret = null;
		
		if (pluginManagers.ContainsKey(pluginType)) {
			ret = pluginManagers[pluginType];
		}
		
		return ret;
	}

	public static IEnumerable<KeyValuePair<Type, PluginManager>> AllManagers {
		get { return pluginManagers; }
	}
}

}
