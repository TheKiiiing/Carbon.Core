using Carbon.Core;
using Harmony;

namespace Carbon.Extended
{
    [HarmonyPatch ( typeof ( ServerBuildingManager ), "Split" )]
    public class OnBuildingSplit
    {
        public static void Postfix ()
        {
            HookExecutor.CallStaticHook ( "OnBuildingSplit" );
        }
    }
}