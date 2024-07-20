import { CdkCellDef } from "@angular/cdk/table";
import { Directive, Input } from "@angular/core";
import { MatCellDef, MatTable } from "@angular/material/table";

/** From https://nartc.me/blog/typed-mat-cell-def/ */
@Directive({
    selector: "[matCellDef]",
    providers: [{ provide: CdkCellDef, useExisting: TypeSafeMatCellDef }],
    standalone: true,
})
export class TypeSafeMatCellDef<T> extends MatCellDef {

    // leveraging syntactic-sugar syntax when we use *matCellDef
    @Input() matCellDefTable?: MatTable<T>;

    // ngTemplateContextGuard flag to help with the Language Service
    static ngTemplateContextGuard<T>(
        dir: TypeSafeMatCellDef<T>,
        ctx: unknown,
    ): ctx is { $implicit: T; index: number } {
        return true;
    }
}