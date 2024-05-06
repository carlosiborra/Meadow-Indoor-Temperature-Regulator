import { useEffect, useState } from "react";
import { dataStore, fetchData } from "../stores/data_store";
import {
    LineChart,
    Line,
    XAxis,
    YAxis,
    CartesianGrid,
    Tooltip,
    Legend,
} from "recharts";

export default function Grafica() {
    const [data2, setData2] = useState(dataStore.get());

    useEffect(() => {
        const unsubscribe = dataStore.subscribe(() => {
            setData2(dataStore.get().slice(-15));
        });

        return () => {
            unsubscribe();
        };
    }, []);

    useEffect(() => {
        const interval = setInterval(() => {
            fetchData();
        }, 2000);

        return () => clearInterval(interval);
    }, []);

    return (
        <>
            <LineChart width={1000} height={500} data={data2} className="custom-chart bg-white p-4 m-4 rounded-md">
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
