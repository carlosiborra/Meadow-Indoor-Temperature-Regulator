import Button from '@/components/Button'
import { useToast } from './ui/use-toast'
import { useStore } from '@nanostores/react';
import { displayRefreshRateStore } from '@/stores/displayRefreshRateStore';
import { fetchStatusStore } from "@/stores/fetchStatusStore";
import { dataStore } from '@/stores/dataStore';

const PUBLIC_BASE_URL = import.meta.env.PUBLIC_BASE_URL ?? 'http://localhost:3000'

export default function StartStop() {
    const { toast } = useToast()

    const displayRefreshRate = useStore(displayRefreshRateStore);

    const handleStart = () => {
        fetchStatusStore.set('start');
        dataStore.set([])
    };

    const handleStop = () => {
        fetchStatusStore.set('stop');
        fetch(`/api/download`, {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json',
            },
            body: JSON.stringify({ data: dataStore.get() })
        })
    };


    return (
        <div className="flex flex-row w-[1000px] gap-4 mx-4 my-8">
            <Button
                onClick={async () => {

                    // Handle stop
                    handleStop();

                    const controller = new AbortController();
                    const timeout = Math.round(displayRefreshRate * 2 / 3)
                    const id = setTimeout(() => controller.abort(), timeout);
                    const start = Date.now()

                    try {
                        await fetch(`${PUBLIC_BASE_URL}/shutdown`, {
                            method: 'POST',
                            signal: controller.signal
                        });
                        toast({
                            title: 'Ok',
                            description: 'Meadow apagada correctamente',
                        });
                    } catch {
                        toast({
                            variant: 'destructive',
                            title: 'Error',
                            description: `No se pudo apagar la Meadow; Timeout: ${timeout}ms, Elaped: ${Date.now() - start}ms`
                        })
                    }
                }}
                className="bg-guardsman-red-700 text-light-primary w-full"
            >
                Apagar
            </Button>
            <Button
                onClick={async () => {
                    
                    // Handle start
                    handleStart();

                    const controller = new AbortController();
                    // const timeout = Math.round(displayRefreshRate * 2 / 3)
                    // const id = setTimeout(() => controller.abort(), timeout);
                    const start = Date.now()

                    try {
                        await fetch(`${PUBLIC_BASE_URL}/start`, {
                            signal: controller.signal
                        });
                        toast({
                            title: 'Ok',
                            description: 'Ronda iniciada correctamente'
                        });
                    } catch {
                        toast({
                            variant: 'destructive',
                            title: 'Error',
                            description: `No se pudo iniciar la ronda; Elaped: ${Date.now() - start}ms`  // ${timeout}
                        })
                    }
                }}
                className="w-full bg-fountain-blue-500 text-black"
            >
                Empezar ronda
            </Button>
        </div>
    )
}
