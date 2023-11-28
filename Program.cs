using System;
using System.Net;
using System.Threading;

namespace ss_api;


class Program
{
    static void Main(string[] args)
    {
        string url = "http://localhost:8080/";

        HttpListener listener = new HttpListener();
        listener.Prefixes.Add(url);

        Console.WriteLine($"Listening for requests on {url}");

        listener.Start();

        ThreadPool.QueueUserWorkItem((o) =>
        {
            Console.WriteLine("Server is running...");

            while (listener.IsListening)
            {
                try
                {
                    HttpListenerContext context = listener.GetContext();

                    // Delegate request handling to a separate method
                    ThreadPool.QueueUserWorkItem(HandleRequest, context);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Exception: {ex.Message}");
                }
            }
        });

        Console.ReadLine();
        listener.Stop();
    }

    static void HandleRequest(object state)
    {
        HttpListenerContext context = (HttpListenerContext)state;
        HttpListenerRequest request = context.Request;
        HttpListenerResponse response = context.Response;

        // Extract the HTTP method and path from the request
        string httpMethod = request.HttpMethod;
        string path = request.Url.AbsolutePath;

        Console.WriteLine($"Request: {httpMethod} {path}");

        // Route the request based on the method and path
        if (httpMethod == "GET" && path == "/")
        {
            // Handle GET request for the root path
            HandleGetRoot(response);
        }
        else if (httpMethod == "POST" && path == "/submit")
        {
            // Handle POST request to the /submit path
            HandlePostSubmit(request, response);
        }
        else
        {
            // Handle 404 - Not Found
            HandleNotFound(response);
        }

        // Close the output stream
        response.OutputStream.Close();
    }

    static void HandleGetRoot(HttpListenerResponse response)
    {
        // Respond with a simple HTML message for the root path
        string responseString = "<html><body>Hello, World! (GET)</body></html>";
        byte[] buffer = System.Text.Encoding.UTF8.GetBytes(responseString);

        response.ContentType = "text/html";
        response.ContentLength64 = buffer.Length;

        response.OutputStream.Write(buffer, 0, buffer.Length);
    }

    static void HandlePostSubmit(HttpListenerRequest request, HttpListenerResponse response)
    {
        // Handle the POST request to the /submit path
        // Read data from the request body, process it, and respond
       

        using (var reader = new System.IO.StreamReader(request.InputStream))
        {
            string requestBody = reader.ReadToEnd();
            Console.WriteLine($"POST data: {requestBody}");

            // Respond with a simple HTML message
            string responseString = "<html><body>Received POST data: " + requestBody + " (POST)</body></html>";
            byte[] buffer = System.Text.Encoding.UTF8.GetBytes(responseString);

            response.ContentType = "text/html";
            response.ContentLength64 = buffer.Length;

            response.OutputStream.Write(buffer, 0, buffer.Length);
        }
    }

    static void HandleNotFound(HttpListenerResponse response)
    {
        // Respond with a 404 - Not Found error
        response.StatusCode = 404;
        string responseString = "<html><body>404 - Not Found</body></html>";
        byte[] buffer = System.Text.Encoding.UTF8.GetBytes(responseString);

        response.ContentType = "text/html";
        response.ContentLength64 = buffer.Length;

        response.OutputStream.Write(buffer, 0, buffer.Length);
    }
}

