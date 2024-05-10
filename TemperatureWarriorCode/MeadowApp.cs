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
        public static int total_time_in_range = 0;
        public static int total_time_out_of_range = 0;

        // Relays
        public static Relay relayBombilla;
        public static Relay relayPlaca;

        public int count = 0;
        public bool start = true;
        public int contiguous_outliers=0;
        public override async Task Run()
        {
            if (count == 0)
            {
                Console.WriteLine("Initialization...");

                // TODO uncomment when needed 
                // Temperature Sensor Configuration
                sensor = new AnalogTemperature(analogPin: Device.Pins.A01, sensorType: AnalogTemperature.KnownSensorType.TMP36);
                sensor.TemperatureUpdated += AnalogTemperatureUpdated; // Subscribing to event (temp change)

                // TODO Modify this value according to the needs of the project
                sensor.StartUpdating(TimeSpan.FromSeconds(2)); // Start updating the temperature every 2 seconds. In our case, we need to decide the time to update the temperature. We could use a lower value to get more accurate results and obtain an average of the temperature deleting outliers.

                // TODO Local Network configuration (uncomment when needed)
                var wifi = Device.NetworkAdapters.Primary<IWiFiNetworkAdapter>();
                wifi.NetworkConnected += WiFiAdapter_ConnectionCompleted;

                //WiFi Channel
                WifiNetwork wifiNetwork = ScanForAccessPoints(Secrets.WIFI_NAME);

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
        public static void StartRound()
        {
            // Initialize the round controller
            var roundController = new RoundController();

            // Initialize relays
            Relay relayBombilla = InstantiateRelay(Device.Pins.D02, initialValue: false);
            Relay relayPlaca = InstantiateRelay(Device.Pins.D03, initialValue: false);

            // Configure temperature ranges for the round
            TemperatureRange[] temperatureRanges = new TemperatureRange[Data.temp_min.Length];
            int totalTime = 0;

            for (int i = 0; i < Data.temp_min.Length; i++)
            {
                double tempMin = double.Parse(Data.temp_min[i]);
                double tempMax = double.Parse(Data.temp_max[i]);
                int roundTime = int.Parse(Data.round_time[i]) * 1000; // Convert seconds to milliseconds

                Console.WriteLine($"Configuring range {i}: Temp_min={tempMin}ºC, Temp_max={tempMax}ºC, Round_time={roundTime}s");

                temperatureRanges[i] = new TemperatureRange(tempMin, tempMax, roundTime);
                totalTime += roundTime;
            }

            // Configure the round controller
            if (roundController.Configure(temperatureRanges, totalTime, Data.refresh, relayBombilla, relayPlaca, out string errorMessage))
            {
                Console.WriteLine("Round controller successfully configured.");

                // Start the round operation (PID controller for each temperature range)
                roundController.StartOperation();

                // Start the round timer
                Task.Run(() => Timer());
            }
            else
            {
                Console.WriteLine($"Error configuring round controller: {errorMessage}");
            }
        }

        private static async Task Timer()
        {
            Data.is_working = true;

            for (int i = 0; i < Data.round_time.Length; i++)
            {
                Data.time_left = int.Parse(Data.round_time[i]);

                while (Data.time_left > 0)
                {
                    Data.time_left--;
                    await Task.Delay(1000); // Delay for 1 second
                }
            }

            Data.is_working = false;
        }


        /*
        ESTE CODIGO NOS LO DAN, LO DEJO DE MOMENTO POR SI NOS ES UTIL EN EL FUTURO
        Console.WriteLine("STARTING");

        //THE TW START WORKING
        while (Data.is_working) {

            //This is the time refresh we did not do before
            Thread.Sleep(Data.refresh - sleep_time);

            //Temperature registration
            Console.WriteLine($"RegTempTimer={regTempTimer.Elapsed.ToString()}, enviando Temp={Data.temp_act}");
            RoundController.RegisterTemperature(double.Parse(Data.temp_act));
            regTempTimer.Restart();

        }
        Console.WriteLine("Round Finish");
        t.Abort();

        total_time_in_range += RoundController.TimeInRangeInMilliseconds;
        total_time_out_of_range += RoundController.TimeOutOfRangeInMilliseconds;
        Data.time_in_range_temp = (RoundController.TimeInRangeInMilliseconds / 1000);

        Console.WriteLine("Tiempo dentro del rango " + (((double)RoundController.TimeInRangeInMilliseconds / 1000)) + " s de " + total_time + " s");
        Console.WriteLine("Tiempo fuera del rango " + ((double)total_time_out_of_range / 1000) + " s de " + total_time + " s");
        */

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

            // Check if the new temperature is an outlier
            if (temp_new < 0.55 * prev_temp || temp_new > 1.45 * prev_temp)
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
            ObservableCollection<WifiNetwork> networks = new ObservableCollection<WifiNetwork>(Device.NetworkAdapters.Primary<IWiFiNetworkAdapter>().Scan()?.Result?.ToList()); //REVISAR SI ESTO ESTA BIEN
            wifiNetwork = networks?.FirstOrDefault(x => string.Compare(x.Ssid, SSID, true) == 0);
            return wifiNetwork;
        }
    }
}
