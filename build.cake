#load CakeScripts\GAssembly.cs
#load CakeScripts\Settings.cs
#addin "Cake.FileHelpers"
#addin "Cake.Incubator"

// VARS

Settings.Cake = Context;
Settings.Version = "3.22.24.29";
Settings.BuildTarget = Argument("BuildTarget", "Default");
Settings.Assembly = Argument("Assembly", "");

var msbuildsettings = new DotNetCoreMSBuildSettings();
var list = new List<GAssembly>();

// TASKS

Task("Init")
    .Does(() =>
{
    // Assign some common properties
    msbuildsettings = msbuildsettings.WithProperty("Version", Settings.Version);
    msbuildsettings = msbuildsettings.WithProperty("Authors", "'GtkSharp Contributors'");
    msbuildsettings = msbuildsettings.WithProperty("PackageLicenseUrl", "'https://github.com/GtkSharp/GtkSharp/blob/cakecore/LICENSE'");

    // Add stuff to list
    Settings.Init();
    foreach(var gassembly in Settings.AssemblyList)
        if(string.IsNullOrEmpty(Settings.Assembly) || Settings.Assembly == gassembly.Name)
            list.Add(gassembly);
});

Task("Prepare")
    .IsDependentOn("Clean")
    .Does(() =>
{
    // Build tools
    DotNetCoreRestore("Source/Tools/Tools.sln");
    DotNetCoreBuild("Source/Tools/Tools.sln", new DotNetCoreBuildSettings {
        Verbosity = DotNetCoreVerbosity.Minimal,
        Configuration = "Release"
    });

    // Generate code and prepare libs projects
    foreach(var gassembly in list)
        gassembly.Prepare();
    DotNetCoreRestore("Source/GtkSharp.sln");
});

Task("Clean")
    .IsDependentOn("Init")
    .Does(() =>
{
    foreach(var gassembly in list)
        gassembly.Clean();
});

Task("FullClean")
    .IsDependentOn("Clean")
    .Does(() =>
{
    DeleteDirectory("BuildOutput", true);
});

Task("Build")
    .IsDependentOn("Prepare")
    .Does(() =>
{
    var settings = new DotNetCoreBuildSettings
    {
        Configuration = "Release",
        MSBuildSettings = msbuildsettings
    };

    if (list.Count == Settings.AssemblyList.Count)
        DotNetCoreBuild("Source/GtkSharp.sln", settings);
    else
    {
        foreach(var gassembly in list)
            DotNetCoreBuild(gassembly.Csproj, settings);
    }
});



Task("PackageNuGet")
    .IsDependentOn("Build")
    .Does(() =>
{
    var settings = new DotNetCorePackSettings
    {
        MSBuildSettings = msbuildsettings,
        Configuration = "Release",
        OutputDirectory = "BuildOutput/NugetPackages",
        NoBuild = true,

    };

    foreach(var gassembly in list)
        DotNetCorePack(gassembly.Csproj, settings);
});

Task("PackageTemplates")
    .IsDependentOn("Init")
    .Does(() =>
{
    var settings = new NuGetPackSettings
    {
        OutputDirectory = "BuildOutput/NugetPackages",
        Version = Settings.Version
    };

    settings.BasePath = "Source/Templates/GtkSharp.Template.CSharp";
    NuGetPack("Source/Templates/GtkSharp.Template.CSharp/GtkSharp.Template.CSharp.nuspec", settings);

    settings.BasePath = "Source/Templates/GtkSharp.Template.VBNet";
    NuGetPack("Source/Templates/GtkSharp.Template.VBNet/GtkSharp.Template.VBNet.nuspec", settings);
});

// TASK TARGETS

Task("Default")
    .IsDependentOn("Build");
    
Task("FullBuild")
    .IsDependentOn("PackageNuGet")
	.IsDependentOn("PackageTemplates");

// EXECUTION

RunTarget(Settings.BuildTarget);
