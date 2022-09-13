using Carbon.Core;
using Harmony;

namespace Carbon.Extended
{
    [HarmonyPatch ( typeof ( ThrownWeapon ), "DoDrop" )]
    public class OnExplosiveDropped
    {
        public static void Postfix ()
        {
            HookExecutor.CallStaticHook ( "OnExplosiveDropped" );
        }
    }
}