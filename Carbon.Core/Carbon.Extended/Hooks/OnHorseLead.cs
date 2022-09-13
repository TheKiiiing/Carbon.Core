using Carbon.Core;
using Harmony;

namespace Carbon.Extended
{
    [HarmonyPatch ( typeof ( BaseRidableAnimal ), "RPC_Lead" )]
    public class OnHorseLead
    {
        public static void Postfix ()
        {
            HookExecutor.CallStaticHook ( "OnHorseLead" );
        }
    }
}