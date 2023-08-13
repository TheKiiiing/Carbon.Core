﻿using System.Diagnostics;
using Carbon.Client.Packets;
using Debug = UnityEngine.Debug;

namespace Carbon.Client;

public struct RPC
{
	public uint Id;
	public string Name;

	public static readonly bool SERVER = typeof(LocalPlayer).GetMethod("OnInventoryChanged") == null;

	public const string DOMAIN = "carbon.com.";

	public static List<RPC> rpcList = new();

	internal static Dictionary<uint, Func<BasePlayer, Network.Message, object>> _cache = new();
	internal static object[] _argBuffer = new object[2];

	public static void Init()
	{
		Init(typeof(RPC).Assembly.GetTypes());
	}
	public static void Init(params Type[] types )
	{
		foreach (var type in types)
		{
			foreach (var method in type.GetMethods(BindingFlags.NonPublic | BindingFlags.Static))
			{
				var attr = method.GetCustomAttribute<Method>();
				if (attr == null) continue;

				var name = $"{DOMAIN}{attr.Id}";
				var id = StringPool.Add(name);
				rpcList.Add(new RPC
				{
					Id = id,
					Name = name
				});

				_cache.Add(id, (player, msg) =>
				{
					_argBuffer[0] = player;
					_argBuffer[1] = msg;

					var result = (object)null;

					try
					{
						result = method.Invoke(null, _argBuffer);
					}
					catch (Exception ex)
					{
						Console.WriteLine($"Failed executing RPC call ({ex.Message})\n{(ex.InnerException ?? ex).Demystify().StackTrace}");
					}

					return result;
				});
			}
		}

		Get("ping");
		Get("pong");
	}
	public static RPC Get(string name)
	{
		name = $"{DOMAIN}{name}";

		return new RPC
		{
			Id = StringPool.Add(name),
			Name = name
		};
	}
	public static RPC Get(uint id)
	{
		return new RPC
		{
			Id = id,
			Name = StringPool.Get(id)
		};
	}

	public static object HandleRPCMessage(BasePlayer player, uint rpc, Network.Message message)
	{
		if (_cache.TryGetValue(rpc, out var value))
		{
			Console.WriteLine($"HandleRPCMessage: {player} {rpc} {StringPool.Get(rpc)}");
			return value(player, message);
		}

		return null;
	}

	[Method("pong")]
	private static void Pong(BasePlayer player, Network.Message message)
	{
		CarbonClient.Get(player);
		Logger.Log($"Player '{player}' has a Carbon client!");
	}

	[AttributeUsage(AttributeTargets.Method)]
	public class Method : Attribute
	{
		public string Id { get; set; }

		public Method(string id)
		{
			Id = id;
		}
	}
}
