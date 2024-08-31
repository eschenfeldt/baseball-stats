export enum Format {
    integer = 'Integer',
    decimal = 'Decimal'
}

export interface StatFormat {
    name: Format,
    decimalPoints?: number
}
