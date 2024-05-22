using System;
using System.Collections.Generic;
using Meadow.Foundation;

namespace TemperatureWarriorCode
{
    class Data
    {

        //WEB VARIABLES
        public static string IP = "127.0.0.1";
        public static int Port = 3000;

        //ROUND VARIABLES
        public static string[] temp_max = { "34", "32" }; // In ºC
        public static string[] temp_min = { "32", "30" }; // In ºC
        public static string[] round_time = { "20", "100" }; // in s
        public static int refresh = 100; // In ms
        public static double output;

        //START ROUND VARIABLES
        public static bool is_working = false;
        public static string temp_act = "0"; // In ºC
        public static int time_left; // in s
        public static int time_in_range_temp = 0; //In ms.

        public static bool next_range = false;

        public static double targetTemperature = 0.0;
        public static double temperaturaMaxima = 0.0;
        public static double temperaturaMinima = 0.0;

        public static temp_info temp_structure = new temp_info();
        

    }
}
public class temp_info {
    public List<double> temp_max { get; set; }
    public List<double> temp_min { get; set; }
    public List<double> temperatures { get; set; }
    public List<long> timestamp { get; set; }

    public temp_info() {
        temp_max = new List<double>();
        temp_min = new List<double>();
        temperatures = new List<double>();
        timestamp = new List<long>();
    }
}


