using Carbon.Core;
using Harmony;

namespace Carbon.Extended
{
    [HarmonyPatch ( typeof ( ResourceDepositManager ), "CreateFromPosition" )]
    public class OnResourceDepositCreated
    {
        public static void Postfix ()
        {
            HookExecutor.CallStaticHook ( "OnResourceDepositCreated" );
        }
    }
}