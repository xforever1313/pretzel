//
//          Copyright Seth Hendrick 2020-2022.
// Distributed under the Microsoft Public License (MS-PL).
//

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using KristofferStrube.ActivityStreams;
using Pretzel.Logic.Templating.Context;

namespace Pretzel.SethExtensions.ActivityPub
{
    internal static class OutboxExtensions
    {
        public static OrderedCollection? FromSiteContext( this SiteContext context )
        {
            const int maximumCharacters = 490;

            var activities = new List<Activity>();
            foreach( Pretzel.Logic.Templating.Context.Page post in context.Posts.OrderByDescending( p => p.Date ) )
            {
                string url = context.UrlCombine(
                    new LinkHelper().EvaluateLink( context, post )
                );

                string title = $@"<p><strong>{post.Title}</strong></p>";
                string urlHtml = $@"<a {SethHtmlFormatter.ATagProperties} href=""{url}"">Read More</a>";

                int charactersLeft = maximumCharacters - title.Length - urlHtml.Length;

                string description = "";
                if( post.Bag.ContainsKey( "description" ) )
                {
                    description = post.Bag["description"]?.ToString() ?? "";
                    if( description.Length > charactersLeft )
                    {
                        description = description.Substring( 0, charactersLeft );
                    }
                    description = $"<p>{description}</p>";
                }

                string status = $"{title}{description}{urlHtml}";

                activities.Add(
                    new Create
                    {
                        Actor = new Actor[]
                        {
                            new Actor
                            {
                                Name = new string[]{ context.GetAddressName() }
                            }
                        },
                        Id = url,
                        Summary = new string[]{ $"Created new post: {post.Title}" },
                        Object = new Note[]
                        {
                            new Note
                            
                            {
                                Content = new string[]{ status },
                                // Mastodon looks at the name if it somehow can't
                                // find the content.  But we have content, so we should be okay?
                                Id = url,
                                Url = new Link[]
                                {
                                    new Link
                                    {
                                        Href = new Uri( url )
                                    }
                                },
                                // Used to determine the profile which authored the status.
                                AttributedTo = new Actor[]
                                {
                                    new Actor
                                    {
                                        Name = new string[]{ context.GetAddressName() }
                                    }
                                },
                                Published = post.Date,
                                // Per mastodon's docs, this should be "as:Public"
                                // to show public status.
                                // Per this URL, it appears as though it needs to be this.
                                // https://blog.joinmastodon.org/2018/06/how-to-implement-a-basic-activitypub-server/
                                To = new Link[]
                                {
                                    new Link
                                    {
                                        Href = new Uri( "https://www.w3.org/ns/activitystreams#Public" )
                                    }
                                },

                                // No summary, it is the CW text.

                                ExtensionData = new Dictionary<string, JsonElement>
                                {
                                    ["sensitive"] = JsonSerializer.SerializeToElement( false )
                                }
                            }
                        }
                    }
                );
            }

            var outboxCollection = new OrderedCollection
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
                Type = new string[] { "OrderedCollection" },
                Summary = new string[]{ $"Outbox for {context.GetAddressName()}" },
                TotalItems = (uint)activities.Count,
                OrderedItems = activities,
                // Unsure if this needs to be the profile or the URL.
                Id = context.GetOutboxUrl()
            };

            return outboxCollection;
        }
    }
}
