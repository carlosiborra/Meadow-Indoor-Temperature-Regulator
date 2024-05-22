using Meadow;
using Meadow.Foundation;
using Meadow.Foundation.Sensors.Temperature;
using Meadow.Devices;
using Meadow.Hardware;
using Meadow.Gateway.WiFi;
using Meadow.Units;
using System;
using System.Net.Http;
using System.Net.NetworkInformation;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Diagnostics;
using TemperatureWarriorCode.Web;
using NETDuinoWar;
using Meadow.Foundation.Relays;
using System.Text.RegularExpressions;
using System.Net;

namespace TemperatureWarriorCode
{
    public class MeadowApp : App<F7FeatherV2>

    {

        //Temperature Sensor
        AnalogTemperature sensor;

        //Time Controller Values
        public static int total_time = 0;

        public WebServer webServer;

        public static TimeController timeController;

        // Relays
        public static Relay relayBombilla;
        public static Relay relayPlaca;

        public int count = 0;
        public bool start = true;
        public int contiguous_outliers = 0;
        public override async Task Run()
        {
            if (count == 0)
            {
                Console.WriteLine("Initialization...");

                // Temperature Sensor Configuration
                sensor = new AnalogTemperature(analogPin: Device.Pins.A01, sensorType: AnalogTemperature.KnownSensorType.TMP36);
                sensor.TemperatureUpdated += AnalogTemperatureUpdated; // Subscribing to event (temp change)

                // TODO Modify this value according to the needs of the project
                sensor.StartUpdating(TimeSpan.FromSeconds(2)); // Start updating the temperature every 2 seconds. In our case, we need to decide the time to update the temperature. We could use a lower value to get more accurate results and obtain an average of the temperature deleting outliers.

                // TODO Local Network configuration (uncomment when needed)
                var wifi = Device.NetworkAdapters.Primary<IWiFiNetworkAdapter>();
                wifi.NetworkConnected += WiFiAdapter_ConnectionCompleted;

                WifiNetwork wifiNetwork = ScanForAccessPoints(Secrets.WIFI_NAME);
                while (wifiNetwork == null)
                {
                    //WiFi Channel
                    wifiNetwork = ScanForAccessPoints(Secrets.WIFI_NAME);
                    Console.WriteLine("No encuentro redes");
                }
                Console.WriteLine($"Wifi Networks: {wifiNetwork}");
                wifi.NetworkConnected += WiFiAdapter_WiFiConnected;
                await wifi.Connect(Secrets.WIFI_NAME, Secrets.WIFI_PASSWORD);

                // Use wifi.IpAddress.ToString() instead of a placeholder IP
                string ipAddressString = wifi.IpAddress.ToString();
                //string ipAddressString = "127.0.0.1";

                // Display the IP address
                Console.WriteLine($"IP Address: {ipAddressString}:{Data.Port}");
                Data.IP = ipAddressString;

                if (!string.IsNullOrWhiteSpace(ipAddressString))
                {
                    Data.IP = ipAddressString;

                    // Convert the string IP address to a System.Net.IPAddress object
                    IPAddress ipAddress = IPAddress.Parse(ipAddressString);

                    // Pass the IPAddress object to the WebServer constructor
                    WebServer webServer = new WebServer(ipAddress, Data.Port);
                    if (webServer != null)
                    {
                        webServer.Start();
                    }
                }

                Console.WriteLine("Meadow Initialized!");

                count = count + 1;
            }
        }


