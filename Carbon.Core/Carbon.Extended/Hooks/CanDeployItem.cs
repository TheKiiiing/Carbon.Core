using Carbon.Core;
using Harmony;

namespace Carbon.Extended
{
    [HarmonyPatch ( typeof ( Deployer ), "DoDeploy" )]
    public class CanDeployItem
    {
        public static void Postfix ()
        {
            HookExecutor.CallStaticHook ( "CanDeployItem" );
        }
    }
}