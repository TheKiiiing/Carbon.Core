﻿using Humanlights.Components;
using Oxide.Plugins;
using System.Linq;
using UnityEngine;

namespace Carbon.Core
{
    public class CarbonCorePlugin : RustPlugin
    {
        internal static void Reply ( object message, ConsoleSystem.Arg arg )
        {
            if ( arg != null && arg.Player () != null )
            {
                arg.Player ().SendConsoleCommand ( $"echo {message}" );
                return;
            }
            CarbonCore.Log ( message );
        }

        [ConsoleCommand ( "version" )]
        private void GetVersion ( ConsoleSystem.Arg arg )
        {
            if ( arg.Player () != null && !arg.Player ().IsAdmin ) return;

            Reply ( $"Carbon v{CarbonCore.Version}", arg );
        }

        [ConsoleCommand ( "list" )]
        private void GetList ( ConsoleSystem.Arg arg )
        {
            if ( arg.Player () != null && !arg.Player ().IsAdmin ) return;

            var body = new StringTable ( "#", "Mod", "Author", "Version" );
            var count = 1;

            Reply ( $"Found: {CarbonLoader._loadedMods.Count} mods  with {CarbonLoader._loadedMods.Sum ( x => x.Plugins.Count )} plugins", arg );

            foreach ( var mod in CarbonLoader._loadedMods )
            {
                body.AddRow ( $"{count:n0}", mod.Name, "", "" );

                foreach ( var plugin in mod.Plugins )
                {
                    body.AddRow ( $"", plugin.Name, plugin.Author, $"v{plugin.Version}" );
                }

                count++;
            }

            Reply ( body.ToStringMinimal (), arg );
        }

        [ConsoleCommand ( "reload" )]
        private void Reload ( ConsoleSystem.Arg arg )
        {
            if ( arg.Player () != null && !arg.Player ().IsAdmin ) return;

            CarbonCore.ClearPlugins ();
            CarbonCore.ReloadPlugins ();
        }
    }
}