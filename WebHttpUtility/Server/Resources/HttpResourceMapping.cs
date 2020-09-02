using HttpServer.Http.Request;
using HttpServer.Server.Servlet;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace HttpServer.Server.Resources
{
    public class HttpResourceMapping
    {
        ServerConfig httpConfig;

        public HttpResourceMapping(HttpServerContext context) {
            this.httpConfig = context.ServerConfig ?? throw new ArgumentNullException(nameof(httpConfig));
        }

        Dictionary<string, HttpResource> ResourceMapping = new Dictionary<string, HttpResource>();

        internal bool TryMatch(HttpRequest request, out ResourceLocator resource) {
            foreach (var item in ResourceMapping) {
                if (request.TargetURI.StartsWith(item.Key)) {
                    string remaining = request.TargetURI.Substring(item.Key.Length);
                    resource = new ResourceLocator(item.Key, remaining, item.Value);
                    return true;
                }
            }
            resource = httpConfig.NotFound404Page;
            return false;
        }

        public bool AddMapping(string mappedPath, HttpResource resource) {
            if (ResourceMapping.ContainsKey(mappedPath)) {
                return false;
            }
            ResourceMapping.Add(mappedPath, resource);
            return true;
        }

        public void RegisterServlet<T>() where T : HttpServlet {
            RegisterServlet(typeof(T));
        }

        public void RegisterServlet(Type servletType) {
            try {
                HttpServlet httpServlet = (HttpServlet)Activator.CreateInstance(servletType);

                httpServlet.OnCreated();

                WebServletAttribute servletAttribute
                    = httpServlet.GetType().GetCustomAttribute(typeof(WebServletAttribute)) as WebServletAttribute;
                if (servletAttribute != null) {
                    AddMapping(servletAttribute.GetMappingRoute(), httpServlet);
                }
            }
            catch {
                // TODO Add Logger
            }
        }

        public bool RemoveMapping(string mappedPath) {
            return ResourceMapping.Remove(mappedPath);
        }
    }
}
