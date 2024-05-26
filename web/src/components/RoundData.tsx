import React from "react";
import { useStore } from "@nanostores/react";
import { timeInRangeStore } from "@/stores/timeInRangeStore";
import { timeElapsedStore } from "@/stores/timeElapsedStore";
import { roundDurationStore } from "@/stores/roundDurationStore";
import { roundNumberStore } from "@/stores/roundNumberStore";

export default function RoundData() {
    const timeInRange = useStore(timeInRangeStore);
    const timeElapsed = useStore(timeElapsedStore);
    const roundDuration = useStore(roundDurationStore);

    const roundNumber = useStore(roundNumberStore);

    return roundNumber === 0 ? (
        <></>
    ) : (
        <div className="px-8 py-4 flex flex-col items-center justify-center w-full text-lg gap-4">
            <h4 className="text-3xl text-fountain-blue-500">Ronda {roundNumber}</h4>
            <p>
                <span className="font-bold">Duración total de la ronda: </span>
                {roundDuration}s ({roundDuration*10}ds)
            </p>
            <p>
                <span className="font-bold">Tiempo en el rango: </span>
                <span
                    className={
                        timeInRange / timeElapsed > 0.25 
                            ? 'text-green-500'
                            : timeInRange / timeElapsed > 0.12 
                                ? 'text-yellow-500' 
                                : 'text-guardsman-red-600'
                    }
                >{Math.round(timeInRange / 1000)}s</span> / {Math.round(timeElapsed / 1000)}s (
                <span
                    className={
                        timeInRange / timeElapsed > 0.25 
                            ? 'text-green-500'
                            : timeInRange / timeElapsed > 0.12
                                ? 'text-yellow-500' 
                                : 'text-guardsman-red-600'
                    }
                >{Math.round(timeInRange / 100)}ds</span>{" "}/ {Math.round(timeElapsed / 100)}ds)
            </p>
        </div>
    );
}
