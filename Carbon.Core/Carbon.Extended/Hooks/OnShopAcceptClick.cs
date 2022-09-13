using Carbon.Core;
using Harmony;

namespace Carbon.Extended
{
    [HarmonyPatch ( typeof ( ShopFront ), "AcceptClicked" )]
    public class OnShopAcceptClick
    {
        public static void Postfix ()
        {
            HookExecutor.CallStaticHook ( "OnShopAcceptClick" );
        }
    }
}