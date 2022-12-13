//
//          Copyright Seth Hendrick 2020-2022.
// Distributed under the Microsoft Public License (MS-PL).
//

using System;
using System.Collections.Generic;
using System.Text.Json;
using KristofferStrube.ActivityStreams;
using Pretzel.Logic.Templating.Context;

namespace Pretzel.SethExtensions.ActivityPub
{
    internal static class FollowingExtensions
    {
        public static OrderedCollection? FromSiteContext( this SiteContext context )
        {
            IEnumerable<string>? followingList = context.Config.GetFollowing();
            if( followingList is null )
            {
                return null;
            }

            string webFingerName = context.GetAddressName();

            var following = new List<Follow>();
            foreach( string follow in followingList )
            {
                following.Add(
                    new Follow
                    {
                        Actor = new Actor[]
                        {
                            new Actor
                            {
                                Name = new string[]{ webFingerName }
                            }
                        },
                        Object = new KristofferStrube.ActivityStreams.Object[]
                        {
                            new Actor
                            {
                                Name = new string[]{ follow }
                            }
                        }
                    }
                );
            }

            return new OrderedCollection
            {
                ExtensionData = new Dictionary<string, JsonElement>
                {
                    ["@context"] = JsonSerializer.SerializeToElement(
                        new Uri[]
                        {
                            new Uri( "https://www.w3.org/ns/activitystreams")
                        }
                    )
                },
                OrderedItems = following,
                Type = new string[]{ "OrderedCollection" },
                TotalItems = (uint)following.Count,
                Summary = new string[]{ $"Who {webFingerName} is following" },

                // Unsure if ID is supposed to be the URL
                // or the ID of the actor.  We'll try URL
                // first.
                Id = context.GetFollowingUrl()
            };
        }
    }
}
