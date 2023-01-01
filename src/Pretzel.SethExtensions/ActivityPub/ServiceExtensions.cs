//
//          Copyright Seth Hendrick 2020-2022.
// Distributed under the Microsoft Public License (MS-PL).
//

using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using KristofferStrube.ActivityStreams;

namespace Pretzel.SethExtensions.ActivityPub
{
    public static class ServiceExtensions
    {
        public static Dictionary<string, JsonElement> AddMastodonExtensions(
            this Dictionary<string, JsonElement> dict
        )
        {
            dict["discoverable"] = JsonSerializer.SerializeToElement( true );
            dict["manuallyApprovesFollowers"] = JsonSerializer.SerializeToElement( false );
            dict["@context"] = JsonSerializer.SerializeToElement(
                new object[]
                {
                    "https://www.w3.org/ns/activitystreams",
                    "https://w3id.org/security/v1",
                    new PropertyValueSchema()
                    {
                        PropertyValue = "schema:PropertyValue",
                        Value = "schema:value"
                    }
                }
            );

            return dict;
        }

        public static IObjectOrLink CreateWebsiteAttachment( string? url )
        {
            ArgumentNullException.ThrowIfNull( url );

            return new PropertyValue
            {
                Name = new string[] { "Website" },
                Type = new string[] { "PropertyValue" },
                Value = GetAttachmentUrl( url )
            };
        }

        public static IObjectOrLink CreateGithubAttachment( string? url )
        {
            ArgumentNullException.ThrowIfNull( url );

            return new PropertyValue
            {
                Name = new string[] { "GitHub" },
                Type = new string[] { "PropertyValue" },
                Value = GetAttachmentUrl( url )
            };
        }

        public static string GetAttachmentUrl( string? baseUrl )
        {
            ArgumentNullException.ThrowIfNull( baseUrl );

            return
                @$"<a href=""{baseUrl}"" rel=""me nofollow noopener noreferrer"" target=""_blank"">{baseUrl}</a>";
        }

        public static Service AddIcon( this Service profile, string iconUrl )
        {
            profile.Icon = new Image[]
            {
                new Image
                {
                    Type = new string[]{ "Image" },
                    MediaType = $"image/{Path.GetExtension( iconUrl ).TrimStart( '.' )}",
                    Url = new Link[]
                    {
                        new Link
                        {
                            Href = new Uri( iconUrl )
                        }
                    }
                }
            };

            return profile;
        }
    }
}
