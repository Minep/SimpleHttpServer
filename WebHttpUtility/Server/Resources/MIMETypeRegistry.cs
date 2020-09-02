using HttpServer.Common;
using System;
using System.Collections.Generic;
using System.Text;

namespace HttpServer.Server.Resources
{
    public class MIMETypeRegistry
    {

        private static volatile MIMETypeRegistry MIMETypeRegistryInstance = null;
        private static readonly object locker = new object();

        public static MIMETypeRegistry Instance
        {
            get {
                if (MIMETypeRegistryInstance == null) {
                    lock (locker) {
                        MIMETypeRegistryInstance = MIMETypeRegistryInstance ?? new MIMETypeRegistry();
                    }
                }
                return MIMETypeRegistryInstance;
            }
        }

        private MIMETypeRegistry() {

        }

        public const string DEFAULT_MIME = "application/octet-stream";

        Dictionary<string, string> Registry = new Dictionary<string, string>()
        {
            {"png", "image/png" },
            {"bmp", "image/bmp" },
            {"jpg", "image/jpeg" },
            {"jpeg", "image/jpeg" },
            {"jfif", "image/jpeg" },
            {"svg", "image/svg+xml" },
            {"tif", "image/tif" },
            {"tiff", "image/tif" },
            {"gif", "image/gif" },

            {"txt", "text/plain" },
            {"json", "text/plain" },
            {"css", "text/css" },
            {"html", "text/html" },
            {"js", "text/javascript" },

            {"mp3", "audio/mpeg" },
            {"mp4", "video/mpeg" },
            {"flac", "audio/flac" },
            {"ogg", "audio/ogg" },
        };

        public string TranslateToMIME(string extension_no_dot) {
            if (Registry.ContainsKey(extension_no_dot)) {
                return Registry[extension_no_dot];
            }
            return DEFAULT_MIME;
        }

        public static MIMEType GetMIMEType(string mime) {
            string[] type = mime.Split('/');
            switch (type[0]) {
                case "text":return MIMEType.TEXT;
                case "image":return MIMEType.IMAGE;
                case "audio":return MIMEType.AUDIO;
                case "application":return MIMEType.APPLICATION;
                case "video":return MIMEType.VIDEO;
                default:
                    return MIMEType.APPLICATION;
            }
        }

        public bool RegisterMIMEType(string fileExtension, string typeAssociated) {
            if (!Registry.ContainsKey(fileExtension)) {
                Registry.Add(fileExtension, typeAssociated);
                return true;
            }
            return false;
        }
    }
}
