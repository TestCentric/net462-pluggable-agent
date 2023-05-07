#tool NuGet.CommandLine&version=6.0.0

#load nuget:?package=TestCentric.Cake.Recipe&version=1.0.0-dev00061

var target = Argument("target", Argument("t", "Default"));
 
BuildSettings.Initialize
(
	context: Context,
	title: "Net462PluggableAgent",
	solutionFile: "net462-pluggable-agent.sln",
	unitTests: "net462-agent-launcher.tests.exe",
	githubRepository: "net462-pluggable-agent"
);

var packageTests = new PackageTest[] {
	new PackageTest(
		1, "Net20PackageTest", "Run mock-assembly.dll targeting .NET 2.0",
		"tests/net20/mock-assembly.dll", CommonResult),
	new PackageTest(
		1, "Net35PackageTest", "Run mock-assembly.dll targeting .NET 3.5",
		"tests/net35/mock-assembly.dll", CommonResult),
	new PackageTest(
		1, "Net462PackageTest", "Run mock-assembly.dll targeting .NET 4.6.2",
		"tests/net462/mock-assembly.dll", CommonResult)
};

var nugetPackage = new NuGetPackage(
	id: "NUnit.Extension.Net462PluggableAgent",
	source: "nuget/Net462PluggableAgent.nuspec",
	basePath: BuildSettings.OutputDirectory,
	checks: new PackageCheck[] {
		HasFiles("LICENSE.txt", "CHANGES.txt"),
		HasDirectory("tools").WithFiles("net462-agent-launcher.dll", "nunit.engine.api.dll"),
		HasDirectory("tools/agent").WithFiles(
			"net462-pluggable-agent.exe", "net462-pluggable-agent.exe.config",
			"net462-pluggable-agent-x86.exe", "net462-pluggable-agent-x86.exe.config",
			"nunit.engine.api.dll", "testcentric.engine.core.dll",
			"testcentric.engine.metadata.dll", "testcentric.extensibility.dll")},
	testRunner: new GuiRunner("TestCentric.GuiRunner", "2.0.0-alpha8"),
	tests: packageTests );

var chocolateyPackage = new ChocolateyPackage(
	id: "nunit-extension-net462-pluggable-agent",
	source: "choco/net462-pluggable-agent.nuspec",
	basePath: BuildSettings.OutputDirectory,
	checks: new PackageCheck[] {
		HasDirectory("tools").WithFiles("net462-agent-launcher.dll", "nunit.engine.api.dll")
			.WithFiles("LICENSE.txt", "CHANGES.txt", "VERIFICATION.txt"),
		HasDirectory("tools/agent").WithFiles(
			"net462-pluggable-agent.exe", "net462-pluggable-agent.exe.config",
			"net462-pluggable-agent-x86.exe", "net462-pluggable-agent-x86.exe.config",
			"nunit.engine.api.dll", "testcentric.engine.core.dll")},
	testRunner: new GuiRunner("testcentric-gui", "2.0.0-alpha8"),
	tests: packageTests);

BuildSettings.Packages.AddRange(new PackageDefinition[] { nugetPackage, chocolateyPackage });

ExpectedResult CommonResult => new ExpectedResult("Failed")
{
	Total = 36,
	Passed = 23,
	Failed = 5,
	Warnings = 1,
	Inconclusive = 1,
	Skipped = 7,
	Assemblies = new ExpectedAssemblyResult[]
	{
		new ExpectedAssemblyResult("mock-assembly.dll", "Net462AgentLauncher")
	}
};

//////////////////////////////////////////////////////////////////////
// TASK TARGETS
//////////////////////////////////////////////////////////////////////

Task("Appveyor")
	.IsDependentOn("Build")
	.IsDependentOn("Test")
	.IsDependentOn("Package")
	.IsDependentOn("Publish")
	.IsDependentOn("CreateDraftRelease")
	.IsDependentOn("CreateProductionRelease");

Task("Default")
    .IsDependentOn("Build");

//////////////////////////////////////////////////////////////////////
// EXECUTION
//////////////////////////////////////////////////////////////////////

RunTarget(target);
