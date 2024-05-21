import { atom } from "nanostores";

export const fetchStatusStore = atom<'start' | 'stop'>('stop');