import React, { useState } from 'react';
import { twMerge } from 'tailwind-merge';
import { useToast } from './ui/use-toast';
import Button from './Button';
import { useStore } from '@nanostores/react';
import { displayRefreshRateStore } from '@/stores/displayRefreshRateStore';


const Inputs = () => {
  const [minTemperature, setMinTemperature] = useState('');
  const [maxTemperature, setMaxTemperature] = useState('');
  const [roundDuration, setRoundDuration] = useState('');
  const [internalRate, setInternalRate] = useState('');
  const [refreshRate, setRefreshRate] = useState('');
  const { toast } = useToast()

  const displayRefreshRate = useStore(displayRefreshRateStore);

  function validateInputs(): boolean {
    if (!minTemperature || !maxTemperature || !roundDuration || !internalRate || !refreshRate) {
      toast({
        title: 'Error',
        variant: 'destructive',
        description: 'Todos los campos deben estar llenos',
      })
      return false;
    }

    const tempValidFormat = /^(1[2-9]|2[0-9]|30)(;(1[2-9]|2[0-9]|30))*$/; // Números del 12 al 30 seguidos de ';'
    const genericValidFormat = /^\d+(;\d+)*$/; // Cualquier número seguido de ';'

    const minValid = tempValidFormat.test(minTemperature);
    const maxValid = tempValidFormat.test(maxTemperature);
    const roundValid = genericValidFormat.test(roundDuration);

    const minValues = minTemperature.split(';').map(Number);
    const maxValues = maxTemperature.split(';').map(Number);

    const allTempsValid = minValues.every((min, index) => min < maxValues[index]);

    const entries = [minTemperature, maxTemperature, roundDuration];
    const allSameLength = new Set(entries.map(entry => entry.split(';').filter(Boolean).length)).size === 1;

    if (!minValid) {
      toast({
        title: 'Error',
        variant: 'destructive',
        description: 'La temperatura mínima no tiene un formato válido'
      })
      return false;
    } if (!maxValid) {
      toast({
        title: 'Error',
        variant: 'destructive',
        description: 'La temperatura máxima no tiene un formato válido'
      })
      return false;
    } if (!roundValid) {
      toast({
        title: 'Error',
        variant: 'destructive',
        description: 'Los números de ronda no tienen un formato válido',
      })
      return false;
    } if (!allSameLength) {
      toast({
        title: 'Error',
        variant: 'destructive',
        description: 'Las 3 primeras entradas deben tener la misma cantidad de valores',
      })
      return false;
    } if (!allTempsValid) {
      toast({
        title: 'Error',
        variant: 'destructive',
        description: 'Las temperaturas no son válidas, recuerda que la temperatura mínima debe ser menor a la máxima.',
      })
      return false;
    }
    if (parseInt(internalRate) < 1) {
      toast({
        title: 'Error',
        variant: 'destructive',
        description: 'La tasa interna debe ser mayor a 0',
      })
      return false;
    }
    if (parseInt(refreshRate) < 200) {
      toast({
        title: 'Error',
        variant: 'destructive',
        description: 'La tasa de refresco debe ser un mayor que 200',
      })
      return false;
    }
    return true;
  };

  async function setParams(): Promise<void> {
    if (validateInputs()) {
      displayRefreshRateStore.set(parseInt(refreshRate));

      const minValues = minTemperature.split(';').map(Number);
      const maxValues = maxTemperature.split(';').map(Number);
      const roundValues = roundDuration.split(';').map(Number);

      const password = localStorage.getItem('password') ?? ''
      console.log(password)

      const params = {
        passs: password,
        tmin: minValues,
        tmax: maxValues,
        round_duration: roundValues,
        internal_rate: Number(internalRate),
      };

      const controller = new AbortController();
      const timeout = Math.round(displayRefreshRate * 2 / 3)
      const id = setTimeout(() => controller.abort(), timeout);
      const start = Date.now()

      try {
        const response = await fetch('http://localhost:5000/params?', {
          method: 'POST',
          headers: {
            'Content-Type': 'application/json',
          },
          body: JSON.stringify(params),
          signal: controller.signal
        });
        if (!response.ok) {
          throw new Error('HTTP error ' + response.status);
        }
        await response.json();
        toast({
          title: 'Ok',
          description: 'Parámetros guardados correctamente',
        });
      } catch {
        toast({
          variant: 'destructive',
          title: 'Error',
          description: `No se pudo enviar los datos al servidor; Timeout: ${timeout}ms, Elaped: ${Date.now() - start}ms`
        })
      }
    }
  }

  return (
    <section className="py-4 px-16 grid grid-cols-2 gap-4 w-[650px] items-center">
      <span className="col-start-1">
        Min Temperature (°C):
      </span>
      <input
        type="text"
        value={minTemperature}
        onChange={(e) => setMinTemperature(e.target.value)}
        className={twMerge("col-start-2 input text-fountain-blue-500 placeholder-fountain-blue-opacity-50")}
        placeholder="12; 15; 14..."
      />

      <span className="col-start-1">
        Max Temperature (°C):
      </span>
      <input
        type="text"
        value={maxTemperature}
        onChange={(e) => setMaxTemperature(e.target.value)}
        className={twMerge("col-start-2 input text-guardsman-red-500 placeholder-guardsman-red-opacity-50")}
        placeholder="25; 29; 30..."
      />

      <span className="col-start-1">
        Round Duration (s):
      </span>
      <input
        type="text"
        value={roundDuration}
        onChange={(e) => setRoundDuration(e.target.value)}
        className={twMerge("col-start-2 input")}
        placeholder="15; 20; 34..."
      />

      <div style={{ margin: '20px 0', background: 'white', height: '2px', gridColumn: "span 2" }}></div>

      <span className="col-start-1">
        Internal rate (ms):
      </span>
      <input
        type="number"
        value={internalRate}
        onChange={(e) => setInternalRate(e.target.value)}
        className={twMerge("col-start-2 input")}
        placeholder='150'
      />

      <span className="col-start-1">
        Display Refresh rate (ms):
      </span>
      <input
        type="number"
        value={refreshRate}
        onChange={(e) => setRefreshRate(e.target.value)}
        className={twMerge("col-start-2 input")}
        placeholder='2000'
      />

      <Button className="col-start-2 mt-4 bg-fountain-blue-500 text-black" onClick={setParams}>
        Enviar
      </Button>
    </section>
  );
};

export default Inputs;
