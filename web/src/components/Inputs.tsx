import React, { useState } from 'react';
import { twMerge } from 'tailwind-merge';

const Inputs = () => {
  const [minTemperature, setMinTemperature] = useState('');
  const [maxTemperature, setMaxTemperature] = useState('');
  const [roundDuration, setRoundDuration] = useState('');
  const [internalRate, setInternalRate] = useState('');
  const [refreshRate, setRefreshRate] = useState('');

  const validateInputs = () => {
    const tempValidFormat = /^((1[2-9]|2[0-9]|30);)*(1[2-9]|2[0-9]|30);?$/; // Números del 12 al 30 seguidos de ';'
    const genericValidFormat = /^(\d+;)*\d+;?$/; // Cualquier número seguido de ';'
    
    const minValid = tempValidFormat.test(minTemperature);
    const maxValid = tempValidFormat.test(maxTemperature);
    const roundValid = genericValidFormat.test(roundDuration);
    
    const minValues = minTemperature.split(';').map(Number);
    const maxValues = maxTemperature.split(';').map(Number);
    
    const allTempsValid = minValues.every((min, index) => min < maxValues[index]);

    const entries = [minTemperature, maxTemperature, roundDuration];
    const allSameLength = new Set(entries.map(entry => entry.split(';').filter(Boolean).length)).size === 1;

    if (!minValid || !maxValid || !roundValid || !allSameLength || !allTempsValid) {
      alert('Uno o más campos están mal escritos, no tienen la misma cantidad de elementos, no están en el rango de 12 a 30, o los valores mínimos no son menores que los máximos correspondientes.');
    } else {
      alert('Todos los datos son correctos.');
    }
  };

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
        Internal Rate (ms):
      </span>
      <input
        type="number"
        value={internalRate}
        onChange={(e) => setInternalRate(e.target.value)}
        className={twMerge("col-start-2 input")}
        placeholder='150'
      />
      
      <span className="col-start-1">
        Refresh Rate (ms):
      </span>
      <input
        type="number"
        value={refreshRate}
        onChange={(e) => setRefreshRate(e.target.value)}
        className={twMerge("col-start-2 input")}
        placeholder='2000'
      />
      
      <button className="col-start-2 mt-4 bg-blue-500 hover:bg-blue-700 text-white font-bold py-2 px-4 rounded" onClick={validateInputs}>
        Enviar
      </button>
    </section>
  );
};

export default Inputs;
