//
//          Copyright Seth Hendrick 2020-2022.
// Distributed under the Microsoft Public License (MS-PL).
//

using System;
using System.Collections.Generic;
using System.IO;
using Pretzel.Logic;
using Pretzel.Logic.Templating.Context;

namespace Pretzel.SethExtensions.ActivityPub
{
    internal static class WebFingerExtensions
    {
        // ---------------- Fields ----------------

        private const string settingsPrefix = ActivityPubPlugin.SettingPrefix;

        // ---------------- Functions ----------------

        public static string GetWebFingerName( this SiteContext context )
        {
            IConfiguration config = context.Config;
            return $"{config[$"{settingsPrefix}_username"]}@{config["urlnohttp"]}";
        }

        public static string GetAddressName( this SiteContext context )
        {
            return $"@{GetWebFingerName( context )}";
        }

        public static WebFinger FromSiteContext( SiteContext context )
        {
            var webFingerLinks = new List<WebFingerLinks>
            {
                new WebFingerLinks
                {
                    Rel = "self",
                    Type = "application/activity+json",
                    Href = new Uri( context.GetProfileJsonUrl() )
                },
                new WebFingerLinks
                {
                    Rel = "http://webfinger.net/rel/profile-page",
                    Type = "text/html",
                    Href = new Uri( context.GetSiteUrl() )
                }
            };

            if( context.Config.TryGetIconUrl( out string iconUrl ) )
            {
                webFingerLinks.Add(
                    new WebFingerLinks
                    {
                        Rel = "http://webfinger.net/rel/avatar",
                        Type = $"image/{Path.GetExtension( iconUrl ).TrimStart( '.' )}",
                        Href = new Uri( iconUrl )
                    }
                );
            }

            var webFinger = new WebFinger
            {
                Subject = $"acct:{GetWebFingerName( context )}",
                Links = webFingerLinks.ToArray()
            };

            return webFinger;
        }
    }
}
