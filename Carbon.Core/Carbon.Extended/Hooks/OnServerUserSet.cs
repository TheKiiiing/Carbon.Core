using Carbon.Core;
using Harmony;

namespace Carbon.Extended
{
    [HarmonyPatch ( typeof ( ServerUsers ), "Set" )]
    public class OnServerUserSet
    {
        public static void Postfix ()
        {
            HookExecutor.CallStaticHook ( "OnServerUserSet" );
        }
    }
}