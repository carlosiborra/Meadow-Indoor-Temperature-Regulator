import React, { useEffect } from 'react'
import type { Data } from '../types/Data'
import { useStore } from '@nanostores/react'
import { dataStore } from '../stores/data_store'
import { twMerge } from 'tailwind-merge'

export default function Inputs(): React.JSX.Element {
    const dataStorage = useStore(dataStore)
    let data: Data | undefined

    useEffect(()=>{
        data = dataStorage.at(dataStorage.length)
    }, [dataStorage])

    return (
        <section className="py-4 px-16 grid grid-cols-2 gap-4 w-[450px] items-center">
            <span className='col-start-1'>
                Temperature Range:
            </span>
            <div className='col-start-2 flex flex-row gap-2 items-center'>
                <InputElement value={data?.tmin ?? 12} units="°C" className='text-fountain-blue-500'/>
                <span className='font-bold'>-</span>
                <InputElement value={data?.tmax ?? 30} units="°C" className='text-guardsman-red-500' />
            </div>

            <span className='col-start-1'>
                Round duration:
            </span>
            <InputElement value={data?.round_duration ?? 0} units="s" className='col-start-2'/>
            
            <span className='col-start-1'>
                Refresh Rate:
            </span>
            <InputElement value={data?.refresh_rate ?? 0} units="ms" className='col-start-2'/>

            <span className='col-start-1'>
                Internal Rate:
            </span>
            <InputElement value={data?.internal_rate ?? 0} units="ms" className='col-start-2'/>
        </section>
    )
}

const InputElement = ({ value, className, units }: {
    value: number,
    units: string,
    className?: string
}): React.JSX.Element => {
    return <label className={twMerge(
        'input flex flex-row-reverse items-center gap-0',
        'p-0 w-14 text-center text-light-primary bg-dark-secondary rounded-md',
        className
    )}>
        <span className='text-start pr-1'>
            {` ${units}`}
        </span>
        <input type="number" className="m-0 w-8 text-end" value={value}/>
    </label>
}