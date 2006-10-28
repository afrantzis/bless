// created on 2/20/2006 at 5:30 PM
using System.Collections;

namespace BlessBuilder
{
	
public class Module
{		
	private string name;
	private bool upToDate;		
	private ArrayList dependencies;
	private string type;
	private string dir;
	private string outputFile;
	private string extra;
	private ArrayList inputFiles;
	private ArrayList packages;
	private ArrayList references;
	
	public string Name {
		get { return name; }
		set { name=value; }
	}
	
	public ArrayList Dependencies {
		get { return dependencies; }
	}
	
	public ArrayList InputFiles {
		get { return inputFiles; }
	}
	
	public ArrayList Packages {
		get { return packages; }
	}
	
	public ArrayList References {
		get { return references; }
	}
	
	public bool UpToDate {
		get { return upToDate; }
		set { upToDate=value; }
	}
	
	public string Type {
		get { return type; }
		set { type=value; }
	}
	
	public string Dir {
		get { return dir; }
		set { dir=value; }
	}	
	
	public string OutputFile {
		get { return outputFile; }
		set { outputFile=value; }
	}
	
	public string Extra {
		get { return extra; }
		set { extra=value; }
	}
	
	public Module(string name)
	{
		this.name=name;
		dependencies=new ArrayList();
		inputFiles=new ArrayList();
		packages=new ArrayList();
		references=new ArrayList();

		upToDate=false;
	}
		
		
}
	
}