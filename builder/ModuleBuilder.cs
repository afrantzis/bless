// created on 2/20/2006 at 7:09 PM
using System.Diagnostics;
using System.Text;
using System.IO;
using System.Collections.Generic;

namespace BlessBuilder {

public enum BuildStatus { UpToDate, Rebuilt, Failed };

public class ModuleDependencyException : System.Exception
{
	public ModuleDependencyException(string msg)
			: base(msg)
	{ }
}

public class ModuleBuilder
{
	private ModuleTree moduleTree;
	private List<string> extraOptions;
	private List<string> modulesVisited;

	public ModuleBuilder(ModuleTree moduleTree)
	{
		this.moduleTree = moduleTree;
		this.extraOptions = new List<string>();
		this.modulesVisited = new List<string>();
	}

	public void AddOption(string option)
	{
		extraOptions.Add(option);
	}
	
	private BuildStatus BuildDeps(Module mod, StringBuilder cmdRefs)
	{
		BuildStatus status = BuildStatus.UpToDate;
		modulesVisited.Add(mod.Name);
		
		foreach(Module dep in mod.Dependencies) {
			BuildStatus depStatus = Build(dep);

			if (depStatus == BuildStatus.Failed) {
				modulesVisited.Remove(mod.Name);
				return BuildStatus.Failed;
			}
			else if (depStatus == BuildStatus.UpToDate) { }
			else if (depStatus == BuildStatus.Rebuilt) {
				status = BuildStatus.Rebuilt;
			}
			string depOutput = moduleTree.GetOutputFile(dep);
			cmdRefs.Append("-r:" + depOutput + " ");
		}
		
		modulesVisited.Remove(mod.Name);
		return status;
	}

	public BuildStatus Build(string name)
	{
		Module mod = moduleTree.FindModule(name);
		if (mod == null)
			return BuildStatus.Failed;

		return Build(mod);
	}

	public BuildStatus Build(Module module)
	{
		// detect cyclic dependencies
		if (modulesVisited.Contains(module.Name)) {
			modulesVisited.Add(module.Name);
			string sa = "Cyclic dependency detected: ";
			foreach (string s in modulesVisited)
				sa += "->" + s;
			throw new ModuleDependencyException(sa);
		}
		
		StringBuilder sb = new StringBuilder();

		BuildStatus depStatus = BuildDeps(module, sb);
		
		// if this an empty (dummy) module
		if (module.Dir == null)
			return depStatus;
		
		string output = moduleTree.GetOutputFile(module);

		sb.Append("-out:" + output);
		sb.Append(" -target:" + module.Type + " " );

		if (module.UpToDate && (depStatus == BuildStatus.UpToDate)) {
			//System.Console.WriteLine("{0} Already Built", module.Name);
			return BuildStatus.UpToDate;
		}

		foreach(string s in module.References) {
			sb.Append("-r:" + s + " ");
		}

		foreach(string s in module.Packages) {
			sb.Append("-pkg:" + s + " ");
		}

		foreach(string s in extraOptions) {
			sb.Append(s + " ");
		}

		foreach(string s in module.InputFiles) {
			sb.Append('"');
			sb.Append(s);
			sb.Append('"');
			sb.Append(' ');
		}

		sb.Append(module.Extra);

		//System.Console.WriteLine("gmcs {0}", sb.ToString());
		System.Console.WriteLine(">> Building module {0}...", module.Name);

		Process buildProcess = Process.Start("gmcs", sb.ToString());
		buildProcess.WaitForExit();

		if (buildProcess.ExitCode == 0) {
			module.UpToDate = true;
			System.Console.WriteLine("Ok");
			return BuildStatus.Rebuilt;
		}
		else {
			module.UpToDate = false;
			System.Console.WriteLine("Failed");
			System.Environment.Exit(1);
			return BuildStatus.Failed;
		}
	}
}


}