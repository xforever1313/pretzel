//
//          Copyright Seth Hendrick 2020-2022.
// Distributed under the Microsoft Public License (MS-PL).
//

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;
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

        /// <summary>
        /// The thumbnail scale of the specific image.
        /// If this is specified, it overrides <see cref="ImageGalleryConfig.ThumbnailScale"/>
        /// </summary>
        public float? ThumbnailScale { get; init; } = null;

        /// <summary>
        /// Set to non-null value if the photo was taken on a different
        /// date as the post date.
        /// </summary>
        public DateOnly? PhotoDate { get; init; } = null;

        /// <summary>
        /// If <see cref="PhotoDate"/> is an estimate or not.
        /// Ignore if <see cref="PhotoDate"/> is null.
        /// </summary>
        public bool DateIsEstimate { get; init; } = false;

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

            if( this.ThumbnailScale.HasValue )
            {
                if( ( this.ThumbnailScale < 0 ) || ( this.ThumbnailScale > 100 ) )
                {
                    errorList.Add(
                        $"{nameof( this.ThumbnailScale )} must be between 0 and 100 (inclusive).  Got: {this.ThumbnailScale}"
                    );
                }
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
                else if( "Caption".Equals( childElement.Name.LocalName ) )
                {
                    imageInfo = imageInfo with { Caption = childElement.Value };
                }
                else if( "ThumbnailScale".Equals( childElement.Name.LocalName ) )
                {
                    if( float.TryParse( childElement.Value, out float scale ) )
                    {
                        imageInfo = imageInfo with { ThumbnailScale = scale };
                    }
                    else
                    {
                        throw new ArgumentException(
                            $"XML file contains non-floating point value for ThumbnailScale: {childElement.Value}",
                            nameof( element )
                        );
                    }
                }
                else if( "Date".Equals( childElement.Name.LocalName ) )
                {
                    bool dateEstimate = false;
                    int? year = null;
                    int? month = null;
                    int? day = null;
                    foreach( XAttribute dateAttr in childElement.Attributes() )
                    {
                        if( "Estimate".Equals( dateAttr.Name ) )
                        {
                            dateEstimate = bool.Parse( dateAttr.Value );
                        }
                    }

                    foreach( XElement dateElement in childElement.Elements() )
                    {
                        if( "Year".Equals( dateElement.Name.LocalName ) )
                        {
                            year = int.Parse( dateElement.Value );
                        }
                        else if( "Month".Equals( dateElement.Name.LocalName ) )
                        {
                            month = int.Parse( dateElement.Value );
                        }
                        else if( "Day".Equals ( dateElement.Name.LocalName ) )
                        {
                            day = int.Parse( dateElement.Value );
                        }
                    }

                    if( ( year is null ) || ( month is null ) || ( day is null ) )
                    {
                        throw new XmlException(
                            $"Incomplete date on photo {imageInfo.FileName}"
                        );
                    }

                    imageInfo = imageInfo with
                    {
                        PhotoDate = new DateOnly( year.Value, month.Value, day.Value ),
                        DateIsEstimate = dateEstimate
                    };
                }
            }

            return imageInfo;
        }
    }
}
