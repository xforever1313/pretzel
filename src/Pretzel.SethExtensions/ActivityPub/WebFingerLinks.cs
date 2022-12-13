//
//          Copyright Seth Hendrick 2020-2022.
// Distributed under the Microsoft Public License (MS-PL).
//

using System;
using System.Text.Json.Serialization;

namespace Pretzel.SethExtensions.ActivityPub
{
    public record class WebFingerLinks
    {
        [JsonPropertyName( "rel" )]
        public string Rel => "self";

        [JsonPropertyName( "type" )]
        public string Type => "application/activity+json";

        [JsonPropertyName( "href" )]
        [JsonIgnore( Condition = JsonIgnoreCondition.WhenWritingNull )]
        public Uri? Href { get; set; }
    }
}
