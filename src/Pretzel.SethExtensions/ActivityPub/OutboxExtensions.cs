//
//          Copyright Seth Hendrick 2020-2022.
// Distributed under the Microsoft Public License (MS-PL).
//

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using KristofferStrube.ActivityStreams;
using KristofferStrube.ActivityStreams.JsonLD;
using Pretzel.Logic.Templating.Context;

namespace Pretzel.SethExtensions.ActivityPub
{
    internal static class OutboxExtensions
    {
        // ---------------- Fields ----------------

        private const string settingsPrefix = ActivityPubPlugin.SettingPrefix;

        private static readonly Uri publicStream = new Uri(
            "https://www.w3.org/ns/activitystreams#Public"
        );

        // ---------------- Functions ----------------

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
                        // ID must be URL to the post itself.
                        Id = url,
                        Type = new string[] { "Create" },
                        Actor = new Link[]
                        {
                            // Actor must be the profile link.
                            new Link
                            {
                                Href = new Uri( context.GetProfileJsonUrl() )
                            }
                        },
                        Published = post.Date,
                        To = new Link[]
                        {
                            new Link
                            {
                                Href = publicStream
                            }
                        },
                        Object = new Note[]
                        {
                            new Note
                            {
                                Id = url,
                                Type = new string[]{ "Note" },
                                Published = post.Date,
                                Url = new Link[]
                                {
                                    new Link
                                    {
                                        Href = new Uri( url )
                                    }
                                },
                                // Used to determine the profile which authored the status.
                                // Needs to be profile URL.
                                AttributedTo = new Link[]
                                {
                                    new Link
                                    {
                                        Href = new Uri( context.GetProfileJsonUrl() )
                                    }
                                },
                                // Per mastodon's docs, this should be "as:Public"
                                // to show public status.
                                // Per this URL, it appears as though it needs to be this.
                                // https://blog.joinmastodon.org/2018/06/how-to-implement-a-basic-activitypub-server/
                                To = new Link[]
                                {
                                    new Link
                                    {
                                        Href = publicStream
                                    }
                                },
                                // Mastodon looks at the name if it somehow can't
                                // find the content.  But we have content, so we should be okay?
                                Content = new string[]{ status },

                                // No summary, it is the CW text.

                                ExtensionData = new Dictionary<string, JsonElement>
                                {
                                    ["sensitive"] = JsonSerializer.SerializeToElement( false )
                                },

                                Attachment = GetFeaturedImage( post, description ),
                            }
                        }
                    }
                );
            }

            var outboxCollection = new OrderedCollection
            {
                JsonLDContext = new ITermDefinition[]
                {
                    new ReferenceTermDefinition( new Uri( "https://www.w3.org/ns/activitystreams") )
                },
                Type = new string[] { "OrderedCollection" },
                TotalItems = (uint)activities.Count,
                OrderedItems = activities,
                // ID must be a self-reference.
                Id = context.GetOutboxUrl()
            };

            return outboxCollection;
        }

        private static IObjectOrLink[]? GetFeaturedImage( Pretzel.Logic.Templating.Context.Page post, string description )
        {
            if( post.Bag.ContainsKey( $"{settingsPrefix}_featured_image" ) == false )
            {
                return null;
            }
            
            string? urlString = post.Bag[$"{settingsPrefix}_featured_image"]?.ToString();
            if ( urlString is null )
            {
                return null;
            }

            uint? TryParse( string bagValue )
            {
                if( post.Bag.ContainsKey( bagValue ) == false )
                {
                    return null;
                }
                else if( uint.TryParse( post.Bag[bagValue]?.ToString(), out uint value ) )
                {
                    return value;
                }
                
                return null;
            }

            uint? width = TryParse( $"{settingsPrefix}_featured_image_width" );
            uint? height = TryParse( $"{settingsPrefix}_featured_image_height" );

            var featuredImage = new Uri( urlString );
            return new IObjectOrLink[]
            {
                new Image
                {
                    Type = new string[] { "Image" },
                    MediaType = $"image/{Path.GetExtension( urlString ).TrimStart( '.' )}",
                    Name = new string[] { description },
                    Url = new Link[]
                    {
                        new Link
                        {
                            Height = height,
                            Width = width,
                            Href = featuredImage
                        }
                    }
                }
            };
        }
    }
}
