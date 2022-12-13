//
//          Copyright Seth Hendrick 2020-2022.
// Distributed under the Microsoft Public License (MS-PL).
//

using System.Text.Json.Serialization;

namespace Pretzel.SethExtensions.ActivityPub
{
    public record class WebFinger
    {
        [JsonPropertyName( "subject" )]
        [JsonIgnore( Condition = JsonIgnoreCondition.WhenWritingNull )]
        public string? Subject { get; init; }

        [JsonPropertyName( "links" )]
        [JsonIgnore( Condition = JsonIgnoreCondition.WhenWritingNull )]
        public WebFingerLinks[]? Links { get; init; }
    }
}
