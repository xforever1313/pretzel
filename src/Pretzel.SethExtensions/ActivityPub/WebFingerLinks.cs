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
        public string? Rel { get; set; }

        [JsonPropertyName( "type" )]
        public string? Type { get; set; }

        [JsonPropertyName( "href" )]
        [JsonIgnore( Condition = JsonIgnoreCondition.WhenWritingNull )]
        public Uri? Href { get; set; }
    }
}
