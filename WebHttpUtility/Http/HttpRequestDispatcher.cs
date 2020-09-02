using HttpServer.Common;
using HttpServer.Http.Request;
using HttpServer.Http.Response;
using HttpServer.Server;
using HttpServer.Server.Resources;
using HttpServer.Server.Servlet;
using HttpServer.Utils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;

namespace HttpServer.Http
{
    public class HttpRequestDispatcher
    {
        HttpServerContext ServerContext;

        public HttpRequestDispatcher(HttpServerContext serverContext) {
            ServerContext = serverContext;
        }

        public void Dispatch(HttpRequest request, HttpResponse httpResponse) {
            string absPath = ServerContext.ServerConfig.DocumentRoot + request.TargetURI;
            if (Directory.Exists(absPath)) {
                // Is directory
                DoDirectory(absPath, httpResponse);
            }
            else if (File.Exists(absPath)) {
                // Is file
                DoFile(absPath, httpResponse);
            }
            else {
                // Must be other mapped resource
                ResourceLocator locator;
                if(ServerContext.ResourceMapping.TryMatch(request, out locator)) {
                    HttpResource resource = locator.Resource;
                    switch (resource.MappingType) {
                        case ResourceMappingType.DIRECTORY:
                            DoDirectory(resource.BasePath, httpResponse);
                            break;
                        case ResourceMappingType.FILE:
                            DoFile(resource.BasePath, httpResponse);
                            break;
                        case ResourceMappingType.SERVLET:
                            InvokeServlet(resource as HttpServlet, locator.RemainingPath, request, httpResponse);
                            break;
                        default:
                            break;
                    }
                }
                else {
                    httpResponse.SetStatus(System.Net.HttpStatusCode.NotFound);
                    DoFile(locator.Resource.BasePath, httpResponse);
                }
            }
            httpResponse.Flush();
            request.Dispose();
            httpResponse.Dispose();
        }

        private void DoDirectory(string absPath, HttpResponse httpResponse) {
            if (!GetFileResponse(absPath + "/index.html", httpResponse)) {
                GetFileResponse(ServerContext.ServerConfig.WelcomePage.Resource.BasePath, httpResponse);
            }
        }

        private void DoFile(string absPath, HttpResponse httpResponse) {
            if (!GetFileResponse(absPath, httpResponse)) {
                GetFileResponse(ServerContext.ServerConfig.WelcomePage.Resource.BasePath, httpResponse);
            }
        }

        private bool GetFileResponse(string fileAbsPath, HttpResponse httpResponse) {
            if (!File.Exists(fileAbsPath)) return false;
            string ext = Path.GetExtension(fileAbsPath);
            if (ext.Length > 0) {
                ext = ext.Substring(1);
            }

            httpResponse.ContentType = MIMETypeRegistry.Instance.TranslateToMIME(ext);
            if(MIMETypeRegistry.GetMIMEType(httpResponse.ContentType) == HttpServer.Common.MIMEType.TEXT) {
                httpResponse.ContentCharset = "utf-8";
            }

            using (FileStream fr = new FileStream(fileAbsPath, FileMode.Open, FileAccess.Read, FileShare.Read)) {
                httpResponse.CopyStreamFrom(fr);
            }
            return true;
        }

        private void InvokeServlet(HttpServlet resource, string remainingURL, HttpRequest request, HttpResponse response) {
            response.SetHeader(HeaderFields.CacheControl, HeaderFields.CacheControl_NoCahe);

            ServletPathPatternAttribute pattern =
                resource.GetType().GetCustomAttribute(typeof(ServletPathPatternAttribute))
                    as ServletPathPatternAttribute;
            if (pattern != null) {
                foreach (var item in PathURLExtractor.ExtractPathVariable(pattern.PathPattern, remainingURL)) {
                    request.AddParameter(item.Key, item.Value);
                }
            }

            resource?.Dispatch(request, response);
        }
    }
}
