﻿/*
	Dataphor
	© Copyright 2000-2009 Alphora
	This file is licensed under a modified BSD-license which can be found here: http://dataphor.org/dataphor_license.txt
*/

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using Alphora.Dataphor.DAE.ServerTests.Utilities;
using Alphora.Dataphor.Logging;
using NUnit.Framework;
using Alphora.Dataphor.DAE.Server;

namespace Alphora.Dataphor.DAE.ServerTests
{
	[TestFixture]
	public class CoverageDeviceTest
	{
		private static readonly ILogger SRFLogger = LoggerFactory.Instance.CreateLogger(typeof(CoverageDeviceTest));
		private ServerConfigurationManager FServerConfigurationManager;
		private ServerConfiguration FConfiguration;
		private IServer FServer;
		private IServerSession FSession;
		private IServerProcess FProcess;
		
		[TestFixtureSetUp]
		public void SetUpFixture()
		{
			SRFLogger.WriteLine(TraceLevel.Verbose, "Test Fixture Set Up");
			MSSQLServerConfigurationManager.ResetDatabase("DataphorTests");
			
			FServerConfigurationManager = new SQLCEServerConfigurationManager();
			FConfiguration = FServerConfigurationManager.GetTestConfiguration("TestInstance");
			FServerConfigurationManager.ResetInstance();
			FServer = FServerConfigurationManager.GetServer();
			FServer.Start();
			
			IServerSession LSession = FServer.Connect(new SessionInfo("Admin", ""));
			try
			{
				IServerProcess LProcess = LSession.StartProcess(new ProcessInfo(LSession.SessionInfo));
				try
				{
					LProcess.ExecuteScript("EnsureLibraryRegistered('Frontend');");
					LProcess.ExecuteScript("EnsureLibraryRegistered('TestFramework');");
					LProcess.ExecuteScript("EnsureLibraryRegistered('TestFramework.Coverage.Base');");
					LProcess.ExecuteScript("EnsureLibraryRegistered('TestFramework.Coverage.Devices');");
				}
				finally
				{
					LSession.StopProcess(LProcess);
				}
			}
			finally
			{
				FServer.Disconnect(LSession);
			}
		}
		
		[TestFixtureTearDown]
		public void TearDownFixture()
		{
			SRFLogger.WriteLine(TraceLevel.Verbose, "Test Fixture Tear Down");
			FServer.Stop();
			FServerConfigurationManager.ResetInstance();
		}
		
		[SetUp]
		public void SetUp()
		{
			SRFLogger.WriteLine(TraceLevel.Verbose, "Test Setup");
			FSession = FServer.Connect(new SessionInfo("Admin", "", "TestFramework.Coverage.Devices"));
			FProcess = FSession.StartProcess(new ProcessInfo(FSession.SessionInfo));
		}
		
		[TearDown]
		public void TearDown()
		{
			SRFLogger.WriteLine(TraceLevel.Verbose, "Test TearDown");
			if (FProcess != null)
			{
				FSession.StopProcess(FProcess);
				FProcess = null;
			}
			
			if (FSession != null)
			{
				FServer.Disconnect(FSession);
				FSession = null;
			}
		}
		
		private void ExecuteScript(string ALibraryName, string AScriptName)
		{
			FProcess.ExecuteScript(String.Format("ExecuteScript('{0}', '{1}');", ALibraryName, AScriptName));
		}
		
		[Test]
		public void TestMSSQLDevice()
		{
			ExecuteScript("TestFramework.Coverage.Devices", "TestMSSQLDevice");
		}

		[Test]
		public void Run()
		{
			
		}
	}
}