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
        <div>
            <h4>Ronda {roundNumber}</h4>
            <p>
                <span>Duración total de la ronda:</span>
                {roundDuration}s
            </p>
            <p>
                <span>Tiempo en el rango:</span>
                <span>{timeInRange / 1000}s</span>/ {timeElapsed / 1000}s
            </p>
        </div>
    );
}
