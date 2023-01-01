//
//          Copyright Seth Hendrick 2020-2022.
// Distributed under the Microsoft Public License (MS-PL).
//

using System.Text.Json.Serialization;
using KristofferStrube.ActivityStreams.JsonLD;

namespace Pretzel.SethExtensions.ActivityPub
{
    public class PropertyValueSchema : ITermDefinition
    {
        [JsonPropertyName( "PropertyValue" )]
        [JsonIgnore( Condition = JsonIgnoreCondition.WhenWritingDefault )]
        public string? PropertyValue { get; set; }

        [JsonPropertyName( "value" )]
        [JsonIgnore( Condition = JsonIgnoreCondition.WhenWritingDefault )]
        public string? Value { get; set; }
    }
}
