//
//          Copyright Seth Hendrick 2020-2022.
// Distributed under the Microsoft Public License (MS-PL).
//

namespace Pretzel.SethExtensions
{
    public static class SethHtmlFormatter
    {
        public static readonly string ATagRelProperties = @"noopener noreferrer nofollow";

        public static readonly string ATagProperties =
            $@"target=""_blank"" rel=""{ATagRelProperties}""";
    }
}
