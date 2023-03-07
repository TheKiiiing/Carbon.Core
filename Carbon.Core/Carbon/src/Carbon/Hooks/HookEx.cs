﻿using System;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using API.Hooks;
using Carbon.Contracts;
using HarmonyLib;

/*
 *
 * Copyright (c) 2022-2023 Carbon Community 
 * All rights reserved.
 *
 */

namespace Carbon.Hooks;

public class HookEx : IDisposable, IHook
{
	private HookRuntime _runtime;
	private string _originalChecksum;
	private readonly TypeInfo _patchMethod;


	public string HookName
	{ get; }

	public string HookFullName
	{ get; }

	public HookFlags Options
	{ get; }

	public Type TargetType
	{ get; }

	public string TargetMethod
	{ get; }

	public Type[] TargetMethodArgs
	{ get; }

	public string Identifier
	{ get; }

	public string ShortIdentifier
	{ get => Identifier.Substring(Identifier.Length - 6); }

	public string Checksum
	{ get; }

	public string[] Dependencies
	{ get; }


	public bool IsPatch
	{ get => Options.HasFlag(HookFlags.Patch); }

	public bool IsStaticHook
	{ get => Options.HasFlag(HookFlags.Static); }

	public bool IsDynamicHook
	{ get => !Options.HasFlag(HookFlags.Static) && !Options.HasFlag(HookFlags.Patch); }

	public bool IsHidden
	{ get => Options.HasFlag(HookFlags.Hidden); }

	public bool IsChecksumIgnored
	{ get => Options.HasFlag(HookFlags.IgnoreChecksum); }

	public bool IsLoaded
	{ get => _runtime.Status is HookState.Success or HookState.Warning or HookState.Failure or HookState.Inactive; }

	public bool IsInstalled
	{ get => _runtime.Status is HookState.Success or HookState.Warning; }

	public bool IsFailed
	{ get => _runtime.Status is HookState.Failure; }

	public override string ToString()
		=> $"{HookName}[{ShortIdentifier}]";

	public bool HasDependencies()
		=> Dependencies is { Length: > 0 };

	public string PatchMethodName
	{ get => _patchMethod.Name; }

	public HookState Status
	{ get => _runtime.Status; set => _runtime.Status = value; }

	public Exception LastError
	{ get => _runtime.LastError; set => _runtime.LastError = value; }


	public HookEx(TypeInfo type)
	{
		try
		{
			Harmony.DEBUG = false;

			if (type == null || !Attribute.IsDefined(type, typeof(HookAttribute.Patch), false))
				throw new Exception($"Type is null or metadata not defined");

			HookAttribute.Patch metadata = type.GetCustomAttribute<HookAttribute.Patch>() ?? null;

			if (metadata == default)
				throw new Exception($"Metadata information is invalid or was not found");

			Dependencies = new string[0];
			HookFullName = metadata.FullName;
			HookName = metadata.Name;
			TargetMethod = metadata.Method;
			TargetMethodArgs = metadata.MethodArgs;
			TargetType = metadata.Target;

			if (Attribute.IsDefined(type, typeof(HookAttribute.Identifier), false))
				Identifier = type.GetCustomAttribute<HookAttribute.Identifier>()?.Value ?? $"{Guid.NewGuid():N}";

			if (Attribute.IsDefined(type, typeof(HookAttribute.Options), false))
				Options = type.GetCustomAttribute<HookAttribute.Options>()?.Value ?? HookFlags.None;

			if (Attribute.IsDefined(type, typeof(HookAttribute.Dependencies), false))
				Dependencies = type.GetCustomAttribute<HookAttribute.Dependencies>()?.Value ?? default;

			if (Attribute.IsDefined(type, typeof(HookAttribute.Checksum), false))
				Checksum = type.GetCustomAttribute<HookAttribute.Checksum>()?.Value ?? default;

			_patchMethod = type;
			_runtime.Status = HookState.Inactive;
			_runtime.HarmonyHandler = new Harmony(Identifier);

			// cache the additional metadata about the hook
			_runtime.Prefix = AccessTools.Method(type, "Prefix") ?? null;
			_runtime.Postfix = AccessTools.Method(type, "Postfix") ?? null;
			_runtime.Transpiler = AccessTools.Method(type, "Transpiler") ?? null;

			// alternative way to find the target method
			_runtime.TargetMethodSeeker = AccessTools.Method(type, "TargetMethod") ?? null;

			if (_runtime.Prefix is null && _runtime.Postfix is null && _runtime.Transpiler is null)
				throw new Exception($"No patch found (prefix, postfix, transpiler)");
		}
		catch (Exception e)
		{
			Logger.Error($"Error while parsing '{type.Name}'", e);
		}
		finally
		{
			Harmony.DEBUG = true;
		}
	}

