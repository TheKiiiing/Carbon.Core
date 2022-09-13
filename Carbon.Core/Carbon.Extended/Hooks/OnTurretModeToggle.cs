using Carbon.Core;
using Harmony;

namespace Carbon.Extended
{
    [HarmonyPatch ( typeof ( AutoTurret ), "SetPeacekeepermode" )]
    public class OnTurretModeToggle
    {
        public static void Postfix ()
        {
            HookExecutor.CallStaticHook ( "OnTurretModeToggle" );
        }
    }
}