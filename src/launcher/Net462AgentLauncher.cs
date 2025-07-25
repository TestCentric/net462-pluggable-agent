// ***********************************************************************
// Copyright (c) Charlie Poole and TestCentric contributors.
// Licensed under the MIT License. See LICENSE file in root directory.
// ***********************************************************************

using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Runtime.Versioning;
using System.Text;
using TestCentric.Extensibility;
using TestCentric.Engine;
using TestCentric.Engine.Extensibility;
using TestCentric.Engine.Internal;

namespace TestCentric.Agents
{
    [Extension(Description ="Pluggable agent running tests under .NET 4.6.2", EngineVersion = "2.0.0")]
    public class Net462AgentLauncher : IAgentLauncher
    {
        // TODO: use logging once it is in a separate assembly
        //static Logger log = InternalTrace.GetLogger(typeof(Net462AgentLauncher));

        private const string AGENT_ASSEMBLY_NAME = "testcentric-net462-agent";
        private const string X86_AGENT_ASSEMBLY_NAME = "testcentric-net462-agent-x86";
        private const string RUNTIME_IDENTIFIER = ".NETFramework";
        private static readonly Version RUNTIME_VERSION = new Version(4, 6, 2);
        private static readonly FrameworkName TARGET_FRAMEWORK = new FrameworkName(RUNTIME_IDENTIFIER, RUNTIME_VERSION);

        public TestAgentInfo AgentInfo => new TestAgentInfo(
            GetType().Name,
            TestAgentType.LocalProcess,
            TARGET_FRAMEWORK.ToString());

        public bool CanCreateProcess(TestPackage package)
        {
            // Get target runtime
            string runtimeSetting = package.Settings.GetValueOrDefault(SettingDefinitions.TargetRuntimeFramework);
            return runtimeSetting.Length > 4 && runtimeSetting.StartsWith("net-") && runtimeSetting[4] <= '4';
        }

        public Process CreateProcess(Guid agentId, string agencyUrl, TestPackage package)
        {
            // Should not be called unless runtime is one we can handle
            if (!CanCreateProcess(package))
                return null;

            // Access other package settings
            bool runAsX86 = package.Settings.GetValueOrDefault(SettingDefinitions.RunAsX86);
            bool debugTests = package.Settings.GetValueOrDefault(SettingDefinitions.DebugTests);
            bool debugAgent = package.Settings.GetValueOrDefault(SettingDefinitions.DebugAgent);
            string traceLevel = package.Settings.GetValueOrDefault(SettingDefinitions.InternalTraceLevel);
            bool loadUserProfile = package.Settings.GetValueOrDefault(SettingDefinitions.LoadUserProfile);
            string workDirectory = package.Settings.GetValueOrDefault(SettingDefinitions.WorkDirectory);

            var sb = new StringBuilder($"--agentId={agentId} --agencyUrl={agencyUrl} --pid={Process.GetCurrentProcess().Id}");

            // Set options that need to be in effect before the package
            // is loaded by using the command line.
            if (traceLevel != "Off")
                sb.Append(" --trace=").EscapeProcessArgument(traceLevel);
            if (debugAgent)
                sb.Append(" --debug-agent");
            if (workDirectory != string.Empty)
                sb.Append($" --work=").EscapeProcessArgument(workDirectory);

            var agentName = runAsX86 ? $"{X86_AGENT_ASSEMBLY_NAME}.exe" : $"{AGENT_ASSEMBLY_NAME}.exe";
            var agentDir = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "agent");
            var agentPath = Path.Combine(agentDir, agentName);
            var agentArgs = sb.ToString();

            var process = new Process();
            process.EnableRaisingEvents = true;

            var startInfo = process.StartInfo;
            startInfo.UseShellExecute = false;
            startInfo.CreateNoWindow = true;
            startInfo.WorkingDirectory = Environment.CurrentDirectory;
            startInfo.LoadUserProfile = loadUserProfile;

            startInfo.FileName = agentPath;
            startInfo.Arguments = agentArgs;

            return process;
        }
    }
}
