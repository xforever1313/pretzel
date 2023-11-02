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
    public record class ImageGalleryConfig
    {
        // ---------------- Properties ---------------

        public string Id { get; init; } = "";

        /// <summary>
        /// The location of the folder that contains the images
        /// of the gallery.
        /// </summary>
        public string InputImageDirectory { get; init; } = "";

        /// <summary>
        /// The URI path that contains the thumbnails
        /// of the generated images.
        /// </summary>
        public string ThumbnailOutputFolder { get; init; } = "";

        /// <summary>
        /// Scale at which to reduce the original image
        /// when generating thumbnails.  For example,
        /// if this is set to 75%, the original image will
        /// be scaled down to 75%.
        /// </summary>
        public float ThumbnailScale { get; init; } = .75f;

        /// <summary>
        /// Information about the Images in the gallery.
        /// </summary>
        public IReadOnlyList<ImageInfo> ImageInfo { get; init; } = Array.Empty<ImageInfo>();

        // ---------------- Functions ----------------

        public void Validate()
        {
            var errorList = new List<string>();

            if( string.IsNullOrWhiteSpace( this.Id ) )
            {
                errorList.Add( $"{nameof( this.Id )} can not be null or empty." );
            }

            if( string.IsNullOrWhiteSpace( this.InputImageDirectory ) )
            {
                errorList.Add( $"{nameof( this.InputImageDirectory )} can not be null or empty." );
            }
            else if( Directory.Exists( this.InputImageDirectory ) == false )
            {
                errorList.Add( $"Directory '{this.InputImageDirectory}' does not exist." );
            }

            if( ( this.ThumbnailScale < 0 ) || ( this.ThumbnailScale > 100 ) )
            {
                errorList.Add(
                    $"{nameof( this.ThumbnailScale )} must be between 0 and 100 (inclusive).  Got: {this.ThumbnailScale}"
                );
            }

            if( string.IsNullOrWhiteSpace( this.ThumbnailOutputFolder ) )
            {
                errorList.Add( $"{nameof( this.ThumbnailOutputFolder )} can not be null or empty." );
            }

            foreach( ImageInfo imageInfo in this.ImageInfo )
            {
                if( imageInfo.TryValidate( out string errorString ) == false )
                {
                    errorList.Add( errorString );
                }
            }

            if( errorList.Any() )
            {
                string errorString = string.Join( Environment.NewLine, errorList );
                throw new InvalidOperationException(
                    $"Error when validating {this.GetType().Name}:{Environment.NewLine}{errorString}"
                );
            }
        }
    }

    internal static class ImageGalleryConfigExtensions
    {
        // ---------------- Fields ----------------

        private const string xmlRootName = "ImageGallery";

        // ---------------- Functions ----------------

        public static ImageGalleryConfig FromXml( FileInfo fileName )
        {
            XDocument doc = XDocument.Load( fileName.FullName );

            XElement? root = doc.Root;
            if( root is null )
            {
                throw new InvalidOperationException( "Somehow, the csproj root node is null" );
            }
            else if( xmlRootName.Equals( root.Name.LocalName ) == false )
            {
                throw new ArgumentException(
                    $"Root XML node not what expected.  Expected: {xmlRootName}, Actual: {root.Name.LocalName}",
                    nameof( fileName )
                );
            }

            var config = new ImageGalleryConfig
            {
                Id = Path.GetFileNameWithoutExtension( fileName.Name )
            };

            foreach( XAttribute attr in root.Attributes() )
            {
                if( "ImageDir".Equals( attr.Name.LocalName ) )
                {
                    config = config with
                    {
                        InputImageDirectory = attr.Value
                    };
                }
                else if( "ThumbnailOutputDir".Equals( attr.Name.LocalName ) )
                {
                    config = config with
                    {
                        ThumbnailOutputFolder = attr.Value
                    };
                }
                else if( "ThumbnailScale".Equals( attr.Name.LocalName ) )
                {
                    if( float.TryParse( attr.Value, out float scale ) )
                    {
                        config = config with { ThumbnailScale = scale };
                    }
                    else
                    {
                        throw new ArgumentException(
                            $"XML file contains non-floating point value for ThumbnailScale: {attr.Value}",
                            nameof( fileName )
                        );
                    }
                }
            }

            var images = new List<ImageInfo>();
            foreach( XElement element in root.Elements() )
            {
                if( ImageInfoExtensions.XmlRootName.Equals( element.Name.LocalName ) )
                {
                    images.Add( ImageInfoExtensions.FromXml( config, element ) );
                }
            }

            config = config with { ImageInfo = images.AsReadOnly() };

            return config;
        }
    }
}
