using Carbon.Core;
using Harmony;

namespace Carbon.Extended
{
    [HarmonyPatch ( typeof ( ExcavatorArm ), "RPC_SetResourceTarget" )]
    public class OnExcavatorResourceSet
    {
        public static void Postfix ()
        {
            HookExecutor.CallStaticHook ( "OnExcavatorResourceSet" );
        }
    }
}