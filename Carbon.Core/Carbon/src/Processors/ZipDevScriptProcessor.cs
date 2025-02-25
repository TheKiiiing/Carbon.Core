﻿#if DEBUG
using System;
using System.Collections;
using System.IO;
using System.IO.Compression;
using System.Linq;
using Carbon.Base;
using Carbon.Components;
using Carbon.Contracts;
using Carbon.Core;
using Carbon.Extensions;

/*
 *
 * Copyright (c) 2022-2023 Carbon Community
 * All rights reserved.
 *
 */

namespace Carbon.Managers;

public class ZipDevScriptProcessor : BaseProcessor, IZipDevScriptProcessor
{
	public override string Name => "ZipDebugScript Processor";
	public override bool EnableWatcher => !Community.IsConfigReady || Community.Runtime.Config.ScriptWatchers;
	public override string Folder => Defines.GetZipDevFolder();
	public override string Extension => ".cs";
	public override Type IndexedType => typeof(ZipDevScript);

	public override void Start()
	{
		BlacklistPattern = new[]
		{
			"backups",
			"debug"
		};

		base.Start();

		IncludeSubdirectories = true;
	}

	public bool AllPendingScriptsComplete()
	{
		foreach (var instance in InstanceBuffer)
		{
			if (instance.Value is ZipDevScript script)
			{
				if (script.Loader != null && !script.Loader.HasFinished) return false;
			}
		}

		return true;
	}
	public bool AllNonRequiresScriptsComplete()
	{
		foreach (var instance in InstanceBuffer)
		{
			if (instance.Value is ZipDevScript script)
			{
				if (script.Loader != null && !script.Loader.HasRequires && !script.Loader.HasFinished) return false;
			}
		}

		return true;
	}
	public bool AllExtensionsComplete()
	{
		foreach (var instance in InstanceBuffer)
		{
			if (instance.Value is ZipDevScript script)
			{
				if (script.Loader != null && !script.Loader.IsExtension && !script.Loader.HasFinished) return false;
			}
		}

		return true;
	}

	void IScriptProcessor.StartCoroutine(IEnumerator coroutine)
	{
		StartCoroutine(coroutine);
	}
	void IScriptProcessor.StopCoroutine(IEnumerator coroutine)
	{
		StopCoroutine(coroutine);
	}

	public override void OnCreated(object sender, FileSystemEventArgs e)
	{
		if (!EnableWatcher || IsBlacklisted(e.FullPath)) return;

		var directory = Path.GetDirectoryName(e.FullPath);
		var id = Path.GetFileNameWithoutExtension(directory);

		if (InstanceBuffer.TryGetValue(directory, out var instance1))
		{
			instance1?.SetDirty();
			return;
		}

		if (InstanceBuffer.TryGetValue(id, out var instance2))
		{
			instance2?.SetDirty();
			return;
		}

		InstanceBuffer.Add(directory, null);
	}
	public override void OnChanged(object sender, FileSystemEventArgs e)
	{
		var path = e.FullPath;
		var directory = Path.GetDirectoryName(path);

		if (!EnableWatcher || IsBlacklisted(e.FullPath)) return;

		if (InstanceBuffer.TryGetValue(directory, out var mod)) mod.SetDirty();
	}
	public override void OnRenamed(object sender, RenamedEventArgs e)
	{
		var path = e.FullPath;
		var directory = Path.GetDirectoryName(path);

		if (!EnableWatcher || IsBlacklisted(path)) return;

		if (InstanceBuffer.TryGetValue(directory, out var mod)) mod.MarkDeleted();
		InstanceBuffer.Add(directory, null);
	}
	public override void OnRemoved(object sender, FileSystemEventArgs e)
	{
		var path = e.FullPath;
		var directory = Path.GetDirectoryName(path);

		if (!EnableWatcher || IsBlacklisted(path)) return;

		if (InstanceBuffer.TryGetValue(directory, out var mod)) mod.MarkDeleted();
	}

	public class ZipDevScript : Process, IScriptProcessor.IScript
	{
		public IScriptLoader Loader { get; set; }

		public override IBaseProcessor.IParser Parser => new ZipDevScriptParser();

		public override void Dispose()
		{
			try
			{
				Loader?.Clear();
			}
			catch (Exception ex)
			{
				Logger.Error($"Error disposing {File}", ex);
			}

			Loader = null;
		}
		public override void Execute(IBaseProcessor processor)
		{
			base.Execute(processor);

			try
			{
				Carbon.Core.ModLoader.FailedMods.RemoveAll(x => x.File == File);

				if (!OsEx.Folder.Exists(File))
				{
					Dispose();
					return;
				}

				Loader = new ScriptLoader
				{
					Parser = Parser,
					Mod = Community.Runtime.ZipPlugins,
					Process = this
				};

				foreach (var file in OsEx.Folder.GetFilesWithExtension(File, processor.Extension))
				{
					var content = OsEx.File.ReadText(file);

					Loader.Sources.Add(new BaseSource
					{
						ContextFilePath = File,
						ContextFileName = Path.GetFileName(File),
						FilePath = file,
						FileName = Path.GetFileName(file),
						Content = content
					});
				}

				Loader.Load();
			}
			catch (Exception ex)
			{
				Logger.Warn($"Failed processing {Path.GetFileNameWithoutExtension(File)}:\n{ex}");
			}
		}
	}

	public class ZipDevScriptParser : Parser, IBaseProcessor.IParser
	{
		internal const string QuoteReplacer = "[CARBONQUOTE]";
		internal const string Quote = "\\\"";
		internal const string NewLineReplacer = "[CARBONNEWLINE]";
		internal const string NewLine = "\\n";
		internal const string Harmony = "Harmony";
		internal const string FOOT = "FindObjectsOfType";

		public override void Process(string file, string input, out string output)
		{
			using (TimeMeasure.New("ScriptParser.Process"))
			{
				try
				{
					#region Handle Unicode & Quote Escaping

					// if (input.Contains("\\u"))
					// {
					// 	output = Regex.Unescape(input.Replace(Quote, QuoteReplacer).Replace(NewLine, NewLineReplacer)).Replace(QuoteReplacer, Quote).Replace(NewLineReplacer, NewLine);
					// }
					// else output = input;

					#endregion

					if (input.Contains(Harmony))
					{
						Logger.Warn($" Warning! '{Path.GetFileNameWithoutExtension(file)}' uses Harmony. That may cause instability, use at your own discretion!");
					}

					if (input.Contains(FOOT))
					{
						Logger.Warn($" Warning! '{Path.GetFileNameWithoutExtension(file)}' uses UnityEngine.GameObject.FindObjectsOfType. That may cause significant performance drops, and/or server stalls. Report to the developer or use at your own discretion!");
					}

					output = input.Replace("PluginTimers", "Timers")
						.Replace("using Harmony;", "using HarmonyLib;")
						.Replace("HarmonyInstance.Create", "new Harmony")
						.Replace("HarmonyInstance", "Harmony");
				}
				catch
				{
					output = input;
				}
			}
		}
	}
}
#endif
