#:sdk Cake.Sdk@6.2.0
#:package Cake.BuildSystems.Module@9.0.0

var target = Argument("target", "Publish");

//////////////////////////////////////////////////////////////////////
// TASKS
//////////////////////////////////////////////////////////////////////

Setup(context =>
{
    var configuration = Argument("configuration", "Release");

    Information("Configuration: {0}", configuration);

    return new BuildData(
        Configuration: configuration,
        ProjectFile: "./src/BitbucketPipelinesShield/BitbucketPipelinesShield.csproj",
        PublishDirectory: "./artifacts/publish",
        ArtifactsDirectory: "./artifacts",
        MSBuildSettings: new DotNetMSBuildSettings()
            .SetConfiguration(configuration));
});

Task("Clean")
    .WithCriteria(c => HasArgument("rebuild"))
    .Does<BuildData>((ctx, data) =>
{
    CleanDirectories(data.DirectoriesToClean);
});

Task("Publish")
    .IsDependentOn("Clean")
    .Does<BuildData>((ctx, data) =>
{
    DotNetPublish(
        data.ProjectFile.FullPath,
        new DotNetPublishSettings
        {
            Configuration = data.Configuration,
            OutputDirectory = data.PublishDirectory,
            MSBuildSettings = data.MSBuildSettings
        });
});

Task("Zip")
    .IsDependentOn("Publish")
    .Does<BuildData>((ctx, data) =>
{
    var zipPath = data.ArtifactsDirectory.CombineWithFilePath("BitbucketPipelinesShield.zip");
    EnsureDirectoryExists(data.ArtifactsDirectory);
    Zip(data.PublishDirectory, zipPath);
    Information("Created {0}", zipPath);
});

Task("UploadArtifacts")
    .IsDependentOn("Zip")
    .Does<BuildData>((ctx, data) =>
    GitHubActions.Commands.UploadArtifact(
        data.ArtifactsDirectory,
        "BitbucketPipelinesShield"));

Task("GitHubActions")
    .IsDependentOn("UploadArtifacts");

//////////////////////////////////////////////////////////////////////
// EXECUTION
//////////////////////////////////////////////////////////////////////

RunTarget(target);

//////////////////////////////////////////////////////////////////////
// Models
//////////////////////////////////////////////////////////////////////

public record BuildData(
    string Configuration,
    FilePath ProjectFile,
    DirectoryPath PublishDirectory,
    DirectoryPath ArtifactsDirectory,
    DotNetMSBuildSettings MSBuildSettings
)
{
    public DirectoryPath[] DirectoriesToClean { get; init; } =
    [
        "./src/BitbucketPipelinesShield/bin/" + Configuration,
        "./src/BitbucketPipelinesShield/obj/" + Configuration,
        ArtifactsDirectory
    ];
}
