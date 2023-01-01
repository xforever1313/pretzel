//
//          Copyright Seth Hendrick 2020-2022.
// Distributed under the Microsoft Public License (MS-PL).
//

using System.Text.Json.Serialization;

namespace Pretzel.SethExtensions.ActivityPub
{
    public class PropertyValue : KristofferStrube.ActivityStreams.Object
    {
        [JsonPropertyName( "value" )]
        [JsonIgnore( Condition = JsonIgnoreCondition.WhenWritingDefault )]
        public string? Value { get; set; }
    }
}