        //TW Combat Round
        public static async Task StartRoundAsync()
        {
            Stopwatch timer = Stopwatch.StartNew();
            timer.Start();

            // Initialize the round controller
            var roundController = new RoundController();
            TimeController timeController = new TimeController();

            // Initialize relays
            Relay relayBombilla = InstantiateRelay(Device.Pins.D05, initialValue: false);
            Relay relayPlaca = InstantiateRelay(Device.Pins.D06, initialValue: false);

            // Configure temperature ranges for the round
            TemperatureRange[] temperatureRanges = new TemperatureRange[Data.temp_min.Length];
            int total_time = 0;

            Data.is_working = true;
            for (int i = 0; i < Data.temp_min.Length; i++)
            {
                double tempMin = double.Parse(Data.temp_min[i]);
                double tempMax = double.Parse(Data.temp_max[i]);
                int roundTime = int.Parse(Data.round_time[i]) * 1000; // Convert seconds to milliseconds

                Console.WriteLine($"Configuring range {i}: Temp_min={tempMin}ºC, Temp_max={tempMax}ºC, Round_time={roundTime / 1000}s");

                // Check that the temperatures obtained are within the range of maximum and minimum temperatures
                tempMax = roundController.CheckTemperature(tempMax);
                tempMin = roundController.CheckTemperature(tempMin);

                // Ensure the duration is greater than 0
                if (roundTime <= 0)
                {
                    throw new ArgumentException("Duration should be greater than zero.");
                }
                temperatureRanges[i] = new TemperatureRange(tempMin, tempMax, roundTime);

                total_time += roundTime;
            }

            bool success;
            string error_message = null;
            // Initialization of timecontroller with the ranges
            timeController.DEBUG_MODE = true;
            success = timeController.Configure(temperatureRanges, total_time, Data.refresh, out error_message);
            Console.WriteLine(success);

            // Configure the round controller
            if (roundController.Configure(temperatureRanges, total_time, Data.refresh, relayBombilla, relayPlaca, out string errorMessage) && success)
            {
                Console.WriteLine("Round controller successfully configured.");

                // Initialization of timer
                _ = Task.Run(() => TimerAsync());  // Run the timer asynchronously

                // Start the round operation (PID controller for each temperature range)
                await Task.Run(() => timeController.StartOperation());
                await Task.Run(() => roundController.StartOperation(timeController, total_time));
            }
            else
            {
                Console.WriteLine($"Error configuring round controller: {errorMessage}");
            }
        }

        //Round Timer
        private static async Task TimerAsync()
        {
            Data.is_working = true;
            Console.WriteLine($"Hilo iniciado");
            for (int i = 0; i < Data.round_time.Length; i++)
            {
                Data.time_left = int.Parse(Data.round_time[i]);
                Console.WriteLine($"{Data.time_left} seconds left");
                while (Data.time_left > 0)
                {
                    Data.time_left--;
                    Console.WriteLine($"{Data.time_left} seconds left");
                    await Task.Delay(1000);
                }
                Data.next_range = true;
            }
            Data.is_working = false;
            Console.WriteLine("Reloj finalizado");
        }

        #region Relay
        private static Relay InstantiateRelay(IPin thePin, bool initialValue)
        {
            Relay theRelay = new Relay(Device.CreateDigitalOutputPort(thePin));
            theRelay.IsOn = initialValue;
            return theRelay;
        }
        #endregion

        //Temperature and Display Updated
        void AnalogTemperatureUpdated(object sender, IChangeResult<Meadow.Units.Temperature> e)
        {
            // Round the new temperature to 2 decimal places
            var temp_new = Math.Round((double)e.New.Celsius, 2);

            // Only check for outliers if start is false (not the first reading)
            if (!start)
            {
                // Parse previous temperature
                var prev_temp = Convert.ToDouble(Data.temp_act);

                // Shutwdown when extreme temperatures or sensor disconnection are detected
                if (temp_new > RoundController.max_allowed_temp || temp_new < RoundController.min_allowed_temp)
                {
                    Console.WriteLine("Shutdown requested");
                    webServer.Stop();
                }

                // Check if the new temperature is an outlier
                if (temp_new < prev_temp - 10.00 || temp_new > prev_temp + 10.00)
                {
                    // Increment the count of contiguous outliers
                    if (++contiguous_outliers < 3)
                    {
                        Console.WriteLine($"Current temperature (outlier): {Data.temp_act}");
                        return;
                    }
                }
                else
                {
                    // Reset the count of contiguous outliers if the temperature is within range
                    contiguous_outliers = 0;
                }
            }

            // Update and print the new temperature
            Data.temp_act = temp_new.ToString();
            start = false;
            Console.WriteLine($"Current temperature: {Data.temp_act}");
        }


        void WiFiAdapter_WiFiConnected(object sender, EventArgs e)
        {
            if (sender != null)
            {
                Console.WriteLine($"Connecting to WiFi Network {Secrets.WIFI_NAME}");
            }
        }

        void WiFiAdapter_ConnectionCompleted(object sender, EventArgs e)
        {
            Console.WriteLine("Connection request completed.");
        }

        protected WifiNetwork ScanForAccessPoints(string SSID)
        {
            WifiNetwork wifiNetwork = null;
            ObservableCollection<WifiNetwork> networks = new ObservableCollection<WifiNetwork>(Device.NetworkAdapters.Primary<IWiFiNetworkAdapter>().Scan()?.Result?.ToList());
            wifiNetwork = networks?.FirstOrDefault(x => string.Compare(x.Ssid, SSID, true) == 0);
            return wifiNetwork;
        }
    }
}
