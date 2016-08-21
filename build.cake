///////////////////////////////////////////////////////////////////////////////
// ARGUMENTS
///////////////////////////////////////////////////////////////////////////////

var target          = Argument<string>("target", "Default");
var configuration   = Argument<string>("configuration", "Release");

///////////////////////////////////////////////////////////////////////////////
// GLOBAL VARIABLES
///////////////////////////////////////////////////////////////////////////////


///////////////////////////////////////////////////////////////////////////////
// SETUP / TEARDOWN
///////////////////////////////////////////////////////////////////////////////

Setup(ctx =>
{
    // Executed BEFORE the first task.
    Information("Running tasks...");
});

Teardown(ctx =>
{
    // Executed AFTER the last task.
    Information("Finished running tasks.");
});

///////////////////////////////////////////////////////////////////////////////
// TASK DEFINITIONS
///////////////////////////////////////////////////////////////////////////////

Task("Clean")
    .Does(ctx => {
        CleanDirectories("./src/**/bin/" + configuration);
        CleanDirectories("./src/**/obj/" + configuration);
});

Task("Restore")
    .IsDependentOn("Clean")
    .Does(ctx => {
        DotNetCoreRestore("./", new DotNetCoreRestoreSettings {
            Sources = new [] { "https://api.nuget.org/v3/index.json" },
            Verbosity = DotNetCoreRestoreVerbosity.Warning
        });
});

Task("Build")
    .IsDependentOn("Restore")
    .Does(ctx => {
        var projects = GetFiles("./**/project.json");
        foreach(var project in projects)
        {
            DotNetCoreBuild(project.GetDirectory().FullPath, new DotNetCoreBuildSettings {
                Configuration = configuration
            });
        }
});

Task("Default")
    .IsDependentOn("Build");

Task("AppVeyor")
    .IsDependentOn("Build");

Task("Bitbucket-Pipelines")
    .IsDependentOn("Build");

Task("Travis")
    .IsDependentOn("Build");

RunTarget(target);