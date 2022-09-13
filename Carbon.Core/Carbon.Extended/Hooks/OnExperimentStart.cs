using Carbon.Core;
using Harmony;

namespace Carbon.Extended
{
    [HarmonyPatch ( typeof ( Workbench ), "RPC_BeginExperiment" )]
    public class OnExperimentStart
    {
        public static void Postfix ()
        {
            HookExecutor.CallStaticHook ( "OnExperimentStart" );
        }
    }
}