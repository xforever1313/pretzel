//
//          Copyright Seth Hendrick 2020-2022.
// Distributed under the Microsoft Public License (MS-PL).
//

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;

namespace Pretzel.SethExtensions.ImageGallery
{
    public record class ImageInfo
    {
        // ---------------- Properties ----------------

        public int Index { get; init; } = -1;

        /// <summary>
        /// The name of the file within the gallery folder.
        /// </summary>
        public string FileName { get; init; } = "";

        /// <summary>
        /// Alt text to the image.  Null means not specified.
        /// </summary>
        public string? Alt { get; init; } = null;

        /// <summary>
        /// Caption of the image.  Null means not specified.
        /// </summary>
        public string? Caption { get; init; } = null;

        public string ThumbnailFileName => $"{Path.GetFileNameWithoutExtension( this.FileName )}_thumb.jpg";

        // ---------------- Functions ----------------

        public string SafeGetAlt()
        {
            return this.Alt ?? "";
        }

        public string SafeGetCaption()
        {
            return this.Caption ?? "";
        }

        public bool TryValidate( out string errorString )
        {
            var errorList = new List<string>();

            if( string.IsNullOrWhiteSpace( this.FileName ) )
            {
                errorList.Add( $"{nameof( this.FileName )} can not be null or empty." );
            }

            if( string.IsNullOrWhiteSpace( this.ThumbnailFileName ) )
            {
                errorList.Add( $"{nameof( this.ThumbnailFileName )} can not be null or empty." );
            }

            if( errorList.Any() )
            {
                errorString = string.Join( Environment.NewLine, errorList );
                return false;
            }
            else
            {
                errorString = "";
                return true;
            }
        }
    }

    internal static class ImageInfoExtensions
    {
        // ---------------- Fields ----------------

        internal const string XmlRootName = "Image";

        // ---------------- Functions ----------------

        public static ImageInfo FromXml( int index, XElement element )
        {
            var imageInfo = new ImageInfo
            {
                Index = index
            };

            foreach( XAttribute attr in element.Attributes() )
            {
                if( "FileName".Equals( attr.Name.LocalName ) )
                {
                    imageInfo = imageInfo with
                    {
                        FileName = attr.Value
                    };
                }
            }

            foreach( XElement childElement in element.Elements() )
            {
                if( "Alt".Equals( childElement.Name.LocalName ) )
                {
                    imageInfo = imageInfo with { Alt = childElement.Value };
                }
                if( "Caption".Equals( childElement.Name.LocalName ) )
                {
                    imageInfo = imageInfo with { Caption = childElement.Value };
                }
            }

            return imageInfo;
        }
    }
}
