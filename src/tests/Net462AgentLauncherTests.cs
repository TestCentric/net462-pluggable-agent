// ***********************************************************************
// Copyright (c) Charlie Poole and TestCentric contributors.
// Licensed under the MIT License. See LICENSE file in root directory.
// ***********************************************************************

using System;
using System.Linq;
using NUnit.Framework;
using System.IO;
using System.Diagnostics;
using TestCentric.Engine;

namespace TestCentric.Agents
{
    public class Net462AgentLauncherTests
    {
        private static readonly Guid AGENTID = Guid.NewGuid();
        private const string AGENT_URL = "tcp://127.0.0.1:1234/TestAgency";
        private static readonly string REQUIRED_ARGS = $"{AGENT_URL} --pid={Process.GetCurrentProcess().Id}";
        private const string AGENT_ASSEMBLY_NAME = "testcentric-net462-agent.exe";
        private const string X86_AGENT_ASSEMBLY_NAME = "testcentric-net462-agent-x86.exe";
        private static string AGENT_DIR = Path.Combine(TestContext.CurrentContext.TestDirectory, "agent");

        // Constants used for settings
        private const string TARGET_RUNTIME_FRAMEWORK = "TargetRuntimeFramework";
        private const string RUN_AS_X86 = "RunAsX86";
        private const string DEBUG_AGENT = "DebugAgent";
        private const string TRACE_LEVEL = "InternalTraceLevel";
        private const string WORK_DIRECTORY = "WorkDirectory";
        private const string LOAD_USER_PROFILE = "LoadUserProfile";


        private static readonly string[] RUNTIMES = new string[]
        {
            "net-2.0", "net-3.0", "net-3.5", "net-4.0", "net-4.5", "net-4.6.2",
            "netcore-1.1", "netcore-2.1", "netcore-3.1", "netcore-5.0",
            "netcore-6.0", "netcore-7.0", "netcore-8.0", "netcore-9.0"
        };

        private static readonly string[] SUPPORTED = new string[]
        { 
            "net-2.0", "net-3.0", "net-3.5", "net-4.0", "net-4.5", "net-4.6.2"
        };

        private Net462AgentLauncher _launcher;
        private TestPackage _package;

        [SetUp]
        public void SetUp()
        {
            _launcher = new Net462AgentLauncher();
            _package = new TestPackage("junk.dll");
        }

        [TestCaseSource(nameof(RUNTIMES))]
        public void CanCreateProcess(string runtime)
        {
            _package.Settings.Set(SettingDefinitions.TargetRuntimeFramework.WithValue(runtime));
            _package.Settings.Set(SettingDefinitions.RunAsX86.WithValue(false));

            bool supported = SUPPORTED.Contains(runtime);
            Assert.That(_launcher.CanCreateProcess(_package), Is.EqualTo(supported));
        }

        [TestCaseSource(nameof(RUNTIMES))]
        public void CanCreateX86Process(string runtime)
        {
            _package.Settings.Set(SettingDefinitions.TargetRuntimeFramework.WithValue(runtime));
            _package.Settings.Set(SettingDefinitions.RunAsX86.WithValue(true));

            bool supported = SUPPORTED.Contains(runtime);
            Assert.That(_launcher.CanCreateProcess(_package), Is.EqualTo(supported));
        }

        [TestCaseSource(nameof(RUNTIMES))]
        public void CreateProcess(string runtime)
        {
            _package.Settings.Set(SettingDefinitions.TargetRuntimeFramework.WithValue(runtime));
            _package.Settings.Set(SettingDefinitions.RunAsX86.WithValue(false));

            if (SUPPORTED.Contains(runtime))
            {
                var process = _launcher.CreateProcess(AGENTID, AGENT_URL, _package);
                CheckStandardProcessSettings(process);
                CheckAgentPath(process, false);
            }
            else
            {
                Assert.That(_launcher.CreateProcess(AGENTID, AGENT_URL, _package), Is.Null);
            }
        }

