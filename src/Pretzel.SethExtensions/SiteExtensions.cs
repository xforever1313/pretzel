//
//          Copyright Seth Hendrick 2020-2022.
// Distributed under the Microsoft Public License (MS-PL).
//

using System;
using Pretzel.Logic.Templating.Context;

namespace Pretzel.SethExtensions
{
    public static class SiteExtensions
    {
        /// <summary>
        /// Combines the given relative URL with the site's URL
        /// to generate the full URL.
        /// </summary>
        public static string UrlCombine( this SiteContext context, string urlLocation )
        {
            string? url = context.GetSiteUrl();

            url = url.TrimEnd( '/' );
            urlLocation = urlLocation.TrimStart( '/' );
            return $"{url}/{urlLocation}";
        }

        public static string GetSiteUrl( this SiteContext context )
        {
            string? url = context.Config["url"].ToString();
            if( url is null )
            {
                throw new ArgumentNullException( nameof( url ), "'url' must be specified in site config." );
            }

            return url;
        }

        public static string GetSiteUrlWithoutHttp( this SiteContext context )
        {
            string? url = context.Config["urlnohttp"].ToString();
            if( url is null )
            {
                throw new ArgumentNullException( nameof( url ), "'urlnohttp' must be specified in site config." );
            }

            return url;
        }
    }
}
