// created on 2/20/2006 at 5:43 PM
using System;
using System.Collections;
using System.Xml;
using System.IO;

namespace BlessBuilder
{
	
public class ModuleTree
{
	internal Hashtable modules;
	internal string baseDir;
	internal string outputDir;
	
	public ModuleTree(string filename)
	{
		modules=new Hashtable();
		XmlDocument xmlDoc=new XmlDocument();
		xmlDoc.Load(filename);
		baseDir=Path.GetDirectoryName(Path.GetFullPath(filename));
		ParseXml(xmlDoc);
	}
	
	private void ParseXml(XmlDocument xmlDoc)
	{
		XmlNodeList optionList=xmlDoc.GetElementsByTagName("option");
		
		foreach(XmlNode optionNode in optionList) {
			if (optionNode.Attributes["name"].Value=="outputdir")
				outputDir=optionNode.InnerText;
			//System.Console.WriteLine("Option {0} = {1}", optionNode.Attributes["name"].Value,optionNode.InnerText);
		}
		
		XmlNodeList moduleList=xmlDoc.GetElementsByTagName("module");
		
		foreach(XmlNode moduleNode in moduleList) {
			ParseModule(moduleNode);
		}
	}
	
	private void ParseModule(XmlNode moduleNode)
	{
		Module module=FindModule(moduleNode.Attributes["name"].Value);
		if (module==null) {
			module=new Module(moduleNode.Attributes["name"].Value);
			modules.Add(module.Name, module);
		}
		//System.Console.WriteLine("Module: {0}", moduleNode.Attributes["name"].Value);
		XmlNodeList children=moduleNode.ChildNodes;
		
		foreach(XmlNode childNode in children) {
			if (childNode.NodeType==XmlNodeType.Element && childNode.LocalName=="depends") {
				//System.Console.WriteLine("    Depends on: {0}", childNode.InnerText);
				Module dep=FindModule(childNode.InnerText);
				if (dep==null) {
					dep=new Module(childNode.InnerText);
					modules.Add(dep.Name, dep);
				}
				module.Dependencies.Add(dep);
			}
			else if (childNode.NodeType==XmlNodeType.Element && childNode.LocalName=="dir") {
				module.Dir=GetNewPath(baseDir, childNode.InnerText);
			}
		}
		
		ParseBuildInfo(module);
	}
	
	private void ParseBuildInfo(Module module)
	{ 
		string biPath=Path.Combine(module.Dir, module.Name)+".bi";
		XmlDocument xmlDoc=new XmlDocument();
		xmlDoc.Load(biPath);
		
			
		// get input files
		XmlNodeList inputList=xmlDoc.GetElementsByTagName("input");
		
		DateTime maxWriteTime=ParseInputFiles(inputList, module);
		
		
			
		// get pkgs
		XmlNodeList pkgList=xmlDoc.GetElementsByTagName("package");
		
		foreach(XmlNode pkgNode in pkgList) {
			module.Packages.Add(pkgNode.InnerText);
		}
		
		// get refs
		XmlNodeList refList=xmlDoc.GetElementsByTagName("reference");
		
		foreach(XmlNode refNode in refList) {
			module.References.Add(refNode.InnerText);
		}
		
		// get extra
		XmlNodeList extraList=xmlDoc.GetElementsByTagName("extra");
		
		foreach(XmlNode extraNode in extraList) {
			module.Extra+=" "+extraNode.InnerText;
		}
		
		// get type
		XmlNodeList typeList=xmlDoc.GetElementsByTagName("type");
		
		foreach(XmlNode typeNode in typeList) {
			module.Type=typeNode.InnerText;
		}
		
		// get output
		XmlNodeList outList=xmlDoc.GetElementsByTagName("output");
		
		foreach(XmlNode outNode in outList) {
			module.OutputFile=outNode.InnerText;
		}
		
		FileInfo output=new FileInfo(GetOutputFile(module));
		
		FileInfo bi=new FileInfo(biPath);
		if (!output.Exists || output.LastWriteTime <= maxWriteTime || bi.LastWriteTime > output.LastWriteTime) {
			module.UpToDate=false;
		}
		else
			module.UpToDate=true;
		
	}
	
	private DateTime ParseInputFiles(XmlNodeList inputList, Module module)
	{
		DateTime maxWriteTime=new DateTime(0);
		
		
		foreach(XmlNode inputNode in inputList) {
			FileInfo[] fiArray;
			string fileDir=Path.GetDirectoryName(inputNode.InnerText);
			string filePattern=Path.GetFileName(inputNode.InnerText);
			DirectoryInfo di=new DirectoryInfo(GetNewPath(module.Dir, fileDir));
			
			fiArray=di.GetFiles(filePattern);
			
			foreach (FileInfo fi in fiArray) {
				module.InputFiles.Add(fi.FullName);
				if (fi.LastWriteTime > maxWriteTime)
					maxWriteTime=fi.LastWriteTime;
				//System.Console.WriteLine("    Input File: {0}", fi.FullName);
			}
		}
		
		return maxWriteTime;
	}
	
	public Module FindModule(string name)
	{
		return (Module)modules[name];
	}
	
	public string GetOutputFile(Module module)
	{
		if (outputDir==null)
			return Path.Combine(module.Dir, module.OutputFile);
		else
			return Path.Combine(this.outputDir, module.OutputFile);
	}
	
	public string GetNewPath(string currentDir, string newPath)
	{
		if (Path.IsPathRooted(newPath))
			return newPath;
		else
			return Path.Combine(currentDir, newPath);
	}
}

} //end namespace