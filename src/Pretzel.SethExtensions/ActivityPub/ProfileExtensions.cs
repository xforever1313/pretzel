//
//          Copyright Seth Hendrick 2020-2022.
// Distributed under the Microsoft Public License (MS-PL).
//

using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using ActivityPub.Models;
using Pretzel.Logic;
using Pretzel.Logic.Templating.Context;

namespace Pretzel.SethExtensions.ActivityPub
{
    internal static class ProfileExtensions
    {
        // ---------------- Fields ----------------

        private const string settingsPrefix = ActivityPubPlugin.SettingPrefix;

        // ---------------- Functions ----------------

        public static Profile FromSiteContext( SiteContext context )
        {
            IConfiguration config = context.Config;

            string baseUrl = config["url"].ToString();

            var profile = new Profile
            {
                Discoverable = true,
                ManuallyApprovesFollowers = false,
                Type = "Service",
                Url = new Uri( baseUrl ),

                // ID must be the base URL.
                Id = baseUrl
            };

            if( config.GenerateInbox() )
            {
                profile = profile with
                {
                    Inbox = new Uri(
                        $"{context.GetActPubUrl()}inbox.json"
                    )
                };
            }

            if( config.GenerateOutbox() )
            {
                profile = profile with
                {
                    Outbox = new Uri(
                        $"{context.GetActPubUrl()}outbox.json"
                    )
                };
            }

            if( config.ContainsKey( $"{settingsPrefix}_profileurl" ) )
            {
                profile = profile with
                {
                    Url = new Uri(
                        context.UrlCombine( config[$"{settingsPrefix}_profileurl"].ToString() )
                    )
                };
            }

            if( config.ContainsKey( $"{settingsPrefix}_username" ) )
            {
                profile = profile with
                {
                    PreferredUserName = config[$"{settingsPrefix}_username"].ToString()
                };
            }

            {
                var attachments = new List<Attachment>();
                attachments.Add(
                    new Attachment
                    {
                        Name = "WebSite",
                        Type = "PropertyValue",
                        Value = GetAttachmentUrl( config["url"].ToString() )

                    }
                );

                if( config.ContainsKey( "github" ) )
                {
                    attachments.Add(
                        new Attachment
                        {
                            Name = "GitHub",
                            Type = "PropertyValue",
                            Value = GetAttachmentUrl( config["github"].ToString() )
                        }
                    );
                }

                if( config.ContainsKey( "contact" ) )
                {
                    attachments.Add(
                        new Attachment
                        {
                            Name = "Email",
                            Type = "PropertyValue",
                            Value = config["contact"].ToString()
                        }
                    );
                }

                profile = profile with { Attachments = attachments.ToArray() };
            }

            if( config.ContainsKey( $"{settingsPrefix}_icon" ) )
            {
                profile = profile with
                {
                    Icon = new Uri( config[$"{settingsPrefix}_icon"].ToString() )
                };
            }

            if( config.ContainsKey( $"{settingsPrefix}_published" ) )
            {
                profile = profile with
                {
                    Published = config[$"{settingsPrefix}_published"].ToString()
                };
            }

            if( config.ContainsKey( $"{settingsPrefix}_summary" ) )
            {
                profile = profile with
                {
                    Summary = config[$"{settingsPrefix}_summary"].ToString()
                };
            }

            if( config.ContainsKey( $"{settingsPrefix}_title" ) )
            {
                profile = profile with
                {
                    Name = config[$"{settingsPrefix}_title"].ToString()
                };
            }

            if( config.ContainsKey( $"{settingsPrefix}_publickeyfile" ) )
            {
                string file = Path.Combine(
                    context.SourceFolder,
                    config[$"{settingsPrefix}_publickeyfile"].ToString()
                );

                var publicKeyBuilder = new StringBuilder();
                foreach( string line in File.ReadAllLines( file ) )
                {
                    publicKeyBuilder.Append( line );
                    publicKeyBuilder.Append( "\\n" );
                }
                publicKeyBuilder.Remove( publicKeyBuilder.Length - 2, 2 );

                profile = profile with
                {
                    PublicKey = new ProfilePublicKey
                    {
                        Id = $"{baseUrl}#main-key",
                        Owner = baseUrl,
                        PublicKeyPem = publicKeyBuilder.ToString()
                    }
                };
            }

            return profile;
        }

        private static string GetAttachmentUrl( string? baseUrl )
        {
            ArgumentNullException.ThrowIfNull( baseUrl );

            return
                @$"<a href=""{baseUrl}"" rel=""me nofollow noopener noreferrer"" target=""_blank"">{baseUrl}</a>";
        }
    }
}
