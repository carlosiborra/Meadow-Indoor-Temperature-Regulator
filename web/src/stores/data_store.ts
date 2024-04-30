import { atom } from "nanostores";
import type { Data } from "@/types/Data";

const BASE_URL = import.meta.env.BACKEND_URL ?? 'http://localhost:3000'
const FETCH_INTERVAL = import.meta.env.FETCH_INTERVAL ?? 2000

export const dataStore = atom<Data[]>([])

export function fetchData(): void {
    let last_element: Data = dataStore.get().pop() ?? {
        tmin: 12,
        tmax: 30,
        current_temp: 0,
        internal_rate: 0,
        refresh_rate: 0,
        round_duration: 0,
        timestamp: Date.now()
    }
    let current_temp = last_element.current_temp

    setTimeout(async () => {
        const resp = await fetch(`${BASE_URL}/temp`)
        if (resp.status === 200) {
            const data = await resp.json()
            if (data.temperatura != null) {
                current_temp = data.temperatura
            }
        }

    }, FETCH_INTERVAL * 2 / 3)

    last_element.current_temp = current_temp

    dataStore.set([...dataStore.get(), last_element])
}

export async function updateParams(
    pass: string,
    params: {
        tmin: number,
        tmax: number,
        internal_rate: number,
        refresh_rate: number,
        round_duration: number
    }
): Promise<void>{
    let element: Data = dataStore.get().pop() ?? {
        tmin: 12,
        tmax: 30,
        current_temp: 0,
        internal_rate: 0,
        refresh_rate: 0,
        round_duration: 0,
        timestamp: Date.now()
    }
    element = {...element, ...params} satisfies Data
    
    await fetch(`${BASE_URL}/setparams`, {
        method: 'POST',
        body: JSON.stringify({
            pass,
            temp_max: element.tmin,
            temp_min: element.tmax,
            display_refresh: element.internal_rate,
            refresh: element.refresh_rate,
            round_time: element.round_duration
        })
    })
}

