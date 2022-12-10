//
//          Copyright Seth Hendrick 2020-2022.
// Distributed under the Microsoft Public License (MS-PL).
//

using Pretzel.Logic;
using Pretzel.Logic.Templating.Context;

namespace Pretzel.SethExtensions.ActivityPub
{
    internal static class ActivityPubExtensions
    {
        // ---------------- Fields ----------------

        private const string settingsPrefix = ActivityPubPlugin.SettingPrefix;

        // ---------------- Functions ----------------

        public static bool GenerateInbox( this IConfiguration config )
        {
            if( config.ContainsKey( $"{settingsPrefix}_inbox" ) == false )
            {
                return false;
            }

            return bool.Parse( config[$"{settingsPrefix}_inbox"].ToString() );
        }

        public static bool GenerateOutbox( this IConfiguration config )
        {
            if( config.ContainsKey( $"{settingsPrefix}_outbox" ) == false )
            {
                return false;
            }

            return bool.Parse( config[$"{settingsPrefix}_outbox"].ToString() );
        }

        public static string GetActPubUrl( this SiteContext context )
        {
            return context.UrlCombine( $"{context.Config[$"{settingsPrefix}_directory"]}/" );
        }
    }
}
