import { Pipe, PipeTransform } from '@angular/core';

@Pipe({
    name: 'sort',
    standalone: true
})
export class SortPipe implements PipeTransform {

    transform<T>(array: T[], property: keyof T | null, order: 'asc' | 'desc' = 'asc'): T[] {
        if (!array || array.length === 0) {
            return array;
        }

        return array.sort((a: T, b: T) => {
            const aValue = property ? a[property] : a;
            const bValue = property ? b[property] : b;

            if (aValue < bValue) {
                return order === 'asc' ? -1 : 1;
            } else if (aValue > bValue) {
                return order === 'asc' ? 1 : -1;
            } else {
                return 0;
            }
        });
    }

}
