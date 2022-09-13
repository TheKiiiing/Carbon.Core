using Carbon.Core;
using Harmony;

namespace Carbon.Extended
{
    [HarmonyPatch ( typeof ( ShopFront ), "CompleteTrade" )]
    public class OnShopCompleteTrade
    {
        public static void Postfix ()
        {
            HookExecutor.CallStaticHook ( "OnShopCompleteTrade" );
        }
    }
}