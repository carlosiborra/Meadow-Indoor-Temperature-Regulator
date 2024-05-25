import Button from "@/components/Button";
import { useToast } from "./ui/use-toast";
import { useStore } from "@nanostores/react";
import { displayRefreshRateStore } from "@/stores/displayRefreshRateStore";
import { fetchStatusStore } from "@/stores/fetchStatusStore";
import { dataStore } from "@/stores/dataStore";
import { roundNumberStore } from "@/stores/roundNumberStore";

const PUBLIC_BASE_URL =
    import.meta.env.PUBLIC_BASE_URL ?? "http://localhost:3000";

export default function StartStop() {
    const { toast } = useToast();
    const displayRefreshRate = useStore(displayRefreshRateStore);


    const handleStart = async () => {
        fetchStatusStore.set("start");
        dataStore.set([]);
        roundNumberStore.set(roundNumberStore.get()+1)
        await startRonda();
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
        await stopRonda();
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
        </div>
    );
}
