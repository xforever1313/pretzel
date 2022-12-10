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

                // ID must be the same as the URL to this page (its a self-reference).
                Id = context.GetProfileJsonUrl()
            };

            if( config.ContainsKey( $"{settingsPrefix}_profileurl" ) )
            {
                profile = profile with
                {
                    Url = new Uri(
                        context.UrlCombine( config[$"{settingsPrefix}_profileurl"].ToString() )
                    )
                };
            }

            if( config.GenerateInbox() )
            {
                profile = profile with
                {
                    Inbox = new Uri( context.GetInboxUrl() )
                };
            }

            if( config.GenerateOutbox() )
            {
                profile = profile with
                {
                    Outbox = new Uri( context.GetOutboxUrl() )
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
                        Name = "Website",
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

            if( config.ContainsKey( $"{settingsPrefix}_created" ) )
            {
                profile = profile with
                {
                    Published = config[$"{settingsPrefix}_created"].ToString()
                };
            }

            if( config.ContainsKey( $"{settingsPrefix}_summary" ) )
            {
                profile = profile with
                {
                    Summary = config[$"{settingsPrefix}_summary"].ToString()
                };
            }

            if( config.ContainsKey( $"title" ) )
            {
                profile = profile with
                {
                    Name = config[$"title"].ToString()
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
                        // ID Must match the Profile's ID.
                        Id = $"{profile.Id}#main-key",
                        Owner = profile.Id.ToString(),
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
