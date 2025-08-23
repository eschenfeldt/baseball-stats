import { CdkCellDef } from "@angular/cdk/table";
import { Directive, Input } from "@angular/core";
import { MatRowDef, MatTable } from "@angular/material/table";
import { TypeSafeMatCellDef } from "./type-safe-mat-cell-def.directive";

/**From https://nartc.me/blog/typed-mat-cell-def/ */
@Directive({
    selector: "[matRowDef]",
    providers: [{ provide: CdkCellDef, useExisting: TypeSafeMatCellDef }],
    standalone: true,
})
export class TypeSafeMatRowDef<T> extends MatRowDef<T> {

    // leveraging syntactic-sugar syntax when we use *matCellDef
    @Input() matRowDefTable?: MatTable<T>;

    // ngTemplateContextGuard flag to help with the Language Service
    static ngTemplateContextGuard<T>(
        dir: TypeSafeMatRowDef<T>,
        ctx: unknown,
    ): ctx is { $implicit: T; index: number } {
        return true;
    }
}