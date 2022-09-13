using Carbon.Core;
using Harmony;

namespace Carbon.Extended
{
    [HarmonyPatch ( typeof ( Item ), "Remove" )]
    public class OnItemRemove
    {
        public static void Postfix ()
        {
            HookExecutor.CallStaticHook ( "OnItemRemove" );
        }
    }
}