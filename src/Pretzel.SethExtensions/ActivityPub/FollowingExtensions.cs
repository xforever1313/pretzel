//
//          Copyright Seth Hendrick 2020-2022.
// Distributed under the Microsoft Public License (MS-PL).
//

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ActivityPub.Models;
using Pretzel.Logic.Templating.Context;

namespace Pretzel.SethExtensions.ActivityPub
{
    internal static class FollowingExtensions
    {
        public static FollowingCollection? FromSiteContext( this SiteContext context )
        {
            IEnumerable<string>? followingList = context.Config.GetFollowing();
            if( followingList is null )
            {
                return null;
            }

            string webFingerName = context.GetWebFingerName();

            var following = new List<Following>();
            foreach( string follow in followingList )
            {
                following.Add(
                    new Following
                    {
                        Actor  = $"@{webFingerName}",
                        Object = follow
                    }
                );
            }

            return new FollowingCollection
            {
                Following = following.ToArray(),
                Summary = $"Who @{webFingerName} is following",

                // Unsure if ID is supposed to be the URL
                // or the ID of the actor.  We'll try URL
                // first.
                Id = context.GetFollowingUrl()
            };
        }
    }
}
