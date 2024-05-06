import React from "react";
import {
    LineChart,
    Line,
    XAxis,
    YAxis,
    CartesianGrid,
    Tooltip,
    Legend,
} from "recharts";


const data = [
    {
        name: "t1",
        tmin: 12,
        tmax: 30,
        current_temp: 15,
        round_duration: 0,
        refresh_rate: 0,
        internal_rate: 0,
        timestamp: 0
    },
    {
        name: "t2",
        tmin: 10,
        tmax: 28,
        current_temp: 18,
        round_duration: 0,
        refresh_rate: 0,
        internal_rate: 0,
        timestamp: 0
    },
    {
        name: "t3",
        tmin: 14,
        tmax: 32,
        current_temp: 22,
        round_duration: 0,
        refresh_rate: 0,
        internal_rate: 0,
        timestamp: 0
    },
    {
        name: "t4",
        tmin: 13,
        tmax: 31,
        current_temp: 19,
        round_duration: 0,
        refresh_rate: 0,
        internal_rate: 0,
        timestamp: 0
    },
    {
        name: "t5",
        tmin: 11,
        tmax: 29,
        current_temp: 16,
        round_duration: 0,
        refresh_rate: 0,
        internal_rate: 0,
        timestamp: 0
    },
    {
        name: "t6",
        tmin: 15,
        tmax: 33,
        current_temp: 24,
        round_duration: 0,
        refresh_rate: 0,
        internal_rate: 0,
        timestamp: 0
    },
    {
        name: "t7",
        tmin: 12,
        tmax: 30,
        current_temp: 17,
        round_duration: 0,
        refresh_rate: 0,
        internal_rate: 0,
        timestamp: 0
    },
    {
        name: "t8",
        tmin: 9,
        tmax: 27,
        current_temp: 20,
        round_duration: 0,
        refresh_rate: 0,
        internal_rate: 0,
        timestamp: 0
    },
    {
        name: "t9",
        tmin: 16,
        tmax: 34,
        current_temp: 25,
        round_duration: 0,
        refresh_rate: 0,
        internal_rate: 0,
        timestamp: 0
    },
    {
        name: "t10",
        tmin: 18,
        tmax: 36,
        current_temp: 28,
        round_duration: 0,
        refresh_rate: 0,
        internal_rate: 0,
        timestamp: 0
    }
];

const styles = `
  g {
    color: #FFF !important;
  }

`;

export default function Grafica() {
    return (
        <>
            <LineChart width={1000} height={500} data={data} className="custom-chart bg-white p-4 m-4 rounded-md">
                <CartesianGrid strokeDasharray="4" />
                <XAxis dataKey="name" padding={{ left: 30, right: 30 }}/>
                <YAxis/>
                <Tooltip/>
                <Legend/>
                <Line
                    type="monotone"
                    dataKey="tmin"
                    stroke="#0392B2"
                    strokeDasharray="5 5"
                    dot={false}
                    strokeWidth={2}
                />
                <Line
                    type="monotone"
                    dataKey="tmax"
                    stroke="#FF4C26"
                    strokeDasharray="5 5"
                    dot={false}
                    strokeWidth={2}
                />
                <Line
                    type="monotone"
                    dataKey="current_temp"
                    stroke="#04252D"
                    dot={{ r: 3, fill: "#04252D"}}
                    strokeWidth={3}
                />
            </LineChart>
        </>
    );
}