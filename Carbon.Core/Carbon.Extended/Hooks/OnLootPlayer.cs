using Carbon.Core;
using Harmony;

namespace Carbon.Extended
{
    [HarmonyPatch ( typeof ( BasePlayer ), "RPC_LootPlayer" )]
    public class OnLootPlayer
    {
        public static void Postfix ()
        {
            HookExecutor.CallStaticHook ( "OnLootPlayer" );
        }
    }
}