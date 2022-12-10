//
//          Copyright Seth Hendrick 2020-2022.
// Distributed under the Microsoft Public License (MS-PL).
//

using System.Composition;
using System.IO;
using System.Text.Json;
using Pretzel.Logic.Extensibility;
using Pretzel.Logic.Templating.Context;

namespace Pretzel.SethExtensions.ActivityPub
{
    [Export( typeof( IBeforeProcessingTransform ) )]
    public sealed class ActivityPubPlugin : IBeforeProcessingTransform
    {
        // ---------------- Fields ----------------

        internal const string SettingPrefix = "actpub";

        // ---------------- Functions ----------------

        /// <summary>
        /// Settings used to transform.  Each setting
        /// in the site's _config.yml file should start
        /// with actpub_ followed by this setting to configure activity pub:
        ///
        /// directory - Where to output the activity pub files.
        ///             If this is not specified, no activity pub files
        ///             will be generated.
        ///
        ///             The url of activity pub stuff will be the site url
        ///             followed by this directory.
        ///
        /// outbox    - Set to true to enable outbox behavior,
        ///             which is just the latest posts.
        ///
        /// inbox     - Set to true to have the inbox show up in the profile
        ///             JSON.  However, no inbox will be created since
        ///             this is a static site.
        ///
        /// following - string list of who the "account" should follow.
        ///
        /// profileurl - The relative URL from the base url of where
        ///              the "profile" should be.  Usually a URL
        ///              to all posts.  If not specified, this becomes
        ///              the base URL.
        /// 
        /// username  - The username that is used.  This becomes the
        ///             "preferred username" used in activity pub.
        ///
        /// icon - The URL to the icon that is used for a profile.
        /// 
        /// created - Timestamp of when this profile was created.
        ///
        /// summary - The summary of the profile.
        ///
        /// publickeyfile - path relative to <see cref="SiteContext.SourceFolder"/>
        ///                 that contains the public key for the account.
        ///
        ///
        /// The base site settings are also used:
        /// title - Becomes the name that is displayed on a profile.
        /// github - Gets shown on the profile.
        /// contact - Gets shown on the profile as the email.
        /// urlnohttp - Gets used with webfinger.
        /// </summary>
        /// <param name="context"></param>
        public void Transform( SiteContext context )
        {
            if( context.Config.ContainsKey( $"{SettingPrefix}_directory" ) == false )
            {
                return;
            }

            DirectoryInfo outputDirectory = new DirectoryInfo(
                Path.Combine( context.OutputFolder, context.Config["actpub_directory"].ToString() )
            );

            if( outputDirectory.Exists == false )
            {
                Directory.CreateDirectory( outputDirectory.FullName );
            }

            // Output directory looks like this:
            // - outbox.json
            // - inbox.json
            // - following.json
            // - profile.json
            // - webfinger
            WriteWebFinger( outputDirectory, context );
            WriteProfile( outputDirectory, context );
        }

        private static void WriteWebFinger( DirectoryInfo outputDir, SiteContext context )
        {
            var webFinger = WebFingerExtensions.FromSiteContext( context );
            string jsonString = JsonSerializer.Serialize(
                webFinger,
                new JsonSerializerOptions
                {
                    WriteIndented = true
                }
            );

            FileInfo outFile = new FileInfo(
                Path.Combine( outputDir.FullName, "webfinger" )
            );
            File.WriteAllText( outFile.FullName, jsonString );
        }

        private static void WriteProfile( DirectoryInfo outputDir, SiteContext context )
        {
            var profile = ProfileExtensions.FromSiteContext( context );
            string jsonString = JsonSerializer.Serialize(
                profile,
                new JsonSerializerOptions
                {
                    WriteIndented = true
                }
            );

            FileInfo outFile = new FileInfo(
                Path.Combine( outputDir.FullName, "profile.json" )
            );
            File.WriteAllText( outFile.FullName, jsonString );
        }
    }
}
