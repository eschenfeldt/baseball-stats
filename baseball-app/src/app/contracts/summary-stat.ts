import { StatCategory } from './stat-category';
import { StatDef } from './stat-def';

export interface SummaryStat {
    category: StatCategory;
    definition: StatDef;
    value?: number;
}
