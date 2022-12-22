//
//          Copyright Seth Hendrick 2020-2022.
// Distributed under the Microsoft Public License (MS-PL).
//

using System;
using System.Collections.Generic;
using Pretzel.Logic;
using Pretzel.Logic.Templating.Context;

namespace Pretzel.SethExtensions.ActivityPub
{
    internal static class ActivityPubExtensions
    {
        // ---------------- Fields ----------------

        private const string settingsPrefix = ActivityPubPlugin.SettingPrefix;

        // ---------------- Functions ----------------

        public static bool GenerateOutbox( this IConfiguration config )
        {
            string key = $"{settingsPrefix}_outbox";

            if( config.ContainsKey( key ) == false )
            {
                return false;
            }
            else if( bool.TryParse( config[key].ToString(), out bool generate ) )
            {
                return generate;
            }
            throw new ArgumentException( $"'{key}' MUST be set to true or false." );
        }

        /// <returns>
        /// null if there are is no one to follow.
        /// </returns>
        public static IEnumerable<string>? GetFollowing( this IConfiguration config )
        {
            string key = $"{settingsPrefix}_following";
            if( config.ContainsKey( key ) == false )
            {
                return null;
            }

            IEnumerable<string>? following = config[key] as IEnumerable<string>;
            if( following is null )
            {
                throw new ArgumentException( $"'{key}' must be a list type." );
            }

            return following;
        }

        public static string GetActPubUrl( this SiteContext context )
        {
            return context.UrlCombine( $"{context.Config[$"{settingsPrefix}_directory"]}/" );
        }

        public static string GetProfileJsonUrl( this SiteContext context )
        {
            return context.UrlCombine( $"{context.Config[$"{settingsPrefix}_directory"]}/profile.json" );
        }

        public static string GetOutboxUrl( this SiteContext context )
        {
            return context.UrlCombine( $"{context.Config[$"{settingsPrefix}_directory"]}/outbox.json" );
        }

        public static string? TryGetInboxUrl( this SiteContext context )
        {
            return context.Config[$"{settingsPrefix}_inbox_url"]?.ToString();
        }

        public static string GetFollowingUrl( this SiteContext context )
        {
            return context.UrlCombine( $"{context.Config[$"{settingsPrefix}_directory"]}/following.json" );
        }

        public static bool TryGetIconUrl( this IConfiguration config, out string iconUrl )
        {
            if( config.ContainsKey( $"{settingsPrefix}_icon" ) == false )
            {
                iconUrl = "";
                return false;
            }

            string? value = config[$"{settingsPrefix}_icon"].ToString();
            if( value is null )
            {
                iconUrl = "";
                return false;
            }

            iconUrl = value;
            return true;
        }
    }
}
