import { StatFormat } from './stat-format';

export interface StatDef {
    name: string;
    shortName?: string;
    format: StatFormat
}

export type StatDefCollection = {
    [name: string]: StatDef
}