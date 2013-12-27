using System.Reflection;
using JetBrains.ActionManagement;
using JetBrains.Application.PluginSupport;

// General Information about an assembly is controlled through the following 
// set of attributes. Change these attribute values to modify the information
// associated with an assembly.
[assembly: AssemblyTitle("ReSharper.LocateFileInSolutionExplorer")]
[assembly: AssemblyDescription("Locate any file or folder by name in the solution explorer, without opening it. \n" +
                               "Invoked via double Alt-Shift-L.")]
[assembly: AssemblyConfiguration("")]
[assembly: AssemblyCompany("Igal Tabachnik")]
[assembly: AssemblyProduct("ReSharper.LocateFileInSolutionExplorer")]
[assembly: AssemblyCopyright("Copyright © Igal Tabachnik, 2013")]
[assembly: AssemblyTrademark("")]
[assembly: AssemblyCulture("")]

[assembly: AssemblyVersion("0.0.1")]
[assembly: AssemblyFileVersion("0.0.1")]

[assembly: ActionsXml("ReSharper.LocateFileInSolutionExplorer.Actions.xml")]

// The following information is displayed by ReSharper in the Plugins dialog
[assembly: PluginTitle("LocateFileInSolutionExplorer")]
[assembly: PluginDescription("Locate any file or folder by name in the solution explorer, without opening it. \n" +
                             "Invoked via double Alt-Shift-L.")]
[assembly: PluginVendor("Igal Tabachnik")]
