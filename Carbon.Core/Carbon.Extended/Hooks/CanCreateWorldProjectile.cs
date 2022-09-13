using Carbon.Core;
using Harmony;

namespace Carbon.Extended
{
    [HarmonyPatch ( typeof ( BasePlayer ), "CreateWorldProjectile" )]
    public class CanCreateWorldProjectile
    {
        public static void Postfix ()
        {
            HookExecutor.CallStaticHook ( "CanCreateWorldProjectile" );
        }
    }
}