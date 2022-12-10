//
//          Copyright Seth Hendrick 2020-2022.
// Distributed under the Microsoft Public License (MS-PL).
//

using ActivityPub.Models;
using Pretzel.Logic;
using Pretzel.Logic.Templating.Context;

namespace Pretzel.SethExtensions.ActivityPub
{
    internal static class WebFingerExtensions
    {
        private const string settingsPrefix = ActivityPubPlugin.SettingPrefix;

        public static WebFinger FromSiteContext( SiteContext context )
        {
            IConfiguration config = context.Config;
            var webFinger = new WebFinger
            {
                Subject = $"acct@{config[$"{settingsPrefix}_username"]}@{config["urlnohttp"]}",
                Links = new WebFingerLinks
                {
                    Href = context.UrlCombine( $"{config[$"{settingsPrefix}_directory"]}/profile.json" )
                }
            };

            return webFinger;
        }
    }
}
