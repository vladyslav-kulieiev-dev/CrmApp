import { CommonModule } from "@angular/common";
import { NgModule } from "@angular/core";
import { ReactiveFormsModule } from "@angular/forms";
import { MatButtonModule } from "@angular/material/button";
import { MatCheckboxModule } from "@angular/material/checkbox";
import { MatDividerModule } from "@angular/material/divider";
import { MatFormFieldModule } from "@angular/material/form-field";
import { MatIconModule } from "@angular/material/icon";
import { MatInputModule } from "@angular/material/input";
import { MatMenuModule } from "@angular/material/menu";
import { MatPaginatorIntl, MatPaginatorModule } from "@angular/material/paginator";
import { MatProgressSpinnerModule } from "@angular/material/progress-spinner";
import { MatSortModule } from "@angular/material/sort";
import { MatTableModule } from "@angular/material/table";
import { MatTooltipModule } from "@angular/material/tooltip";
import { CrmTableComponent, CrmTableToolbarDirective } from "./crm-table.component";
import { CustomPaginatorIntl } from "src/app/core/intl/CustomPaginatorIntl";

@NgModule({
    declarations: [CrmTableComponent, CrmTableToolbarDirective],
    exports:      [CrmTableComponent, CrmTableToolbarDirective],
    imports: [
        CommonModule,
        ReactiveFormsModule,
        MatTableModule,
        MatSortModule,
        MatPaginatorModule,
        MatMenuModule,
        MatButtonModule,
        MatIconModule,
        MatFormFieldModule,
        MatInputModule,
        MatCheckboxModule,
        MatDividerModule,
        MatProgressSpinnerModule,
        MatTooltipModule
    ],
    providers: [{ provide: MatPaginatorIntl, useClass: CustomPaginatorIntl }]
})
export class CrmTableModule {}