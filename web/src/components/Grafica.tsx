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
import type { Data } from "@/types/Data";

const BASE_URL = import.meta.env.PUBLIC_BASE_URL ?? 'http://localhost:3000'

export default function Grafica() {
    const displayRefreshRate = useStore(displayRefreshRateStore);
    const data = useStore(dataStore)

    const [processedData, setProcessedData] = useState<{
        temp_max: number,
        temp_min: number,
        temperature: number,
        name: number
    }[]>([])

    useEffect(() => {
        console.log(`Base url: ${BASE_URL}`)
        setInterval(async () => {
            const controller = new AbortController();
            const timeout = Math.round(displayRefreshRate * 2)
            const id = setTimeout(() => controller.abort(), timeout);
            const start = Date.now()

            try {
                const resp = await fetch(`${BASE_URL}/temp`, {
                    // signal: controller.signal,
                    headers: {
                        'Content-Type': 'application/json',
                        'Access-Control-Allow-Origin': '*',
                        'Acces-Controll-Allow-Headers': '*'
                    },
                });

                const body = await resp.json();
                console.log(body)
                let new_data: Data[] = []
                for (let i = 0; i < body.temp_max.length; i++) {
                    new_data.push({
                        temp_max: body.temp_max[i],
                        temp_min: body.temp_min[i],
                        temperature: body.temperatures[i],
                        timestamp: body.timestamp[i]
                    } satisfies Data)
                }

                dataStore.set([...data, ...new_data])
                const n = data.length
                let newProcessedData = []
                for (let i = n - 11; i < n; i++) {
                    newProcessedData.push({
                        name: Date.now() - data[i].timestamp,
                        temp_min: data[i].temp_min,
                        temp_max: data[i].temp_max,
                        temperature: data[i].temperature
                    })
                }
    
                setProcessedData(newProcessedData)
            } catch {
                console.warn(`No se pudo recibir los datos del servidor; Timeout: ${timeout}ms, Elaped: ${Date.now() - start}ms`)
            }           
        }, displayRefreshRate)
    }, [])

    return (
        <>
            <LineChart width={1000} height={500} data={processedData} className="custom-chart bg-white p-4 m-4 rounded-md">
                <CartesianGrid strokeDasharray="4" />
                <XAxis dataKey="name" padding={{ left: 30, right: 30 }} />
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