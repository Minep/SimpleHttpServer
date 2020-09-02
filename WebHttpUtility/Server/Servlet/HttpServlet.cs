using HttpServer.Common;
using HttpServer.Http.Request;
using HttpServer.Http.Response;
using HttpServer.Server.Resources;
using System;
using System.Collections.Generic;
using System.Text;

namespace HttpServer.Server.Servlet
{
    public class HttpServlet : HttpResource
    {
        public HttpServlet() : base(ResourceMappingType.SERVLET) {
        }

        private readonly HttpMethod[] supported = new HttpMethod[]
        {
            HttpMethod.GET,
            HttpMethod.POST,
            HttpMethod.PUT,
            HttpMethod.DELETE
        };
        public virtual HttpMethod[] SupportedMethods { get => supported; }
        public virtual void OnCreated() {

        }

        public void Dispatch(HttpRequest request, HttpResponse response) {
            //Set default content
            response.ContentType = "text/html";
            response.ContentCharset = "utf-8";
            switch (request.Method) {
                case HttpMethod.GET:
                case HttpMethod.HEAD:
                    OnGet(request, response);
                    break;
                case HttpMethod.POST:
                    OnPost(request, response);
                    break;
                case HttpMethod.PUT:
                    OnPut(request, response);
                    break;
                case HttpMethod.DELETE:
                    OnDelete(request, response);
                    break;
                case HttpMethod.OPTIONS:
                    StringBuilder builder = new StringBuilder();
                    builder.AppendJoin(',', SupportedMethods);
                    response.SetStatus(System.Net.HttpStatusCode.OK);
                    response.SetHeader(HeaderFields.Allow, builder.ToString());
                    break;
                default:
                    break;
            }
            response.RestoreDefaulHeader();
        }

        public virtual void OnGet(HttpRequest request, HttpResponse response) {

        }
        public virtual void OnPost(HttpRequest request, HttpResponse response) {

        }
        public virtual void OnPut(HttpRequest request, HttpResponse response) {

        }
        public virtual void OnDelete(HttpRequest request, HttpResponse response) {

        }
    }
}
