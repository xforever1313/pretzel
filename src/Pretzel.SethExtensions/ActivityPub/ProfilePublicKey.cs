//
//          Copyright Seth Hendrick 2022.
// Distributed under the Boost Software License, Version 1.0.
//    (See accompanying file LICENSE_1_0.txt or copy at
//          http://www.boost.org/LICENSE_1_0.txt)
//

using System.Text.Json.Serialization;

namespace Pretzel.SethExtensions.ActivityPub
{
    public record class ProfilePublicKey
    {
        [JsonPropertyName( "id" )]
        [JsonIgnore( Condition = JsonIgnoreCondition.WhenWritingNull )]
        public string? Id { get; init; }

        [JsonPropertyName( "owner" )]
        [JsonIgnore( Condition = JsonIgnoreCondition.WhenWritingNull )]
        public string? Owner { get; init; }

        [JsonPropertyName( "publicKeyPem" )]
        [JsonIgnore( Condition = JsonIgnoreCondition.WhenWritingNull )]
        public string? PublicKeyPem { get; init; }
    }
}
