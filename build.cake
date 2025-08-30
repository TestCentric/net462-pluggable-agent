// Load the recipe
#load nuget:?package=TestCentric.Cake.Recipe&version=1.4.1-dev00002
// Comment out above line and uncomment below for local tests of recipe changes
//#load ../TestCentric.Cake.Recipe/recipe/*.cake

const string AGENT_NAME = "testcentric-net462-agent";
const string X86_AGENT_NAME = "testcentric-net462-agent-x86";

BuildSettings.Initialize
(
	context: Context,
	title: "Net462PluggableAgent",
	solutionFile: "net462-pluggable-agent.sln",
	unitTests: "**/*.tests.exe",
	githubRepository: "net462-pluggable-agent",
	nugetVerbosity: NuGetVerbosity.Detailed
);

var MockAssemblyResult1 = new ExpectedResult("Failed")
{
	Total = 36, Passed = 23, Failed = 5, Warnings = 1, Inconclusive = 1, Skipped = 7,
	Assemblies = new ExpectedAssemblyResult[] { new ExpectedAssemblyResult("mock-assembly.dll") }
};

var MockAssemblyX86Result1 = new ExpectedResult("Failed")
{
	Total = 36, Passed = 23, Failed = 5, Warnings = 1, Inconclusive = 1, Skipped = 7,
	Assemblies = new ExpectedAssemblyResult[] { new ExpectedAssemblyResult("mock-assembly-x86.dll") }
};

var MockAssemblyResult2 = new ExpectedResult("Failed")
{
	Total = 37, Passed = 23, Failed = 5, Warnings = 1, Inconclusive = 1, Skipped = 7,
	Assemblies = new ExpectedAssemblyResult[] { new ExpectedAssemblyResult("mock-assembly.dll") }
};

var MockAssemblyX86Result2 = new ExpectedResult("Failed")
{
	Total = 37, Passed = 23, Failed = 5, Warnings = 1, Inconclusive = 1, Skipped = 7,
	Assemblies = new ExpectedAssemblyResult[] { new ExpectedAssemblyResult("mock-assembly-x86.dll") }
};

var PackageTests = new PackageTest[] {
	new PackageTest(
		1, "Net20PackageTest", "Run mock-assembly.dll targeting .NET 2.0",
		"tests/net20/mock-assembly.dll --trace:Debug", MockAssemblyResult1),
	new PackageTest(
		1, "Net35PackageTest", "Run mock-assembly.dll targeting .NET 3.5",
		"tests/net35/mock-assembly.dll --trace:Debug", MockAssemblyResult2),
	new PackageTest(
		1, "Net462PackageTest", "Run mock-assembly.dll targeting .NET 4.6.2",
		"tests/net462/mock-assembly.dll --trace:Debug", MockAssemblyResult2),
	new PackageTest(
		1, "Net20X86PackageTest", "Run mock-assembly-x86.dll targeting .NET 2.0",
		"tests/net20/mock-assembly-x86.dll --x86 --trace:Debug", MockAssemblyX86Result1),
	new PackageTest(
		1, "Net35X86PackageTest", "Run mock-assembly-x86.dll targeting .NET 3.5",
		"tests/net35/mock-assembly-x86.dll --x86 --trace:Debug", MockAssemblyX86Result2),
	new PackageTest(
		1, "Net462X86PackageTest", "Run mock-assembly-x86.dll targeting .NET 4.6.2",
		"tests/net462/mock-assembly-x86.dll --x86 --trace:Debug", MockAssemblyX86Result2)
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
            $"{AGENT_NAME}.exe", $"{AGENT_NAME}.pdb", $"{AGENT_NAME}.exe.config",
            $"{X86_AGENT_NAME}.exe", $"{X86_AGENT_NAME}.pdb", $"{X86_AGENT_NAME}.exe.config",
			"TestCentric.Engine.Api.dll", "TestCentric.Agent.Core.dll",				
			"TestCentric.Metadata.dll", "TestCentric.Extensibility.dll",
			"TestCentric.Extensibility.Api.dll", "TestCentric.InternalTrace.dll")
	},
	testRunner: new AgentRunner(
      BuildSettings.NuGetTestDirectory + $"TestCentric.Extension.Net462PluggableAgent.{BuildSettings.PackageVersion}/tools/agent/{AGENT_NAME}.exe",
      BuildSettings.NuGetTestDirectory + $"TestCentric.Extension.Net462PluggableAgent.{BuildSettings.PackageVersion}/tools/agent/{X86_AGENT_NAME}.exe"),
	tests: PackageTests) );
	
BuildSettings.Packages.Add(new ChocolateyPackage(
	"testcentric-extension-net462-pluggable-agent",
	title: "TestCentric Extension - .NET 4.6.2 Pluggable Agent",
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
            $"{AGENT_NAME}.exe", $"{AGENT_NAME}.pdb", $"{AGENT_NAME}.exe.config",
			$"{X86_AGENT_NAME}.exe", $"{X86_AGENT_NAME}.pdb", $"{X86_AGENT_NAME}.exe.config",
            "TestCentric.Engine.Api.dll", "TestCentric.Agent.Core.dll",
			"TestCentric.Metadata.dll", "Testcentric.Extensibility.dll",
			"TestCentric.Extensibility.Api.dll", "TestCentric.InternalTrace.dll" )
	},
	testRunner: new AgentRunner(
        $"{BuildSettings.ChocolateyTestDirectory}testcentric-extension-net462-pluggable-agent.{BuildSettings.PackageVersion}/tools/agent/{AGENT_NAME}.exe",
        $"{BuildSettings.ChocolateyTestDirectory}testcentric-extension-net462-pluggable-agent.{BuildSettings.PackageVersion}/tools/agent/{X86_AGENT_NAME}.exe"),
	tests: PackageTests) );

//////////////////////////////////////////////////////////////////////
// EXECUTION
//////////////////////////////////////////////////////////////////////

Build.Run();
