using System;
using System.Collections.Generic;
using System.Text;

namespace HttpServer.Server.Resources
{
    public class HttpResource
    {
        public string BasePath { get; set; }
        public string MIMEType { get; set; }
        public ResourceMappingType MappingType { get; }

        public HttpResource(ResourceMappingType mappingType) {
            MappingType = mappingType;
        }
    }
}
