//
//          Copyright Seth Hendrick 2020-2022.
// Distributed under the Microsoft Public License (MS-PL).
//

using System;
using System.Collections.Generic;
using System.Linq;
using ActivityPub.Models;
using Pretzel.Logic.Templating.Context;

namespace Pretzel.SethExtensions.ActivityPub
{
    internal static class OutboxExtensions
    {
        public static OutboxCollection? FromSiteContext( this SiteContext context )
        {
            const int maximumCharacters = 490;

            var activities = new List<Activity>();
            foreach( Page post in context.Posts.OrderByDescending( p => p.Date ) )
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
                    new CreateActivity
                    {
                        Actor = context.GetAddressName(),
                        Id = url,
                        Summary = $"Created new post: {post.Title}",
                        Object = new NoteLink
                        {
                            Content = status,
                            // Mastodon looks at the name if it somehow can't
                            // find the content.  But we have content, so we should be okay?
                            Id = url,
                            Url = new Uri( url ),
                            // Used to determine the profile which authored the status.
                            AttributedTo = context.GetAddressName(),
                            Sensitive = false,
                            Published = post.Date,
                            // Per mastodon's docs, this should be "as:Public"
                            // to show public status.
                            // Per this URL, it appears as though it needs to be this.
                            // https://blog.joinmastodon.org/2018/06/how-to-implement-a-basic-activitypub-server/
                            To = new string[] { "https://www.w3.org/ns/activitystreams#Public" }

                            // No summary, it is the CW text.
                        }
                    }
                );
            }

            var outboxCollection = new OutboxCollection
            {
                Summary = $"Outbox for {context.GetAddressName()}",
                Activities = activities.ToArray(),
                // Unsure if this needs to be the profile or the URL.
                Id = context.GetOutboxUrl()
            };

            return outboxCollection;
        }
    }
}
