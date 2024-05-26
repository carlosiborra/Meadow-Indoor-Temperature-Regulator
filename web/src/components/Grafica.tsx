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
import { timeInRangeStore } from "@/stores/timeInRangeStore";
import { timeElapsedStore } from "@/stores/timeElapsedStore";
import { internalRefreshRateStore } from "@/stores/internalRefreshRateStore";
import { roundDurationStore } from "@/stores/roundDurationStore";
import type { Data } from "@/types/Data";

const PUBLIC_BASE_URL = import.meta.env.PUBLIC_BASE_URL ?? 'http://localhost:3000';
const N_DATA = 500;

export default function Grafica() {
    const displayRefreshRate = useStore(displayRefreshRateStore);
    const fetchStatus = useStore(fetchStatusStore); // Get fetch status
    const internalRefreshRate = useStore(internalRefreshRateStore)
    const [data, setData] = useState<Data[]>([]);
    let intervalId: NodeJS.Timeout;

    useEffect(() => {
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

                let new_data: Data[] = dataStore.get();
                for (let i = 0; i < body.temp_max.length; i++) {
                    new_data.push({
                        temp_min: body.temp_min[i],
                        temp_max: body.temp_max[i],
                        temperature: body.temperatures[i],
                        timestamp: body.timestamp[i],
                    } satisfies Data);
                }
                dataStore.set(new_data);
                setData(new_data.slice(-N_DATA));

                timeInRangeStore.set(new_data.filter((d) => d.temperature >= d.temp_min && d.temperature <= d.temp_max).length * internalRefreshRate);
                timeElapsedStore.set(new_data.length * internalRefreshRate)

                if (timeElapsedStore.get() >= roundDurationStore.get() * 1000) {
                    fetchStatusStore.set('stop')
                }
            } catch (error) {
                console.warn(`No se pudo recibir los datos del servidor; Elapsed: ${Date.now() - start}ms`);
            }
        };


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
                <XAxis />
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
