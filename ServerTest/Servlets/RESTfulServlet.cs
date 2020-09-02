using HttpServer.Http.Request;
using HttpServer.Http.Response;
using HttpServer.Server.Servlet;
using System;
using System.Collections.Generic;
using System.Text;

namespace ServerTest.Servlets
{
    [WebServlet("product", Path = "/get")]
    [ServletPathPattern("/{id}/{type}")]
    public class RESTfulServlet : HttpServlet
    {
        public override void OnGet(HttpRequest request, HttpResponse response) {
            string id = request.GetParameter("id");
            string type = request.GetParameter("type");
            if(string.IsNullOrEmpty(id) || string.IsNullOrEmpty(type)) {
                response.SendError(System.Net.HttpStatusCode.BadRequest);
                return;
            }
            response.WriteLine("<html><body><h1>");
            response.Write("Product ID: {0} <br> Product Type: {1}", id, type);
            response.WriteLine("</h1></body></html>");
        }
    }
}