        private void CheckAgentPath(Process process, bool x86)
        {
            string agentPath = Path.Combine(AGENT_DIR, x86 ? X86_AGENT_ASSEMBLY_NAME : AGENT_ASSEMBLY_NAME);
            Assert.That(process.StartInfo.FileName, Is.SamePath(agentPath));
        }

        [TestCaseSource(nameof(RUNTIMES))]
        public void CreateX86Process(string runtime)
        {
            _package.Settings.Set(SettingDefinitions.TargetRuntimeFramework.WithValue(runtime));
            _package.Settings.Set(SettingDefinitions.RunAsX86.WithValue(true));

            if (SUPPORTED.Contains(runtime))
            {
                var process = _launcher.CreateProcess(AGENTID, AGENT_URL, _package);
                CheckStandardProcessSettings(process);
                CheckAgentPath(process, true);
            }
            else
            {
                Assert.That(_launcher.CreateProcess(AGENTID, AGENT_URL, _package), Is.Null);
            }
        }

        private void CheckStandardProcessSettings(Process process)
        {
            Assert.That(process, Is.Not.Null);
            Assert.That(process.EnableRaisingEvents, Is.True, "EnableRaisingEvents");

            var startInfo = process.StartInfo;
            Assert.That(startInfo.UseShellExecute, Is.False, "UseShellExecute");
            Assert.That(startInfo.CreateNoWindow, Is.True, "CreateNoWindow");
            Assert.That(startInfo.LoadUserProfile, Is.False, "LoadUserProfile");
            Assert.That(startInfo.WorkingDirectory, Is.EqualTo(Environment.CurrentDirectory));

            var arguments = startInfo.Arguments;
            Assert.That(arguments, Does.Not.Contain("--trace="));
            Assert.That(arguments, Does.Not.Contain("--debug-agent"));
            Assert.That(arguments, Does.Not.Contain("--work="));
        }

        [Test]
        public void DebugAgentSetting()
        {
            var runtime = SUPPORTED[0];
            _package.Settings.Set(SettingDefinitions.TargetRuntimeFramework.WithValue(runtime));
            _package.Settings.Set(SettingDefinitions.DebugAgent.WithValue(true));
            var agentProcess = _launcher.CreateProcess(AGENTID, AGENT_URL, _package);
            Assert.That(agentProcess.StartInfo.Arguments, Does.Contain("--debug-agent"));
        }

        [Test]
        public void TraceLevelSetting()
        {
            var runtime = SUPPORTED[0];
            _package.Settings.Set(SettingDefinitions.TargetRuntimeFramework.WithValue(runtime));
            _package.Settings.Set(SettingDefinitions.InternalTraceLevel.WithValue("Debug"));
            var agentProcess = _launcher.CreateProcess(AGENTID, AGENT_URL, _package);
            Assert.That(agentProcess.StartInfo.Arguments, Does.Contain("--trace=Debug"));
        }

        [Test]
        public void WorkDirectorySetting()
        {
            var runtime = SUPPORTED[0];
            _package.Settings.Set(SettingDefinitions.TargetRuntimeFramework.WithValue(runtime));
            _package.Settings.Set(SettingDefinitions.WorkDirectory.WithValue("WORKDIRECTORY"));
            var agentProcess = _launcher.CreateProcess(AGENTID, AGENT_URL, _package);
            Assert.That(agentProcess.StartInfo.Arguments, Does.Contain("--work=WORKDIRECTORY"));
        }

        [Test]
        public void LoadUserProfileSetting()
        {
            var runtime = SUPPORTED[0];
            _package.Settings.Set(SettingDefinitions.TargetRuntimeFramework.WithValue(runtime));
            _package.Settings.Set(SettingDefinitions.LoadUserProfile.WithValue(true));
            var agentProcess = _launcher.CreateProcess(AGENTID, AGENT_URL, _package);
            Assert.That(agentProcess.StartInfo.LoadUserProfile, Is.True);
        }
    }
}
