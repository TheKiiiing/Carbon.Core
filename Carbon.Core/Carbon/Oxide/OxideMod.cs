﻿///
/// Copyright (c) 2022 Carbon Community 
/// All rights reserved
/// 

using System;
using Carbon.Core;
using Oxide.Core.Libraries;

namespace Oxide.Core
{
	public class OxideMod
	{
		public DataFileSystem DataFileSystem { get; private set; } = new DataFileSystem(CarbonCore.GetDataFolder());

		public Permission Permission { get; private set; }

		public string RootDirectory { get; private set; }
		public string ExtensionDirectory { get; private set; }
		public string InstanceDirectory { get; private set; }
		public string PluginDirectory { get; private set; }
		public string ConfigDirectory { get; private set; }
		public string DataDirectory { get; private set; }
		public string LangDirectory { get; private set; }
		public string LogDirectory { get; private set; }
		public string TempDirectory { get; private set; }

		public float Now => UnityEngine.Time.realtimeSinceStartup;

		public void Load()
		{
			InstanceDirectory = CarbonCore.GetRootFolder();
			RootDirectory = Environment.CurrentDirectory;
			if (RootDirectory.StartsWith(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData)))
				RootDirectory = AppDomain.CurrentDomain.BaseDirectory;

			ConfigDirectory = CarbonCore.GetConfigsFolder();
			DataDirectory = CarbonCore.GetDataFolder();
			LangDirectory = CarbonCore.GetLangFolder();
			LogDirectory = CarbonCore.GetLogsFolder();
			PluginDirectory = CarbonCore.GetPluginsFolder();
			TempDirectory = CarbonCore.GetTempFolder();

			DataFileSystem = new DataFileSystem(DataDirectory);

			Permission = new Permission();
		}

		public void NextTick(Action action)
		{

		}

		public void UnloadPlugin(string name)
		{

		}

		public void OnSave()
		{

		}
	}
}
