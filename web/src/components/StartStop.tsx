import React, { useState, useEffect } from "react";
import Button from "@/components/Button";
import { useToast } from "./ui/use-toast";
import { useStore } from "@nanostores/react";
import { displayRefreshRateStore } from "@/stores/displayRefreshRateStore";
import { fetchStatusStore } from "@/stores/fetchStatusStore";
import { dataStore } from "@/stores/dataStore";
import { roundDurationStore } from "@/stores/roundDurationStore"; // New store

const PUBLIC_BASE_URL =
    import.meta.env.PUBLIC_BASE_URL ?? "http://localhost:3000";

export default function StartStop() {
    const { toast } = useToast();
    const displayRefreshRate = useStore(displayRefreshRateStore);
    const roundDurations = useStore(roundDurationStore); // Get round durations
    const [globalTimeLeft, setGlobalTimeLeft] = useState(0);
    const [roundTimeLeft, setRoundTimeLeft] = useState(0);
    const [isRunning, setIsRunning] = useState(false);
    const [currentRound, setCurrentRound] = useState(0);
    const [timeInRange, setTimeInRange] = useState(0); // State for time in range
    const [dentroDeRango, setDentroDeRango] = useState(0); // State for count in range
    const fetchStatus = useStore(fetchStatusStore);

    const handleStart = async () => {
        fetchStatusStore.set("start");
        dataStore.set([]);
        setIsRunning(true);
        const response = await startRonda();
        setCurrentRound(0);
    };

    const handleStop = async () => {
        fetchStatusStore.set("stop");
        await fetch(`/api/download`, {
            method: "POST",
            headers: {
                "Content-Type": "application/json",
            },
            body: JSON.stringify({ data: dataStore.get() }),
        });
        stopRonda();
        setIsRunning(false);
    };

    const startRonda = async () => {
        const controller = new AbortController();
        const timeout = Math.round((displayRefreshRate * 2) / 3);
        const id = setTimeout(() => controller.abort(), timeout);
        const start = Date.now();

        try {
            await fetch(`${PUBLIC_BASE_URL}/start`, {
                signal: controller.signal,
            });
            toast({
                title: "Ok",
                description: "Ronda iniciada correctamente",
            });
            const globalDuration =
                roundDurations.reduce((acc, curr) => acc + curr, 0) * 10;
            setGlobalTimeLeft(globalDuration);
            console.log(`Round durations: ${roundDurations}`);
            setRoundTimeLeft(roundDurations[0] * 10);
            return true;
        } catch {
            toast({
                variant: "destructive",
                title: "Error",
                description: `No se pudo iniciar la ronda; Elapsed: ${
                    Date.now() - start
                }ms`,
            });
            return false;
        }
    };

    const stopRonda = async () => {
        const controller = new AbortController();
        const timeout = Math.round((displayRefreshRate * 2) / 3);
        const id = setTimeout(() => controller.abort(), timeout);
        const start = Date.now();

        try {
            await fetch(`${PUBLIC_BASE_URL}/shutdown`, {
                method: "POST",
                signal: controller.signal,
            });
            toast({
                title: "Ok",
                description: "Meadow apagada correctamente",
            });
        } catch {
            toast({
                variant: "destructive",
                title: "Error",
                description: `No se pudo apagar la Meadow; Timeout: ${timeout}ms, Elapsed: ${
                    Date.now() - start
                }ms`,
            });
        }
    };

    useEffect(() => {
        let globalInterval: NodeJS.Timeout;
        let roundInterval: NodeJS.Timeout;

        if (isRunning) {
            globalInterval = setInterval(() => {
                setGlobalTimeLeft((prev) => Math.max(prev - 1, 0));
            }, 100);

            roundInterval = setInterval(() => {
                setRoundTimeLeft((prev) => {
                    if (prev - 1 <= 0) {
                        const nextRound = currentRound + 1;
                        if (nextRound < roundDurations.length) {
                            setCurrentRound(nextRound);
                            setRoundTimeLeft(roundDurations[nextRound] * 10);
                        } else {
                            setIsRunning(false);
                            clearInterval(globalInterval);
                            clearInterval(roundInterval);
                            return 0;
                        }
                    }
                    return prev - 1;
                });
            }, 100);
        }

        return () => {
            clearInterval(globalInterval);
            clearInterval(roundInterval);
        };
    }, [isRunning, currentRound, roundDurations]);

    useEffect(() => {
        if (isRunning) {
            setRoundTimeLeft(roundDurations[currentRound] * 10);
        }
    }, [currentRound, roundDurations, isRunning]);
    
    useEffect(() => {
        console.log(`Base url: ${PUBLIC_BASE_URL}`);

        const timeInRange = async () => {
            const start = Date.now();
            try {
                const resp = await fetch(`${PUBLIC_BASE_URL}/temp`, {
                    headers: {
                        "Content-Type": "application/json",
                    },
                });
                if (!resp.ok) {
                    throw new Error(`HTTP error! status: ${resp.status}`);
                }

                const body = await resp.json();
                console.log("ponemos algo por aqui1");
                console.log(body);
                console.log(body.temp_max);
                console.log(body.temp_min);
                console.log(body.temperatures);
                console.log("ponemos algo por aqui2");

                // Variables para calcular el tiempo en rango
                let timeInRange = 0;
                let dentroDeRango = 0;
                const { temp_min, temp_max, temperatures, timestamp } = body;

                // Calcular el tiempo y la cantidad de veces en el que la temperatura está dentro del rango
                for (let i = 0; i < temperatures.length; i++) {
                    if (
                        temperatures[i] >= temp_min[i] &&
                        temperatures[i] <= temp_max[i]
                    ) {
                        dentroDeRango++; // Incrementar el contador
                        if (i === 0) {
                            timeInRange += timestamp[i + 1] - timestamp[i];
                        } else {
                            timeInRange += timestamp[i] - timestamp[i - 1];
                        }
                    }
                }

                setTimeInRange(timeInRange); // Set state for time in range
                setDentroDeRango(dentroDeRango); // Set state for count in range

                console.log(`Tiempo en rango: ${timeInRange}ms`);
                console.log(
                    `Cantidad de veces dentro de rango: ${dentroDeRango}`
                );
            } catch (error) {
                console.warn(
                    `No se pudo recibir los datos del servidor; Elapsed: ${
                        Date.now() - start
                    }ms`
                );
            }
        };
        let intervalId: NodeJS.Timeout;

        if (fetchStatus === "start") {
            intervalId = setInterval(timeInRange, displayRefreshRate);
        } else if (fetchStatus === "stop") {
            clearInterval(intervalId);
            // Cleat the graph
            dataStore.set([]);
        }

        return () => clearInterval(intervalId);
    }, [displayRefreshRate, fetchStatus]);

    return (
        <div className="flex flex-col w-[1000px] gap-4 mx-4 my-8">
            <div className="flex flex-row gap-4">
                <Button
                    onClick={handleStop}
                    className="bg-guardsman-red-700 text-light-primary w-full"
                >
                    Apagar
                </Button>
                <Button
                    onClick={handleStart}
                    className="w-full bg-fountain-blue-500 text-black"
                >
                    Empezar ronda
                </Button>
            </div>
            <hr className="w-full my-4 bg-fountain-blue-700 h-1" />
            <div className="flex flex-col items-center mt-4">
                <div className="text-guardsman-red-700">
                    <span className="flex flex-col items-center text-xl text-fountain-blue-500">
                        Round {currentRound + 1}
                    </span>
                    <br />
                    <span className="text-fountain-blue-500">
                        Global Time Left:{" "}
                    </span>
                    <span>{Math.round(globalTimeLeft / 10)} seconds</span>
                    <span> / {globalTimeLeft} deciseconds</span>
                </div>
                <div className="text-guardsman-red-700">
                    <span className="text-fountain-blue-500">
                        Round Time Left:{" "}
                    </span>
                    <span>{Math.round(roundTimeLeft / 10)} seconds</span>
                    <span> / {roundTimeLeft} deciseconds</span>
                </div>
                <div className="text-guardsman-red-700">
                    <span className="text-fountain-blue-500">
                        Time In Range:{" "}
                    </span>
                    <span>{Math.round(timeInRange / 1000)} seconds</span>
                    <span> / {Math.round(timeInRange / 100)} deciseconds</span>
                </div>
            </div>
        </div>
    );
}
