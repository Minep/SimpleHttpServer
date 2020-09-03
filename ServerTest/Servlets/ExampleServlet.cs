using HttpServer.Http.Common.WebIO;
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

            SimplePayloadStream dataStream = response.CreateSimpleStreamWriter();

            dataStream.WriteLine("<html><body><h1>");

            string userInput = request.GetParameter("name");
            if(userInput == null) {
                dataStream.WriteLine("You dont input any thing");
            }
            else {
                dataStream.WriteLine("You say : {0}", userInput);
            }
            dataStream.WriteLine("</h1></body></html>");
        }
    }
}
