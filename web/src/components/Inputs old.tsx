import React, { useState, useEffect, useRef, type ChangeEventHandler } from 'react'
import type { Data } from '../types/Data'
import { useStore } from '@nanostores/react'
import { twMerge } from 'tailwind-merge'
import { FETCH_INTERVAL, dataStore, updateParams } from '@/stores/data_store'

export default function InputsOld(): React.JSX.Element {
    const [data, setData] = useState(dataStore.get().pop())
    const tminRef = useRef<HTMLInputElement>(null);
    const tmaxRef = useRef<HTMLInputElement>(null);
    const roundDurationRef = useRef<HTMLInputElement>(null);
    const refreshRateRef = useRef<HTMLInputElement>(null);
    const internalRateRef = useRef<HTMLInputElement>(null);

    useEffect(() => {
        setInterval(() => {
            setData(dataStore.get().pop())
        }, FETCH_INTERVAL)
    }, [])

    const onChangeFunc = () => {
        console.log("Algo cambió")
        const tmin = parseInt(tminRef.current?.value ?? `${data?.tmin}` ?? '12')
        const tmax = parseInt(tmaxRef.current?.value ?? `${data?.tmax}` ?? '30')
        const round_duration = parseInt(roundDurationRef.current?.value ?? `${data?.round_duration}` ?? '0')
        const refresh_rate = parseInt(refreshRateRef.current?.value ?? `${data?.refresh_rate}` ?? '0')
        const internal_rate = parseInt(internalRateRef.current?.value ?? `${data?.internal_rate}` ?? '0')

        const password = localStorage.getItem('password') ?? ''
        console.log(password)

        setData({...data, tmin, tmax, internal_rate, refresh_rate, round_duration } as Data)
        updateParams(password, { tmin, tmax, internal_rate, refresh_rate, round_duration })
    }

    return (
        <section className="py-4 px-16 grid grid-cols-2 gap-4 w-[450px] items-center">
            <span className='col-start-1'>
                Temperature Range:
            </span>
            <div className='col-start-2 flex flex-row gap-2 items-center'>
                <InputElement r={tminRef} id="tmin" onChangeFunc={onChangeFunc} value={data?.tmin ?? 12} units="°C" className='text-fountain-blue-500' />
                <span className='font-bold'>-</span>
                <InputElement r={tmaxRef} id="tmax" onChangeFunc={onChangeFunc} value={data?.tmax ?? 30} units="°C" className='text-guardsman-red-500' />
            </div>

            <span className='col-start-1'>
                Round duration:
            </span>
            <InputElement r={roundDurationRef} id="round_duration" onChangeFunc={onChangeFunc} value={data?.round_duration ?? 0} units="s" className='col-start-2' />

            <span className='col-start-1'>
                Refresh Rate:
            </span>
            <InputElement r={refreshRateRef} id="refresh_rate" onChangeFunc={onChangeFunc} value={data?.refresh_rate ?? 0} units="ms" className='col-start-2' />

            <span className='col-start-1'>
                Internal Rate:
            </span>
            <InputElement r={internalRateRef} id="internal_rate" onChangeFunc={onChangeFunc} value={data?.internal_rate ?? 0} units="ms" className='col-start-2' />
        </section>
    )
}

const InputElement = ({ id, value, className, units, onChangeFunc, r }: {
    id: string,
    value: number,
    units: string,
    className?: string,
    onChangeFunc: ChangeEventHandler<HTMLInputElement>,
    r: React.ForwardedRef<HTMLInputElement>
}): React.JSX.Element => {
    return <label className={twMerge(
        'input flex flex-row-reverse items-center gap-0',
        'p-0 w-14 text-center text-light-primary bg-dark-secondary rounded-md',
        className
    )}>
        <span className='text-start pr-1'>
            {` ${units}`}
        </span>
        <input  ref={r} onChange={onChangeFunc} type="number" className="m-0 w-8 text-end" value={value} />
    </label>
}