using Carbon.Core;
using Harmony;

namespace Carbon.Extended
{
    [HarmonyPatch ( typeof ( RelationshipManager ), "promote" )]
    public class OnTeamPromote
    {
        public static void Postfix ()
        {
            HookExecutor.CallStaticHook ( "OnTeamPromote" );
        }
    }
}