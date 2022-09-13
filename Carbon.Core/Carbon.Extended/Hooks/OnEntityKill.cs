using Carbon.Core;
using Harmony;

namespace Carbon.Extended
{
    [HarmonyPatch ( typeof ( BaseNetworkable ), "Kill" )]
    public class OnEntityKill
    {
        public static void Postfix ()
        {
            HookExecutor.CallStaticHook ( "OnEntityKill" );
        }
    }
}