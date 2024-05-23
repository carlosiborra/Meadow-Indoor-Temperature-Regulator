import React, { useState, useEffect } from 'react';
import Button from '@/components/Button';
import { useToast } from './ui/use-toast';
import { useStore } from '@nanostores/react';
import { displayRefreshRateStore } from '@/stores/displayRefreshRateStore';
import { fetchStatusStore } from "@/stores/fetchStatusStore";
import { dataStore } from '@/stores/dataStore';
import { roundDurationStore } from '@/stores/roundDurationStore'; // New store

const PUBLIC_BASE_URL = import.meta.env.PUBLIC_BASE_URL ?? 'http://localhost:3000';

export default function StartStop() {
  const { toast } = useToast();
  const displayRefreshRate = useStore(displayRefreshRateStore);
  const roundDurations = useStore(roundDurationStore); // Get round durations
  const [globalTimeLeft, setGlobalTimeLeft] = useState(0);
  const [roundTimeLeft, setRoundTimeLeft] = useState(0);
  const [isRunning, setIsRunning] = useState(false);
  const [currentRound, setCurrentRound] = useState(0);

  const handleStart = async () => {
    fetchStatusStore.set('start');
    dataStore.set([]);
    setIsRunning(true);
    const response = await startRonda();
    setCurrentRound(0);
};

  const handleStop = async () => {
    fetchStatusStore.set('stop');
    await fetch(`/api/download`, {
      method: 'POST',
      headers: {
        'Content-Type': 'application/json',
      },
      body: JSON.stringify({ data: dataStore.get() }),
    });
    stopRonda();
    setIsRunning(false);
  };

  const startRonda = async () => {
    const controller = new AbortController();
    const timeout = Math.round(displayRefreshRate * 2 / 3);
    const id = setTimeout(() => controller.abort(), timeout);
    const start = Date.now();
    
    try {
        await fetch(`${PUBLIC_BASE_URL}/start`, {
            signal: controller.signal,
        });
        toast({
            title: 'Ok',
            description: 'Ronda iniciada correctamente',
        });
        const globalDuration = roundDurations.reduce((acc, curr) => acc + curr, 0) * 10;
        setGlobalTimeLeft(globalDuration);
        console.log(`Round durations: ${roundDurations}`);
        setRoundTimeLeft(roundDurations[0] * 10);
        return true;
    } catch {
      toast({
        variant: 'destructive',
        title: 'Error',
        description: `No se pudo iniciar la ronda; Elapsed: ${Date.now() - start}ms`,
      });
      return false;
    }
  };

  const stopRonda = async () => {
    const controller = new AbortController();
    const timeout = Math.round(displayRefreshRate * 2 / 3);
    const id = setTimeout(() => controller.abort(), timeout);
    const start = Date.now();

    try {
      await fetch(`${PUBLIC_BASE_URL}/shutdown`, {
        method: 'POST',
        signal: controller.signal,
      });
      toast({
        title: 'Ok',
        description: 'Meadow apagada correctamente',
      });
    } catch {
      toast({
        variant: 'destructive',
        title: 'Error',
        description: `No se pudo apagar la Meadow; Timeout: ${timeout}ms, Elapsed: ${Date.now() - start}ms`,
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
            <span className="flex flex-col items-center text-xl text-fountain-blue-500">Round {currentRound + 1}</span>
            <br />
            <span className='text-fountain-blue-500'>Global Time Left: </span>
            <span>{Math.round(globalTimeLeft / 10)} seconds</span>
            <span> / {globalTimeLeft} deciseconds</span>
          </div>
          <div className="text-guardsman-red-700">
            <span className='text-fountain-blue-500'>Round Time Left: </span>
            <span>{Math.round(roundTimeLeft / 10)} seconds</span>
            <span> / {roundTimeLeft} deciseconds</span>
          </div>
        </div>
    </div>
  );
}
