// input.tsx
import React, { useState } from 'react';
import { twMerge } from 'tailwind-merge';
import { useToast } from './ui/use-toast';
import Button from './Button';
import { useStore } from '@nanostores/react';
import { displayRefreshRateStore } from '@/stores/displayRefreshRateStore';
import { internalRefreshRateStore } from '@/stores/internalRefreshRateStore';
import { roundDurationStore } from '@/stores/roundDurationStore'; // New store

const Inputs = () => {
  const [minTemperature, setMinTemperature] = useState('');
  const [maxTemperature, setMaxTemperature] = useState('');
  const [roundDuration, setRoundDuration] = useState('');
  const [internalRate, setInternalRate] = useState('');
  const [refreshRate, setRefreshRate] = useState('');
  const { toast } = useToast();

  const displayRefreshRate = useStore(displayRefreshRateStore);
  const PUBLIC_BASE_URL = import.meta.env.PUBLIC_BASE_URL ?? 'http://localhost:3000';

  function validateInputs(): boolean {
    if (!minTemperature || !maxTemperature || !roundDuration || !internalRate || !refreshRate) {
      toast({
        title: 'Error',
        variant: 'destructive',
        description: 'Todos los campos deben estar llenos',
      });
      return false;
    }

    const tempValidFormat = /^(1[2-9](\.\d)?|2[0-9](\.\d)?|30(\.0)?)(;(1[2-9](\.\d)?|2[0-9](\.\d)?|30(\.0)?))*$/; // Números del 12 al 30 seguidos de ';'
    const genericValidFormat = /^\d+(;\d+)*$/; // Cualquier número seguido de ';'

    const minValid = tempValidFormat.test(minTemperature);
    const maxValid = tempValidFormat.test(maxTemperature);
    const roundValid = genericValidFormat.test(roundDuration);

    const minValues = minTemperature.split(';').map(Number);
    const maxValues = maxTemperature.split(';').map(Number);

    const allTempsValid = minValues.every((min, index) => min < maxValues[index]);

    const entries = [minTemperature, maxTemperature, roundDuration];
    const allSameLength = new Set(entries.map(entry => entry.split(';').filter(Boolean).length)).size === 1;

    let description = '';

    if (!minValid) {
      description = 'La temperatura mínima no tiene un formato válido';
    } else if (!maxValid) {
      description = 'La temperatura máxima no tiene un formato válido';
    } else if (!roundValid) {
      description = 'Los números de ronda no tienen un formato válido';
    } else if (!allSameLength) {
      description = 'Las 3 primeras entradas deben tener la misma cantidad de valores';
    } else if (!allTempsValid) {
      description = 'Las temperaturas no son válidas, recuerda que la temperatura mínima debe ser menor a la máxima.';
    } else if (parseInt(internalRate) < 1) {
      description = 'La tasa interna debe ser mayor a 0';
    } else if (parseInt(refreshRate) < 200) {
      description = 'La tasa de refresco debe ser un mayor que 200';
    } else {
      return true;
    }
    toast({
      title: 'Error',
      variant: 'destructive',
      description,
    });
    return false;
  }

  async function setParams(): Promise<void> {
    if (validateInputs()) {
      displayRefreshRateStore.set(parseInt(refreshRate));
      roundDurationStore.set(roundDuration.split(';').map(Number).reduce((p, a) => p + a, 0))
      internalRefreshRateStore.set(parseInt(internalRate))

      const password = localStorage.getItem('password') ?? '';

      // Note: data is passed as str and the backed does the transformation
      const params = {
        pass: password,
        temp_min: minTemperature,
        temp_max: maxTemperature,
        round_time: roundDuration,
        refresh: Number(internalRate),
      };

      const controller = new AbortController();
      const start = Date.now();

      try {
        const response = await fetch(`${PUBLIC_BASE_URL}/setparams`, {
          method: 'POST',
          headers: {
            'Content-Type': 'application/json',
          },
          body: JSON.stringify(params),
          // signal: controller.signal,
        });
        if (!response.ok) {
          throw new Error('HTTP error ' + response.status);
        }
        toast({
          title: 'Ok',
          description: 'Parámetros guardados correctamente',
        });
      } catch {
        toast({
          variant: 'destructive',
          title: 'Error',
          description: `No se pudo enviar los datos al servidor; Elapsed: ${Date.now() - start}ms`,
        });
      }
    }
  }

  return (
    <section className="py-4 px-16 grid grid-cols-2 gap-4 w-[750px] items-center">
      <span className="col-start-1">
        Temperatura Min (°C):
      </span>
      <input
        type="text"
        value={minTemperature}
        onChange={(e) => setMinTemperature(e.target.value)}
        className={twMerge("col-start-2 input text-fountain-blue-500 placeholder-fountain-blue-opacity-50")}
        placeholder="12; 15; 14..."
      />

      <span className="col-start-1">
        Temperatura Max (°C):
      </span>
      <input
        type="text"
        value={maxTemperature}
        onChange={(e) => setMaxTemperature(e.target.value)}
        className={twMerge("col-start-2 input text-guardsman-red-500 placeholder-guardsman-red-opacity-50")}
        placeholder="25; 29; 30..."
      />

      <span className="col-start-1">
        Duración rondas (s):
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
        Frecuencia medición Meadow (ms):
      </span>
      <input
        type="number"
        value={internalRate}
        onChange={(e) => setInternalRate(e.target.value)}
        className={twMerge("col-start-2 input")}
        placeholder='150'
      />

      <span className="col-start-1">
        Frecuencia actualización gráfica (ms):
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
