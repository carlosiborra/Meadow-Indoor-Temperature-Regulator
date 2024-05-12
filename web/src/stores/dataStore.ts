import { atom } from 'nanostores'
import type { Data } from '@/types/Data'

export const dataStore = atom<Data[]>([])