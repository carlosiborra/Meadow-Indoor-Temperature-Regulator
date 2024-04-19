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
                            string url = req.RawUrl;
                            if (!string.IsNullOrWhiteSpace(url))
                            {

                                //Get text to the right from the interrogation mark
                                string[] urlParts = url.Split('?');
                                if (urlParts?.Length >= 1)
                                {

                                    //The parametes are in the array first position
                                    string[] parameters = urlParts[1].Split('&');
                                    if (parameters?.Length >= 2)
                                    {

                                        // Param 5 => to pass
                                        string[] pass_parts = parameters[5].Split('=');
                                        string pass_temp = pass_parts[1];

                                        if (string.Equals(pass, pass_temp))
                                        {

                                            // Param 0 => Temp max
                                            string[] temp_max_parts = parameters[0].Split('=');
                                            //string[] temp_max_final = temp_max_parts[1].Split(";");
                                            //Data.temp_max = new string[] { temp_max_parts[1].Split(";") };
                                            Data.temp_max = temp_max_parts[1].Split(";");


                                            // Param 1 => Temp min
                                            string[] temp_min_parts = parameters[1].Split('=');
                                            //Data.temp_min = new string[] { temp_min_parts[1] };
                                            //Data.temp_min = new string[] { "12", "12" };
                                            Data.temp_min = temp_min_parts[1].Split(";");

                                            // Param 2 => to display_refresh
                                            string[] display_refresh_parts = parameters[2].Split('=');
                                            Data.display_refresh = Int16.Parse(display_refresh_parts[1]);
                                            //Data.display_refresh = 1000;

                                            // Param 3 => to refresh
                                            string[] refresh_parts = parameters[3].Split('=');
                                            Data.refresh = Int16.Parse(refresh_parts[1]);
                                            //Data.refresh = 1000;



                                            // Param 4 => to round_time
                                            string[] round_time_parts = parameters[4].Split('=');
                                            //Data.round_time = new string[] { round_time_parts[1] };
                                            //Data.round_time = new string[] { "5", "15" };
                                            Data.round_time = round_time_parts[1].Split(";");

                                            ready = tempCheck(Data.temp_max, false) && tempCheck(Data.temp_min, true);
                                            if (ready)
                                            {
                                                message = $"{{\"timestamp\":{DateTimeOffset.UtcNow.ToUnixTimeSeconds()},\"mensaje\":\"Los parametros se han cambiado satisfactoriamente. Todo preparado.\"}}";
                                                resp.StatusCode = 400;
                                                resp.StatusDescription = "Bad Request";
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
                                else
                                {
                                    resp.StatusCode = 400;
                                    resp.StatusDescription = "Bad Request";
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

        public static string writeHTML(string message, int pageViews, string disableSubmit)
        {

            // Variables
            var disabled = ready ? "disabled" : "";
            var saveButton = ready ? "" : "<button type=\"button\" onclick='save()'>Guardar</button>";
            var tempLink = "<a href='#' class='btn btn-primary tm-btn-search' onclick='temp()'>Consultar Temperatura</a>";
            var startButton = ready && !Data.is_working ? "<button type=\"button\" onclick='start()'>Comenzar Ronda</button>" : "";
            var graphCanvas = ""; // Assume logic for setting this value

            // CSS variables
            string backgroundColor = "#f2f2f2";
            string textColor = "#1e1e1e1";

            // Note: Using string interpolation with a verbatim string literal
            // needs double curly braces as single ones are used for string interpolation

            // Graph canvas and 
            //if (Data.csv_counter != 0) {
            //    graphCanvas = "<canvas id='myChart' width='0' height='0'></canvas>";
            //    message = "El tiempo que se ha mantenido en el rango de temperatura es de " + Data.time_in_range_temp.ToString() + " s.";
            //}

            // CSS styles
            string cssStyles = $@"
                /* CSS styles here */

                html, body {{
                    font-family: 'Open Sans', sans-serif;
                    margin: 0;
                    padding: 0;
                    background-color: {backgroundColor};
                    color: {textColor};
                }}

                p {{
                    color: #ffffff;
                }}

                .tm-main-content {{
                    margin-top: 20px;
                    padding: 20px;
                }}

                .tm-section {{
                    margin: 20px;
                    background-color: #fff;
                    padding: 20px;
                }}

                body {{
                    font-family: 'Open Sans', sans-serif;
                    background-color: #24303c;
                    margin: 0;
                    padding: 0;
                }}

                .container {{
                    width: 80%;
                    margin: auto;
                    overflow: hidden;
                }}

                .navbar {{
                    background-color: #fff;
                    color: #ffffff;
                    display: flex;
                    align-items: center;
                    padding: 10px 20px;
                    border-radius: 20px;
                }}

                .navbar a.navbar-brand {{
                    color: #ffffff;
                    text-decoration: none;
                }}

                .navbar-brand img {{
                    max-width: 100%;
                    height: auto;
                }}

                .tm-section {{
                    background-color: #24303c;
                    margin: 20px 0;
                    padding: 20px;
                    border-radius: 8px;
                    box-shadow: 0 0 10px rgba(0, 0, 0, 0.1);
                }}

                .form-group {{
                    margin-bottom: 15px;
                }}

                .form-group a,
                .form-group p {{
                    margin: 0;
                    font-size: 18px;
                    color: #ffffff;
                }}

                input[type='text'],
                input[type='number'],
                input[type='password'],
                input[type='text']:focus,
                input[type='number']:focus,
                input[type='password']:focus {{
                    width: 100%;
                    padding: 10px;
                    margin-top: 5px;
                    margin-bottom: 5px;
                    border-radius: 4px;
                    border: 2px solid #449d44;
                    box-sizing: border-box;
                    background: #182d43;
                    color: #ffffff;
                    outline: none;
                }}

                input[type='text']:disabled,
                input[type='number']:disabled {{
                    background-color: #182d43;
                    border: 2px solid #fa5454;
                }}

                input[type='submit'],
                button {{
                    background-color: #1f53c5;
                    color: white;
                    border: none;
                    padding: 10px 20px;
                    border-radius: 4px;
                    cursor: pointer;
                    font-size: 16px;
                }}

                input[type='submit']:hover,
                button:hover {{
                    background-color: #1a4195;
                }}

                input[type='button']:hover {{
                    background-color: #286090;
                }}

                .tm-form-element {{
                    display: flex;
                    flex-direction: column;
                }}

                .tm-search-form-row {{
                    margin-bottom: 20px;
                }}

                .tm-form-element-50 {{
                    flex-basis: 50%;
                }}

                .tm-form-element-100 {{
                    flex-basis: 100%;
                }}

                @media (max-width: 768px) {{
                    .form-group {{
                        width: 100%;
                    }}

                    .tm-form-element-50,
                    .tm-form-element-100 {{
                        flex-basis: 100%;
                    }}
                }}

                /* Styles for the graph placeholder */
                .graph-placeholder {{
                    width: 100%;
                    background: #ddd;
                    color: #333;
                    text-align: center;
                    padding: 50px 0;
                    margin-top: 20px;
                    border-radius: 8px;
                }}
            ";

            // JS scripts
            string jsScripts = @"
                /* JavaScript scripts here */

                function save() {
                    console.log('Calling Save in JS!!');
                    var tempMax = document.forms['params']['tempMax'].value;
                    var tempMin = document.forms['params']['tempMin'].value;
                    var displayRefresh = document.forms['params']['displayRefresh'].value;
                    var refresh = document.forms['params']['refresh'].value;
                    var time = document.forms['params']['time'].value;
                    var pass = document.forms['params']['pass'].value;
                    location.href = 'setparams?tempMax=' + tempMax + '&tempMin=' + tempMin + '&displayRefresh=' + displayRefresh + '&refresh=' + refresh + '&time=' + time + '&pass=' + pass;
                }

                function temp() {
                    console.log('Calling temp in JS!!');
                    location.href = 'temp';
                }

                function start() {
                    console.log('Calling start in JS!!');
                    location.href = 'start';
                }
            ";

            // Write the HTML page
            string html = $@"
                <!DOCTYPE html>
                <html>
                <head>
                     <meta charset='utf-8'>
                    <meta http-equiv='X-UA-Compatible' content='IE=edge'>
                    <meta name='viewport' content='width=device-width, initial-scale=1'>
                    <title>Meadow Controller</title>
                    <link rel='stylesheet' href='https://fonts.googleapis.com/css?family=Open+Sans:300,400,600,700'>
                    <link rel='stylesheet' href='http://127.0.0.1:8887/css/bootstrap.min.css'>
                    <link rel='stylesheet' href='http://127.0.0.1:8887/css/tooplate-style.css'>
                    <script src='https://cdnjs.cloudflare.com/ajax/libs/Chart.js/3.8.0/chart.js'></script>
                    <style>
                        {cssStyles}
                    </style>
                </head>
                <body>
                    <script>
                        {jsScripts}
                    </script>
                    <div class='tm-main-content' id='top'>
                        <div class='tm-top-bar-bg'></div>
                        <div class='container'>
                            <div class='row'>
                                <nav class='navbar navbar-expand-lg narbar-light'>
                                    <a class='navbar-brand mr-auto' href='#'>
                                        <img id='logo' class='logo' src='https://gitlab.msu.edu/uploads/-/system/project/avatar/6141/gitlab-ci-cd-logo_2x.png' alt='Site logo' width='700'
                                            height='300'>
                                    </a>
                                </nav>
                            </div>
                        </div>
                    </div>
                    <div class='tm-section tm-bg-img' id='tm-section-1'>
                        <div class='tm-bg-white ie-container-width-fix-2'>
                            <div class='container ie-h-align-center-fix'>
                                <div class='row'>
                                    <div class='col-xs-12 ml-auto mr-auto ie-container-width-fix'>
                                        <form name='params' method='get' class='tm-search-form tm-section-pad-2'>
                                            <div class='form-row tm-search-form-row'>
                                                <div class='form-group tm-form-element tm-form-element-100'>
                                                    <p>Temperatura Max <b>(&deg;C)</b> <input name='tempMax' type='text'
                                                            class='form-control' value='  mostarDatos(Data.temp_max)  '
                                                            disabled></input></p>
                                                </div>
                                                <div class='form-group tm-form-element tm-form-element-50'>
                                                    <p>Temperatura Min <b>(&deg;C)</b> <input name='tempMin' type='text'
                                                            class='form-control' value='  mostarDatos(Data.temp_min)  '
                                                            disabled></input></p>
                                                </div>
                                                <div class='form-group tm-form-element tm-form-element-50'>
                                                    <p>Duraci&oacute;n Ronda <b>(s)</b> <input name='time' type='text'
                                                            class='form-control' value='  mostarDatos(Data.round_time)  '
                                                            disabled></input></p>
                                                </div>
                                            </div>
                                            <div class='form-row tm-search-form-row'>
                                                <div class='form-group tm-form-element tm-form-element-100'>
                                                    <p>Cadencia Refresco <b>(ms)</b> <input name='displayRefresh' type='number'
                                                            class='form-control' value='  Data.display_refresh  ' disabled></input></p>
                                                </div>
                                                <div class='form-group tm-form-element tm-form-element-50'>
                                                    <p>Cadencia Interna <b>(ms)</b> <input name='refresh' type='number'
                                                            class='form-control' value='  Data.refresh  ' disabled></input></p>
                                                </div>
                                                <div class='form-group tm-form-element tm-form-element-50'>
                                                    <p>Contrase&ntilde;a <input name='pass' type='password' class='form-control'>
                                                        </input></p>
                                                </div>

                                        </form>
                                        <div class='form-group tm-form-element tm-form-element-50'>
                                            {saveButton}{startButton}
                                        </div>
                                        <div class='form-group tm-form-element tm-form-element-50'>
                                            {tempLink}
                                        </div>
                                    </div>
                                    <p style='text-align:center;font-weight:bold;'>{message}</p>
                                </div>
                            </div>
                        </div>
                    </div>

                    <div class='container ie-h-align-center-fix'>
                        {graphCanvas}
                    </div>
                </body>
                </html>
            ";

            return html;
        }
    }
}
