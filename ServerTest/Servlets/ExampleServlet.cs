using HttpServer.Http.Request;
using HttpServer.Http.Response;
using HttpServer.Server.Servlet;
using System;
using System.Collections.Generic;
using System.Text;

namespace ServerTest.Servlets
{
    [WebServlet("hello")]
    public class ExampleServlet: HttpServlet
    {
        public override void OnCreated() {
            Console.WriteLine("I am created");
        }
        public override void OnGet(HttpRequest request, HttpResponse response) {

            response.WriteLine("<html><body><h1>");

            string userInput = request.GetParameter("name");
            if(userInput == null) {
                response.WriteLine("You dont input any thing");
            }
            else {
                response.WriteLine("You say : {0}", userInput);
            }
            response.WriteLine("</h1></body></html>");
        }
    }
}
