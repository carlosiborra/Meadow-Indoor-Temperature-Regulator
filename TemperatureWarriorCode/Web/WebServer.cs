using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Threading;
using System.Runtime.CompilerServices;
using Meadow.Units;
using System.Xml.Linq;
using YamlDotNet.Core.Tokens;
using Meadow.Foundation.Graphics;
using Meadow.Peripherals.Displays;
using System.Collections.Specialized;

namespace TemperatureWarriorCode.Web
{
    public class WebServer
    {

        private IPAddress _ip = null;
        private int _port = -1;
        private bool _runServer = true;
        private static HttpListener _listener;
        private static int _pageViews = 0;
        private static int _requestCount = 0;
        private static bool ready = false;
        private static readonly string pass = "pass";
        private static string message = "";


        /// <summary>
        /// Delegate for the CommandReceived event.
        /// </summary>
        public delegate void CommandReceivedHandler(object source, WebCommandEventArgs e);

        /// <summary>
        /// CommandReceived event is triggered when a valid command (plus parameters) is received.
        /// Valid commands are defined in the AllowedCommands property.
        /// </summary>
        public event CommandReceivedHandler CommandReceived;

        public string Url
        {
            get
            {
                string ip = _ip?.ToString() ?? "127.0.0.1";  // Convert _ip to string if it's not null; otherwise, use "127.0.0.1".
                return $"http://{ip}:{_port}/";
            }
        }

        public WebServer(IPAddress ip, int port)
        {
            _ip = ip ?? throw new ArgumentNullException(nameof(ip), "IP Address cannot be null");
            _port = port > 0 ? port : throw new ArgumentOutOfRangeException(nameof(port), "Port must be positive");
        }


        public async void Start()
        {
            if (_listener == null)
            {
                _listener = new HttpListener();
                _listener.Prefixes.Add(Url);
                _listener.Start();
                Console.WriteLine($"The url of the webserver is {Url}");
            }

            // Handle requests
            try
            {
                while (_runServer)
                {
                    HttpListenerContext ctx = await _listener.GetContextAsync();
                    await HandleIncomingConnections(ctx);
                }
            }
            finally
            {
                _listener.Close();
            }
        }

        public async void Stop()
        {
            _runServer = false;
        }

