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
    public class Outbox
    {
        // ---------------- Fields ----------------

        private const string settingsPrefix = ActivityPubPlugin.SettingPrefix;

        private readonly SiteContext siteContext;

        private readonly IList<Logic.Templating.Context.Page> posts;

        private readonly Dictionary<uint, List<Logic.Templating.Context.Page>> postsByPage;

        private readonly uint postsPerOutboxPage;

        private readonly string outboxUrl;

        // ---------------- Constructor ----------------

        public Outbox( SiteContext siteContext )
        {
            this.siteContext = siteContext;
            this.posts = siteContext.Posts.OrderByDescending( p => p.Date ).ToList();
            this.postsByPage = new Dictionary<uint, List<Logic.Templating.Context.Page>>();

            if( siteContext.Config.ContainsKey( $"{settingsPrefix}_posts_per_outbox_page" )  == false )
            {
                throw new ArgumentException(
                    $"Missing setting: {settingsPrefix}_posts_per_outbox_page",
                    nameof( siteContext )
                );
            }

            this.postsPerOutboxPage = uint.Parse(
                siteContext.Config[$"{settingsPrefix}_posts_per_outbox_page"]?.ToString() ?? ""
            );

            this.outboxUrl = siteContext.GetOutboxUrl();

            uint pageId = 1;
            for( int postId = 0; postId < this.posts.Count; )
            {
                var pageList = new List<Logic.Templating.Context.Page>();
                for(
                    int postsInPage = 0;
                    ( postsInPage < postsPerOutboxPage ) && ( postId < this.posts.Count );
                    ++postsInPage, ++postId
                )
                {
                    pageList.Add( this.posts[postId] );
                }

                postsByPage.Add( pageId, pageList );
                ++pageId;
            }
        }

        // ---------------- Functions ----------------

        public OrderedCollection GetOutboxIndex()
        {
            return new OrderedCollection
            {
                // ID must be a self-reference.
                Id = this.outboxUrl,
                JsonLDContext = new ITermDefinition[]
                {
                    new ReferenceTermDefinition( new Uri( "https://www.w3.org/ns/activitystreams") )
                },
                TotalItems = (uint)this.posts.Count,
                Type = new string[] { "OrderedCollection" },

                // Per the spec, the curent points to the page containing the items
                // that have been created or updated the most recently.
                // May as well make that the first page, which is the
                // most recent messages.
                Current = GetStartUrl(),
                First = GetStartUrl(),
                Last = GetEndUrl(),
            };
        }

        public IReadOnlyDictionary<uint, OrderedCollectionPage> GetOutboxPages()
        {
            var dict = new Dictionary<uint, OrderedCollectionPage>();

            for( uint i = 1; i <= this.postsByPage.Count; ++i )
            {
                Link? previous = null;
                if( i > 1 )
                {
                    previous = new Link
                    {
                        Href = GetOutboxUrl( i - 1 )
                    };
                }

                Link? next = null;
                if( i < this.postsByPage.Count )
                {
                    next = new Link
                    {
                        Href = GetOutboxUrl( i + 1 )
                    };
                }

                dict.Add(
                    i,
                    new OrderedCollectionPage
                    {
                        JsonLDContext = new ITermDefinition[]
                        {
                            new ReferenceTermDefinition( new Uri( "https://www.w3.org/ns/activitystreams") )
                        },
                        // ID must be a self-reference.
                        Id = GetOutboxUrl( i ).ToString(),
                        Type = new string[] { "OrderedCollectionPage" },
                        PartOf = new Link
                        {
                            Href = new Uri( this.siteContext.GetOutboxUrl() )
                        },
                        // Per the spec, the curent points to the page containing the items
                        // that have been created or updated the most recently.
                        // May as well make that the first page, which is the
                        // most recent messages.
                        Current = GetStartUrl(),
                        First = GetStartUrl(),
                        Last = GetEndUrl(),
                        Next = next,
                        Prev = previous,
                        TotalItems = (uint)this.posts.Count,
                        OrderedItems = GetActivities( this.postsByPage[i] )
                    }
                );
            }

            return dict;
        }

        private Link? GetStartUrl()
        {
            // If there are no posts, there's no where to start.
            if( this.posts.Count == 0 )
            {
                return null;
            }

            // If there are posts, the start link is always
            // the first page.
            return new Link
            {
                Href = GetOutboxUrl( 1 )
            };
        }

        private Link? GetEndUrl()
        {
            // If there are no posts, there's no where to end.
            if( this.posts.Count == 0 )
            {
                return null;
            }

            return new Link
            {
                Href = GetOutboxUrl( this.postsByPage.Keys.Max() )
            };
        }

        private Uri GetOutboxUrl( uint index )
        {
            string newString = this.outboxUrl.Replace( ".json", $"{index}.json" );

            return new Uri( newString );
        }

        private IEnumerable<Activity> GetActivities( IEnumerable<Logic.Templating.Context.Page> posts )
        {
            const int maximumCharacters = 490;

            var activities = new List<Activity>();
            foreach( var post in posts )
            {
                string url = this.siteContext.UrlCombine(
                    new LinkHelper().EvaluateLink( this.siteContext, post )
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
                                Href = new Uri( this.siteContext.GetProfileJsonUrl() )
                            }
                        },
                        Published = post.Date,
                        To = new Link[]
                        {
                            new Link
                            {
                                Href = ActivityPubPlugin.PublicStream
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
                                        Href = new Uri( this.siteContext.GetProfileJsonUrl() )
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
                                        Href = ActivityPubPlugin.PublicStream
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

            return activities;
        }

        private static IObjectOrLink[]? GetFeaturedImage( Logic.Templating.Context.Page post, string description )
        {
            if( post.Bag.ContainsKey( $"{settingsPrefix}_featured_image" ) == false )
            {
                return null;
            }

            string? urlString = post.Bag[$"{settingsPrefix}_featured_image"]?.ToString();
            if( urlString is null )
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
