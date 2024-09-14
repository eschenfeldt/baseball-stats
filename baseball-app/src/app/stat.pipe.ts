import { Injector, Pipe, PipeTransform, Type } from '@angular/core';
import { StatFormat } from './contracts/stat-format';
import { DecimalPipe, PercentPipe } from '@angular/common';
import { StatDef } from './contracts/stat-def';

@Pipe({
    name: 'stat',
    standalone: true
})
export class StatPipe implements PipeTransform {

    constructor(private injector: Injector) { }

    transform(value: number | null | undefined, def: StatDef): string | null {
        let requiredPipe: Type<PipeTransform>
        if (StatPipe.percentageFormats.some(f => f === def.format)) {
            requiredPipe = PercentPipe
        } else {
            requiredPipe = DecimalPipe
        }
        const injector = Injector.create({
            name: 'StatPipe',
            parent: this.injector,
            providers: [
                { provide: requiredPipe }
            ]
        });
        const formatArgs = StatPipe.formatString(def.format);
        const pipe = injector.get(requiredPipe) as DecimalPipe | PercentPipe;
        return pipe.transform(value, formatArgs);
    }

    private static readonly percentageFormats = [
        StatFormat.percentage1
    ];

    private static formatString(format: StatFormat): string {
        switch (format) {
            case StatFormat.decimal2:
                return '0.2-2';
            case StatFormat.decimal3:
                return '0.3-3';
            case StatFormat.percentage1:
                return '0.1-1';
            default:
                return ''
        }
    }
}