        private async Task HandleIncomingConnections(HttpListenerContext ctx)
        {

            await Task.Run(async () =>
            {
                // While a user hasn't visited the `shutdown` url, keep on handling requests
                while (_runServer)
                {

                    // Will wait here until we hear from a connection
                    HttpListenerContext ctx = await _listener.GetContextAsync();

                    // Peel out the requests and response objects
                    HttpListenerRequest req = ctx.Request;
                    HttpListenerResponse resp = ctx.Response;

                    // Print out some info about the request
                    Console.WriteLine($"Request #: {++_requestCount}");
                    Console.WriteLine(req.Url);
                    Console.WriteLine(req.HttpMethod);
                    Console.WriteLine(req.UserHostName);
                    Console.WriteLine(req.UserAgent);
                    Console.WriteLine();


                    try
                    {
                        // If `shutdown` url requested w/ POST, then shutdown the server after serving the page
                        if (req.HttpMethod == "POST" && req.Url.AbsolutePath == "/shutdown")
                        {
                            Console.WriteLine("Shutdown requested");
                            _runServer = false;
                            resp.StatusCode = 200;
                            resp.StatusDescription = "OK";
                        }
                        else if (req.HttpMethod == "GET" && req.Url.AbsolutePath == "/setparams")
                        {
                            //Get parameters
                            NameValueCollection queryParams = req.QueryString;
                            if (queryParams["pass"] != null && queryParams["temp_max"] != null && queryParams["temp_min"] != null && queryParams["display_refresh"] != null && queryParams["refresh"] != null && queryParams["round_time"] != null)
                            {
                                string pass_temp = queryParams["pass"];
                                Data.temp_max = queryParams["temp_max"].Split(";");
                                Data.temp_min = queryParams["temp_min"].Split(";");
                                Data.display_refresh = Int16.Parse(queryParams["display_refresh"]);
                                Data.refresh = Int16.Parse(queryParams["refresh"]);
                                Data.round_time = queryParams["round_time"].Split(";");

                                if (string.Equals(pass, pass_temp))
                                {
                                    ready = tempCheck(Data.temp_max, false) && tempCheck(Data.temp_min, true);
                                    if (ready)
                                    {
                                        message = $"{{\"timestamp\":{DateTimeOffset.UtcNow.ToUnixTimeSeconds()},\"mensaje\":\"Los parametros se han cambiado satisfactoriamente. Todo preparado.\"}}";
                                        resp.StatusCode = 200;
                                        resp.StatusDescription = "OK";
                                    }
                                    else
                                    {
                                        message = $"{{\"timestamp\":{DateTimeOffset.UtcNow.ToUnixTimeSeconds()},\"mensaje\":\"El rango de temperatura maximo es entre 30 y 12 grados C.\"}}";
                                        resp.StatusCode = 400;
                                        resp.StatusDescription = "Bad Request";
                                    }
                                }
                                else
                                {
                                    message = $"{{\"timestamp\":{DateTimeOffset.UtcNow.ToUnixTimeSeconds()},\"mensaje\":\"La contrasena es incorrecta.\"}}";
                                    resp.StatusCode = 401;
                                    resp.StatusDescription = "Unauthorized";
                                }
                            }
                            else
                            {
                                resp.StatusCode = 400;
                                resp.StatusDescription = "Bad Request";
                            }
                        }
                        else if (req.HttpMethod == "GET" && req.Url.AbsolutePath == "/start")
                        {

                            // Start the round
                            Thread ronda = new Thread(MeadowApp.StartRound);
                            ronda.Start();

                            // Wait for the round to finish
                            while (Data.is_working)
                            {
                                Thread.Sleep(1000);
                            }
                            ready = false;

                            message = $"{{\"timestamp\":{DateTimeOffset.UtcNow.ToUnixTimeSeconds()},\"tiempo_rango\":{Data.time_in_range_temp}}}";
                            resp.StatusCode = 200;
                            resp.StatusDescription = "OK";
                        }
                        else if (req.HttpMethod == "GET" && req.Url.AbsolutePath == "/temp")
                        {
                            message = $"{{\"timestamp\":{DateTimeOffset.UtcNow.ToUnixTimeSeconds()},\"temperatura\":{Data.temp_act}}}";
                            resp.StatusCode = 200;
                            resp.StatusDescription = "OK";
                        }
                        else
                        {
                            resp.StatusCode = 404;
                            resp.StatusDescription = "Not Found";
                        }
                        if (message != "")
                        {
                            // Write the response info
                            byte[] data = Encoding.UTF8.GetBytes(message);
                            resp.ContentType = "application/json";
                            resp.ContentEncoding = Encoding.UTF8;
                            resp.ContentLength64 = data.LongLength;

                            // Write out to the response stream (asynchronously), then close it
                            await resp.OutputStream.WriteAsync(data, 0, data.Length);
                        }
                        message = "";
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex.Message);
                        resp.StatusCode = 500;
                        resp.StatusDescription = "Internal Server Error";
                    }

                    resp.Close();
                }

            });
        }

        public static string showData(string[] data)
        {
            if (data == null) return string.Empty;

            StringBuilder datos = new StringBuilder();
            foreach (var item in data)
            {
                datos.Append(item + ";");
            }
            return datos.ToString();
        }

        public static bool tempCheck(string[] data, bool tipo)
        {
            if (data == null) return true; // Early exit if data is null

            double limit = tipo ? 12 : 30; // Determine the temperature limit based on tipo
            foreach (string temp in data)
            {
                if (double.TryParse(temp, out double tempValue) && ((tipo && tempValue < limit) || (!tipo && tempValue > limit)))
                {
                    return false;
                }
            }
            return true;
        }
    }
}
