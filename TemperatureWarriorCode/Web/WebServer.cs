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
using System.IO;
using System.Text.Json;
using NETDuinoWar;

namespace TemperatureWarriorCode.Web
{
    public class WebServer
    {
        private IPAddress _ip = null;
        private int _port = -1;
        public bool _runServer = true;
        private static HttpListener _listener;
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

        public string Url => $"http://{_ip}:{_port}/";

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
                _listener.Prefixes.Add($"http://*:{_port}/"); // Accept requests from any IP
                _listener.Start();
                Console.WriteLine($"The url of the webserver is {Url}");
            }

            try
            {
                while (_runServer)
                {
                    var ctx = await _listener.GetContextAsync();
                    await HandleIncomingConnections(ctx);
                }
            }
            finally
            {
                _listener.Close();
            }
        }

        public void Stop()
        {
            _runServer = false;
            MeadowApp.relayBombilla.IsOn = true;
            MeadowApp.relayPlaca.IsOn = true;
            // Exit the application
            Environment.Exit(0);
        }

        public async Task HandleIncomingConnections(HttpListenerContext ctx)
        {
            var req = ctx.Request;
            var resp = ctx.Response;

            Console.WriteLine($"Request #{Interlocked.Increment(ref _requestCount)}: {req.Url.AbsolutePath} {req.HttpMethod} {req.Url} - {req.UserHostAddress}");

            // Handle CORS preflight request
            if (req.HttpMethod == "OPTIONS")
            {
                resp.AddHeader("Access-Control-Allow-Origin", "*");
                resp.AddHeader("Access-Control-Allow-Methods", "GET, POST, OPTIONS");
                resp.AddHeader("Access-Control-Allow-Headers", "Content-Type");
                resp.AddHeader("Access-Control-Max-Age", "86400"); // Cache preflight response for 24 hours
                resp.StatusCode = 204; // No Content
                resp.Close();
                return;
            }

            try
            {
                switch (req.Url.AbsolutePath)
                {
                    case "/shutdown" when req.HttpMethod == "POST":
                        Console.WriteLine("Shutdown requested");
                        message = $"{{\"timestamp\":{DateTimeOffset.UtcNow.ToUnixTimeSeconds()},\"message\":\"Server shutting down.\"}}";
                        resp.StatusCode = 200;
                        resp.StatusDescription = "OK";
                        // Once response is sent, stop the server
                        Stop();
                        break;

                    case "/setparams" when req.HttpMethod == "POST":
                        Console.WriteLine("\nSetting parameters - MODO PREPARADO\n");

                        using (var reader = new StreamReader(req.InputStream, req.ContentEncoding))
                        {
                            var jsonBody = await reader.ReadToEndAsync();
                            using (JsonDocument document = JsonDocument.Parse(jsonBody))
                            {
                                var body = document.RootElement;
                                string pass_temp = body.GetProperty("pass").GetString();
                                string temp_max_str = body.GetProperty("temp_max").GetString();
                                string temp_min_str = body.GetProperty("temp_min").GetString();

                                Data.temp_max = temp_max_str.Split(";");
                                Data.temp_min = temp_min_str.Split(";");
                                Data.refresh = body.GetProperty("refresh").GetInt16();
                                Data.round_time = body.GetProperty("round_time").GetString().Split(";");

                                Console.WriteLine($"Temp max: {showData(Data.temp_max)}\nTemp min: {showData(Data.temp_min)}\nRefresh: {Data.refresh}\nRound time: {showData(Data.round_time)}");

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
                        }
                        break;

                    case "/start" when req.HttpMethod == "GET":
                        Console.WriteLine("\nStarting round - MODO COMBATE\n");

                        // Run the round operation asynchronously
                        _ = Task.Run(async () =>
                        {
                            await MeadowApp.StartRoundAsync();
                            Data.is_working = false;
                        });

                        resp.StatusCode = 200;
                        resp.StatusDescription = "OK";
                        message = $"{{\"timestamp\":{DateTimeOffset.UtcNow.ToUnixTimeSeconds()},\"message\":\"Round started.\"}}";

                        break;

                    case "/temp" when req.HttpMethod == "GET":
                        // Serialize the temp_info object to a JSON string
                        string json_structure = JsonSerializer.Serialize(Data.temp_structure, new JsonSerializerOptions
                        {
                            WriteIndented = true
                        });

                        // Prepare the response message
                        message = $"{json_structure}";

                        resp.StatusCode = 200;
                        resp.StatusDescription = "OK";

                        // Clear the data after sending the response using a lock
                        lock (Data.temp_structure)
                        {
                            Data.temp_structure.temp_max.Clear();
                            Data.temp_structure.temp_min.Clear();
                            Data.temp_structure.temperatures.Clear();
                            Data.temp_structure.timestamp.Clear();
                        }

                        break;
                    
                    case "/round_info" when req.HttpMethod == "GET":
                        Console.WriteLine("Retrieve round info");

                        resp.StatusCode = 200;
                        resp.StatusDescription = "OK";
                        message = $"{{\"timestamp\":{DateTimeOffset.UtcNow.ToUnixTimeSeconds()},\"time_in_range\":\"\"}}";
                        break;

                    default:
                        resp.StatusCode = 404;
                        resp.StatusDescription = "Not Found";
                        break;
                }

                resp.AddHeader("Access-Control-Allow-Origin", "*");
                resp.AddHeader("Access-Control-Allow-Methods", "GET, POST, OPTIONS");
                resp.AddHeader("Access-Control-Allow-Headers", "Content-Type");

                if (!string.IsNullOrEmpty(message))
                {
                    byte[] data = Encoding.UTF8.GetBytes(message);
                    resp.ContentType = "application/json";
                    resp.ContentEncoding = Encoding.UTF8;
                    resp.ContentLength64 = data.Length;
                    await resp.OutputStream.WriteAsync(data, 0, data.Length);
                }
            }
            catch (JsonException jsonEx)
            {
                Console.WriteLine($"JSON Error: {jsonEx.Message}");
                resp.StatusCode = 400;
                resp.StatusDescription = "Bad Request";
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                resp.StatusCode = 500;
                resp.StatusDescription = "Internal Server Error";
            }
            finally
            {
                message = "";
                resp.Close();
            }
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
            if (data == null) return true;

            double limit = tipo ? 12 : 30;
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
