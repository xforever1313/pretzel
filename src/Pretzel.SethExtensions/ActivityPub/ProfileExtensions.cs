//
//          Copyright Seth Hendrick 2020-2022.
// Distributed under the Microsoft Public License (MS-PL).
//

using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using System.Text.Json;
using KristofferStrube.ActivityStreams;
using Pretzel.Logic;
using Pretzel.Logic.Templating.Context;

namespace Pretzel.SethExtensions.ActivityPub
{
    public static class ProfileExtensions
    {
        // ---------------- Fields ----------------

        private const string settingsPrefix = ActivityPubPlugin.SettingPrefix;

        // ---------------- Functions ----------------

        public static Service FromSiteContext( SiteContext context )
        {
            IConfiguration config = context.Config;

            string baseUrl = GetBaseUrl( config );

            var extensionData = new Dictionary<string, JsonElement>();
            extensionData.AddMastodonExtensions();

            var profile = new Service
            {
                // ID must be the same as the URL to this page (its a self-reference).
                Id = context.GetProfileJsonUrl(),
                Type = new string[]{ "Service" },
                Url = new Link[]
                { 
                    new Link
                    {
                        Href = new Uri( baseUrl )
                    }
                }
            };

            if( config.GetFollowing() is not null )
            {
                profile.Following = new Link
                {
                    Href = new Uri( context.GetFollowingUrl() )
                };
            }

            string? inbox = context.TryGetInboxUrl();
            if( string.IsNullOrWhiteSpace( inbox ) == false )
            {
                profile.Inbox = new Link
                {
                    Href = new Uri( inbox )
                };
            }

            if( config.GenerateOutbox() )
            {
                profile.Outbox = new Link
                {
                    Href = new Uri( context.GetOutboxUrl() )
                };
            }

            if( config.ContainsKey( $"{settingsPrefix}_username" ) )
            {
                profile.PreferredUsername = config[$"{settingsPrefix}_username"].ToString();
            }

            if( config.ContainsKey( $"title" ) )
            {
                profile.Name = new string[]
                {
                    config[$"title"]?.ToString() ?? ""
                };
            }

            if( config.ContainsKey( $"{settingsPrefix}_summary" ) )
            {
                profile.Summary = new string[]
                {
                    config[$"{settingsPrefix}_summary"]?.ToString() ?? ""
                };
            }

            if( TryGetProfileUrl( config, out string profileUrl ) )
            {
                profile.Url = new Link[]
                { 
                    new Link
                    {
                        Href = new Uri( context.UrlCombine( profileUrl ) )
                    }
                };
            }

            if( config.ContainsKey( $"{settingsPrefix}_created" ) )
            {
                profile.Published = ParseCreatedDate( 
                    config[$"{settingsPrefix}_created"]?.ToString()
                );
            }

            if( TryGetPublicKeyFileLocation( config, out string fileLocation ) )
            {
                string file = Path.Combine(
                    context.SourceFolder,
                    fileLocation
                );

                string publicKeyContents = ReadPublicKey( file );

                var key = new ProfilePublicKey
                {
                    // ID Must match the Profile's ID.
                    Id = $"{profile.Id}#main-key",
                    Owner = profile.Id.ToString(),
                    PublicKeyPem = publicKeyContents
                };

                extensionData["publicKey"] = JsonSerializer.SerializeToElement( key );
            }

            {
                var attachments = new List<IObjectOrLink>();
                attachments.Add( ServiceExtensions.CreateWebsiteAttachment( config["url"].ToString() ) );

                if( config.ContainsKey( "github" ) )
                {
                    attachments.Add( ServiceExtensions.CreateGithubAttachment( config["github"].ToString() ) );
                }

                if( config.ContainsKey( "contact" ) )
                {
                    attachments.Add(
                        new PropertyValue
                        {
                            Name = new string[]{ "Email" },
                            Type = new string[] { "PropertyValue" },
                            Value = @$"<a href=""mailto:{config["contact"]}"">{config["contact"]}</a>"
                        }
                    );
                }

                profile.Attachment = attachments;
            }

            if( config.TryGetIconUrl( out string iconUrl ) )
            {
                profile.AddIcon( iconUrl );
            }

            profile.ExtensionData = extensionData;

            return profile;
        }

        public static DateTime ParseCreatedDate( string? createdDate )
        {
            return DateTime.ParseExact(
                createdDate ?? "",
                "o",
                CultureInfo.InvariantCulture,
                DateTimeStyles.AllowWhiteSpaces | DateTimeStyles.RoundtripKind
            );
        }

        public static string ReadPublicKey( string file )
        {
            var publicKeyBuilder = new StringBuilder();
            foreach( string line in File.ReadAllLines( file ) )
            {
                publicKeyBuilder.Append( line );
                publicKeyBuilder.Append( "\\n" );
            }
            publicKeyBuilder.Remove( publicKeyBuilder.Length - 2, 2 );

            return publicKeyBuilder.ToString();
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
