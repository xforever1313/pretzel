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

            string baseUrl = GetBaseUrl( config );

            var profile = new Profile
            {
                Discoverable = true,
                ManuallyApprovesFollowers = false,
                Type = "Service",
                Url = new Uri( baseUrl ),

                // ID must be the same as the URL to this page (its a self-reference).
                Id = context.GetProfileJsonUrl()
            };

            if( TryGetProfileUrl( config, out string profileUrl ) )
            {
                profile = profile with
                {
                    Url = new Uri(
                        context.UrlCombine( profileUrl )
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

            if( config.GetFollowing() is not null )
            {
                profile = profile with
                {
                    Following = new Uri( context.GetFollowingUrl() )
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

            if( TryGetIconUrl( config, out string iconUrl ) )
            {
                profile = profile with
                {
                    Icon = new Uri( iconUrl )
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

            if( TryGetPublicKeyFileLocation( config, out string fileLocation ) )
            {
                string file = Path.Combine(
                    context.SourceFolder,
                    fileLocation
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

        private static string GetBaseUrl( IConfiguration config )
        {
            string? baseUrl = config["url"].ToString();

            // Base URL is required.  Throw if its null.
            ArgumentNullException.ThrowIfNull( baseUrl );

            return baseUrl;
        }

        private static bool TryGetProfileUrl( IConfiguration config, out string profileUrl )
        {
            if( config.ContainsKey( $"{settingsPrefix}_profileurl" ) == false )
            {
                profileUrl = "";
                return false;
            }

            string? value = config[$"{settingsPrefix}_profileurl"].ToString();
            if( value is null )
            {
                profileUrl = "";
                return false;
            }

            profileUrl = value;
            return true;
        }

        private static bool TryGetIconUrl( IConfiguration config, out string iconUrl )
        {
            if( config.ContainsKey( $"{settingsPrefix}_icon" ) == false )
            {
                iconUrl = "";
                return false;
            }

            string? value = config[$"{settingsPrefix}_icon"].ToString();
            if( value is null )
            {
                iconUrl = "";
                return false;
            }

            iconUrl = value;
            return true;
        }

        private static bool TryGetPublicKeyFileLocation( IConfiguration config, out string fileLocation )
        {
            if( config.ContainsKey( $"{settingsPrefix}_publickeyfile" ) == false )
            {
                fileLocation = "";
                return false;
            }

            string? value = config[$"{settingsPrefix}_publickeyfile"].ToString();
            if( value is null )
            {
                fileLocation = "";
                return false;
            }

            fileLocation = value;
            return true;
        }
    }
}