	public bool ApplyPatch()
	{
		try
		{
			if (IsInstalled) return true;

			MethodBase original = ((TargetMethod == null)
				? _runtime.TargetMethodSeeker ?? null
				: original = AccessTools.Method(TargetType, TargetMethod, TargetMethodArgs) ?? null) ?? throw new Exception($"Target method not found");

			HarmonyMethod
				prefix = null,
				postfix = null,
				transpiler = null;

			if (_runtime.Prefix != null)
				prefix = new HarmonyMethod(_runtime.Prefix);

			if (_runtime.Postfix != null)
				postfix = new HarmonyMethod(_runtime.Postfix);

			if (_runtime.Transpiler != null)
				transpiler = new HarmonyMethod(_runtime.Transpiler);

			if (prefix is null && postfix is null && transpiler is null)
				throw new Exception($"No patch found while applying patch (prefix, postfix, transpiler)");

			MethodInfo current = (_runtime.HarmonyHandler.Patch(original,
				prefix: prefix, postfix: postfix, transpiler: transpiler
			) ?? null) ?? throw new Exception($"Harmony failed to execute");

			_runtime.Status = HookState.Success;
			Logger.Debug($"Hook '{this}' patched '{original}'", 2);
		}
#if DEBUG
		catch (HarmonyException e)
		{
			StringBuilder sb = new StringBuilder();
			Logger.Error($"Error while patching hook '{this}' index:{e.GetErrorIndex()} offset:{e.GetErrorOffset()}", e);
			sb.AppendLine($"{e.InnerException?.Message.Trim() ?? string.Empty}");

			int x = 0;
			foreach (var instruction in e.GetInstructionsWithOffsets())
				sb.AppendLine($"\t{x++:000} {instruction.Key:X4}: {instruction.Value}");

			Logger.Error(sb.ToString());
			sb = default;
			return false;
		}
#endif
		catch (System.Exception e)
		{
			Logger.Error($"Error while patching hook '{this}'", e.InnerException ?? e);
			_runtime.Status = HookState.Failure;
			_runtime.LastError = e;
			return false;
		}

		return true;
	}

	public bool RemovePatch()
	{
		try
		{
			if (!IsInstalled) return true;
			_runtime.HarmonyHandler.UnpatchAll(Identifier);

			Logger.Debug($"Hook '{this}' unpatched '{TargetType.Name}.{TargetMethod}'", 2);
			_runtime.Status = HookState.Inactive;
			return true;
		}
		catch (System.Exception e)
		{
			Logger.Error($"Error while unpatching hook '{HookName}'", e);
			_runtime.Status = HookState.Failure;
			_runtime.LastError = e;
			return false;
		}
	}

	public string GetTargetMethodChecksum()
	{
		if (_originalChecksum != null) return _originalChecksum;
		MethodBase original = AccessTools.Method(TargetType, TargetMethod, TargetMethodArgs) ?? null;
		if (original == null) return default;

		using SHA1Managed sha1 = new SHA1Managed();
		byte[] bytes = sha1.ComputeHash(original.GetMethodBody()?.GetILAsByteArray() ?? Array.Empty<byte>());
		_originalChecksum = string.Concat(bytes.Select(b => b.ToString("x2")));
		Logger.Debug($">> CALC: {_originalChecksum}");
		Logger.Debug($">> HOOK: {Checksum}");

		return _originalChecksum;
	}

	public void SetStatus(HookState Status, Exception e = null)
	{
		_runtime.Status = Status;
		_runtime.LastError = e;
	}

	private bool disposedValue;

	protected virtual void Dispose(bool disposing)
	{
		if (!disposedValue)
		{
			// managed resources
			if (disposing)
			{
				RemovePatch();
			}

			// unmanaged resources
			_runtime.HarmonyHandler = null;
			disposedValue = true;
		}
	}

	public void Dispose()
	{
		Dispose(disposing: true);
		GC.SuppressFinalize(this);
	}
}
