// Load the recipe
#load nuget:?package=TestCentric.Cake.Recipe&version=1.1.0-dev00063
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
	packageContent: new PackageContent()
		.WithRootFiles("../../LICENSE.txt", "../../README.md", "../../testcentric.png")
		.WithDirectories(
			new DirectoryContent("tools").WithFiles(
				"net462-agent-launcher.dll", "net462-agent-launcher.pdb",
				"testcentric.extensibility.api.dll", "testcentric.engine.api.dll" ),
			new DirectoryContent("tools/agent").WithFiles(
				"agent/net462-agent.exe", "agent/net462-agent.pdb", "agent/net462-agent.exe.config",
				"agent/testcentric.engine.api.dll", "agent/testcentric.engine.core.dll", "agent/TestCentric.Agent.Core.dll",				
				"agent/testcentric.engine.metadata.dll", "agent/testcentric.extensibility.dll",
				"agent/testcentric.extensibility.api.dll", "agent/TestCentric.InternalTrace.dll" ) ),
	testRunner: new AgentRunner(BuildSettings.NuGetTestDirectory + "TestCentric.Extension.Net462PluggableAgent." + BuildSettings.PackageVersion + "/tools/agent/net462-agent.exe"),
	tests: PackageTests) );
	
BuildSettings.Packages.Add(new ChocolateyPackage(
		"testcentric-extension-net462-pluggable-agent",
		title: ".NET 4.6.2 Pluggable Agent",
		description: "TestCentric engine extension for running tests under .NET 4.6.2",
		tags: new [] { "testcentric", "pluggable", "agent", "net462" },
		packageContent: new PackageContent()
			.WithRootFiles("../../testcentric.png")
			.WithDirectories(
				new DirectoryContent("tools").WithFiles(
					"../../LICENSE.txt", "../../README.md", "../../VERIFICATION.txt",
					"net462-agent-launcher.dll", "net462-agent-launcher.pdb", "testcentric.extensibility.api.dll", "testcentric.engine.api.dll" ),
				new DirectoryContent("tools/agent").WithFiles(
					"agent/net462-agent.exe", "agent/net462-agent.pdb", "agent/net462-agent.exe.config", "agent/testcentric.engine.api.dll", "agent/TestCentric.InternalTrace.dll",
					"agent/testcentric.engine.core.dll", "agent/TestCentric.Agent.Core.dll", "agent/testcentric.engine.metadata.dll", "agent/testcentric.extensibility.dll", "agent/testcentric.extensibility.api.dll" ) ),
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
