import {
    LineChart,
    Line,
    XAxis,
    YAxis,
    CartesianGrid,
    Tooltip,
    Legend,
} from "recharts";
import { useEffect, useState } from "react";
import { useStore } from "@nanostores/react";
import { dataStore } from "@/stores/dataStore";
import { displayRefreshRateStore } from "@/stores/displayRefreshRateStore";
import { fetchStatusStore } from "@/stores/fetchStatusStore"; // Import the new store
import type { Data } from "@/types/Data";

const PUBLIC_BASE_URL = import.meta.env.PUBLIC_BASE_URL ?? 'http://localhost:3000';
const N_DATA = 500;

export default function Grafica() {
    const displayRefreshRate = useStore(displayRefreshRateStore);
    const data = useStore(dataStore);
    const fetchStatus = useStore(fetchStatusStore); // Get fetch status

    useEffect(() => {
        console.log(`Base url: ${PUBLIC_BASE_URL}`);

        const fetchData = async () => {
            const start = Date.now();
            try {
                const resp = await fetch(`${PUBLIC_BASE_URL}/temp`, {
                    headers: {
                        'Content-Type': 'application/json',
                    },
                });

                if (!resp.ok) {
                    throw new Error(`HTTP error! status: ${resp.status}`);
                }

                const body = await resp.json();
                console.log(body);

                let new_data: Data[] = [];
                for (let i = 0; i < body.temp_max.length; i++) {
                    new_data.push({
                        temp_min: body.temp_min[i],
                        temp_max: body.temp_max[i],
                        temperature: body.temperatures[i],
                        timestamp: body.timestamp[i],
                    } satisfies Data);
                }
                dataStore.set(new_data.slice(-N_DATA));
            } catch (error) {
                console.warn(`No se pudo recibir los datos del servidor; Elapsed: ${Date.now() - start}ms`);
            }
        };

        let intervalId: NodeJS.Timeout;

        if (fetchStatus === 'start') {
            intervalId = setInterval(fetchData, displayRefreshRate);
        } else if (fetchStatus === 'stop') {
            clearInterval(intervalId);
        }

        return () => clearInterval(intervalId);
    }, [displayRefreshRate, fetchStatus]); // Add fetchStatus to dependencies

    return (
        <>
            <LineChart width={1000} height={500} data={data} className="custom-chart bg-white p-4 m-4 rounded-md">
                <CartesianGrid strokeDasharray="4" />
                <XAxis dataKey="timestamp" />
                <YAxis />
                <Tooltip />
                <Legend />
                <Line
                    type="monotone"
                    dataKey="temp_min"
                    stroke="#0392B2"
                    strokeDasharray="5 5"
                    dot={false}
                    strokeWidth={2}
                />
                <Line
                    type="monotone"
                    dataKey="temp_max"
                    stroke="#FF4C26"
                    strokeDasharray="5 5"
                    dot={false}
                    strokeWidth={2}
                />
                <Line
                    type="monotone"
                    dataKey="temperature"
                    stroke="#04252D"
                    dot={{ r: 3, fill: "#04252D" }}
                    strokeWidth={3}
                />
            </LineChart>
        </>
    );
}
