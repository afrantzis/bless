// created on 2/20/2006 at 7:09 PM
using System.Diagnostics;
using System.Text;
using System.IO;
using System.Collections.Generic;

namespace BlessBuilder {

public enum BuildStatus { UpToDate, Rebuilt, Failed };

public class ModuleBuilder
{	
	private ModuleTree moduleTree;
	private List<string> extraOptions;
	
	public ModuleBuilder(ModuleTree moduleTree)
	{
		this.moduleTree=moduleTree;
		this.extraOptions = new List<string>();
	}
	
	public void AddOption(string option)
	{
		extraOptions.Add(option);
	}
	
	public BuildStatus Build(string name)
	{
		Module mod=moduleTree.FindModule(name);
		if (mod==null)
			return BuildStatus.Failed;
		
		return Build(mod);
	}
	
	public BuildStatus Build(Module module)
	{
		BuildStatus status=BuildStatus.UpToDate;
		
		string output=moduleTree.GetOutputFile(module);
		
		StringBuilder sb=new StringBuilder("-out:"+output);
		sb.Append(" -target:"+module.Type+ " " );
		
		// make sure dependencies are built
		foreach(Module dep in module.Dependencies) {
			BuildStatus depStatus=Build(dep);
			
			if (depStatus==BuildStatus.Failed)
				return BuildStatus.Failed;
			else if (depStatus==BuildStatus.UpToDate) { }
			else if (depStatus==BuildStatus.Rebuilt) {
				status=BuildStatus.Rebuilt;	
			}
			string depOutput=moduleTree.GetOutputFile(dep);
			sb.Append("-r:"+depOutput+" ");
		}
		
		if (module.UpToDate && status==BuildStatus.UpToDate) {
			//System.Console.WriteLine("{0} Already Built", module.Name);
			return BuildStatus.UpToDate;
		}
		
		foreach(string s in module.References) {
			sb.Append("-r:"+s+" ");
		}
		
		foreach(string s in module.Packages) {
			sb.Append("-pkg:"+s+" ");
		}
		
		foreach(string s in extraOptions) {
			sb.Append(s + " ");
		}
		
		foreach(string s in module.InputFiles) {
			sb.Append(s);
			sb.Append(' ');
		}
		
		sb.Append(module.Extra);
		
		//System.Console.WriteLine("mcs {0}", sb.ToString());
		System.Console.WriteLine(">> Building module {0}...", module.Name);
		
		Process buildProcess=Process.Start("gmcs", sb.ToString());
		buildProcess.WaitForExit();
		
		if (buildProcess.ExitCode==0) {
			module.UpToDate=true;
			System.Console.WriteLine("Ok");
			return BuildStatus.Rebuilt;
		}
		else {
			module.UpToDate=false;
			System.Console.WriteLine("Failed");
			System.Environment.Exit(1);
			return BuildStatus.Failed;
		}
	}
}


}