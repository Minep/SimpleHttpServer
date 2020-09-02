using System;
using System.Collections.Generic;
using System.Text;

namespace HttpServer.Server.Resources
{
    public struct ResourceLocator
    {
        // e.g. A target URL mightbe
        //     /a/b/c/d/e/f/g
        // if we have a mapping such that "/a/b/c/d" -> {Some HttpResource}
        // then "/a/b/c/d" is the MatchedPath, hence
        //    [MatchedPath]/e/f/g
        //                 \____/
        //                   ||
        //               RemainingPath
        public readonly string MatchedPath { get; }
        // Reserve for RESTful support
        public readonly string RemainingPath { get; }

        public readonly HttpResource Resource { get; }

        public ResourceLocator(string matchedPath, string remainingPath, HttpResource resource) {
            MatchedPath = matchedPath;
            RemainingPath = remainingPath;
            Resource = resource;
        }

        
    }
}
