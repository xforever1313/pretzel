//
//          Copyright Seth Hendrick 2020-2022.
// Distributed under the Microsoft Public License (MS-PL).
//

using System;
using System.Collections.Generic;
using System.Text.Json;
using KristofferStrube.ActivityStreams;
using KristofferStrube.ActivityStreams.JsonLD;
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

            var following = new List<IObjectOrLink>();
            foreach( string follow in followingList )
            {
                following.Add(
                    new Link{
                        Href = new Uri( follow )
                    }
                );
            }

            return new OrderedCollection
            {
                JsonLDContext = new ITermDefinition[]
                {
                    new ReferenceTermDefinition( new Uri( "https://www.w3.org/ns/activitystreams") )
                },
                OrderedItems = following,
                Type = new string[]{ "OrderedCollection" },
                TotalItems = (uint)following.Count,

                // ID should be a self-reference.
                Id = context.GetFollowingUrl()
            };
        }
    }
}
