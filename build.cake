// Load the recipe
#load nuget:?package=TestCentric.Cake.Recipe&version=1.1.0-dev00067
// Comment out above line and uncomment below for local tests of recipe changes
//#load ../TestCentric.Cake.Recipe/recipe/*.cake

BuildSettings.Initialize
(
	context: Context,
	title: "Net462PluggableAgent",
	solutionFile: "net462-pluggable-agent.sln",
	unitTests: "**/*.tests.exe",
	githubRepository: "net462-pluggable-agent",
	nugetVerbosity: NuGetVerbosity.Detailed
);

var MockAssemblyResult = new ExpectedResult("Failed")
{
	Total = 36, Passed = 23, Failed = 5, Warnings = 1, Inconclusive = 1, Skipped = 7,
	Assemblies = new ExpectedAssemblyResult[] { new ExpectedAssemblyResult("mock-assembly.dll") }
};


var PackageTests = new PackageTest[] {
	new PackageTest(
		1, "Net20PackageTest", "Run mock-assembly.dll targeting .NET 2.0",
		"tests/net20/mock-assembly.dll --trace:Debug", MockAssemblyResult),
	new PackageTest(
		1, "Net35PackageTest", "Run mock-assembly.dll targeting .NET 3.5",
		"tests/net35/mock-assembly.dll --trace:Debug", MockAssemblyResult),
	new PackageTest(
		1, "Net462PackageTest", "Run mock-assembly.dll targeting .NET 4.6.2",
		"tests/net462/mock-assembly.dll --trace:Debug", MockAssemblyResult)
};

BuildSettings.Packages.Add(new NuGetPackage(
	"TestCentric.Extension.Net462PluggableAgent",
	title: ".NET 4.6.2 Pluggable Agent",
	description: "TestCentric engine extension for running tests under .NET 4.6.2",
	tags: new [] { "testcentric", "pluggable", "agent", "net462" },
	source: "nuget/TestCentric.Extension.Net462PluggableAgent.nuspec",
	checks: new PackageCheck[] {
		HasFiles("LICENSE.txt", "README.md", "testcentric.png"),
		HasDirectory("tools").WithFiles(
			"net462-agent-launcher.dll", "net462-agent-launcher.pdb",
			"testcentric.extensibility.api.dll", "TestCentric.Engine.Api.dll" ),	
		HasDirectory("tools/agent").WithFiles(
			"net462-agent.exe", "net462-agent.pdb", "net462-agent.exe.config",
			"TestCentric.Engine.Api.dll", "TestCentric.Agent.Core.dll",				
			"TestCentric.Metadata.dll", "TestCentric.Extensibility.dll",
			"TestCentric.Extensibility.Api.dll", "TestCentric.InternalTrace.dll")
	},
	testRunner: new AgentRunner(BuildSettings.NuGetTestDirectory + "TestCentric.Extension.Net462PluggableAgent." + BuildSettings.PackageVersion + "/tools/agent/net462-agent.exe"),
	tests: PackageTests) );
	
BuildSettings.Packages.Add(new ChocolateyPackage(
		"testcentric-extension-net462-pluggable-agent",
		title: ".NET 4.6.2 Pluggable Agent",
		description: "TestCentric engine extension for running tests under .NET 4.6.2",
		tags: new [] { "testcentric", "pluggable", "agent", "net462" },
		source: "choco/testcentric-extension-net462-pluggable-agent.nuspec",
		checks: new PackageCheck[] {
			HasFile("testcentric.png"),
			HasDirectory("tools").WithFiles(
				"LICENSE.txt", "README.md", "VERIFICATION.txt",
				"net462-agent-launcher.dll", "net462-agent-launcher.pdb",
				"TestCentric.Extensibility.Api.dll", "TestCentric.Engine.Api.dll" ),
			HasDirectory("tools/agent").WithFiles(
				"net462-agent.exe", "net462-agent.pdb", "net462-agent.exe.config",
				"TestCentric.Engine.Api.dll", "TestCentric.Agent.Core.dll",
				"TestCentric.Metadata.dll", "Testcentric.Extensibility.dll",
				"TestCentric.Extensibility.Api.dll", "TestCentric.InternalTrace.dll" )
		},
		testRunner: new AgentRunner(BuildSettings.ChocolateyTestDirectory + "testcentric-extension-net462-pluggable-agent." + BuildSettings.PackageVersion + "/tools/agent/net462-agent.exe"),
		tests: PackageTests) );

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

RunTarget(CommandLineOptions.Target);
