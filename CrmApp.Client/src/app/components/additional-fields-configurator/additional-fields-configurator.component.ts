import { Component, computed, OnInit, signal } from "@angular/core";
import { MatDialog } from "@angular/material/dialog";
import { AdditionalFieldsService, TableName, TableNames } from "src/app/core/services/additional-fields.service";
import { AdditionalFieldDTO, AdditionalFieldReorderDTO } from "src/models/DTO/AdditionalFieldDTO";
import { FieldFormDialogComponent } from "./field-form-dialog/field-form-dialog.component";
import { DialogConfig } from "src/models/view-models/DialogConfig";
import { CdkDragDrop, moveItemInArray } from "@angular/cdk/drag-drop";
import { NotificationService } from "src/app/core/services/notification.service";

interface TableConfig {
  name: TableName;
  label: string;
  icon: string;
}

@Component({
  selector: 'crm-additional-fields-configurator',
  templateUrl: './additional-fields-configurator.component.html',
  styleUrls: ['./additional-fields-configurator.component.scss'],
  standalone: false
})
export class AdditionalFieldsConfiguratorComponent implements OnInit {

  readonly tables: TableConfig[] = [
    { name: TableNames.Contractors, label: 'Kontrahenci', icon: 'person_card' },
    { name: TableNames.CatalogItems, label: 'Produkty', icon: 'shopping_cart' },
    { name: TableNames.Users, label: 'Użytkownicy', icon: 'person_shield' },
  ];

  selectedTable = signal<TableConfig>(this.tables[0]);
  fields = signal<AdditionalFieldDTO[]>([]);
  selectedField = signal<AdditionalFieldDTO | null>(null);
  loading = signal(false);
  saving = signal(false);

  listFields = computed(() =>
    this.fields().filter(f => f.isShowOnLists));

  constructor(
    private additionalFieldsService: AdditionalFieldsService,
    private dialog: MatDialog,
    private notify: NotificationService
  ) { }

  ngOnInit() {
    this.loadFields();
  }

  selectTable(table: TableConfig) {
    this.selectedTable.set(table);
    this.selectedField.set(null);
    this.loadFields();
  }

  selectField(field: AdditionalFieldDTO) {
    this.selectedField.set(field);
  }

  async loadFields() {
    this.loading.set(true);
    try {
      const result = await this.additionalFieldsService
        .getFieldsForTable(this.selectedTable().name);
      this.fields.set(result ?? []);
    } finally {
      this.loading.set(false);
    }
  }

  openAddDialog() {
    const ref = this.dialog.open<FieldFormDialogComponent>(FieldFormDialogComponent, {
      width: '640px',
      data: {
        mode: 'create',
        tableName: this.selectedTable().name
      }
    });

    ref.afterClosed().subscribe(result => {
      if (result) {
        this.fields.update(fields => [...fields, result]);
        this.selectedField.set(result);
      }
    });
  }

  openEditDialog(field: AdditionalFieldDTO) {
    const ref = this.dialog.open<FieldFormDialogComponent>(FieldFormDialogComponent, {
      width: '640px',
      data: {
        mode: 'edit',
        tableName: this.selectedTable().name,
        field
      }
    });

    ref.afterClosed().subscribe(result => {
      if (result) {
        this.fields.update(fields =>
          fields.map(f => f.id === result.id ? result : f));
        this.selectedField.set(result);
      }
    });
  }

  async deleteField(field: AdditionalFieldDTO) {
    this.notify.confirm(`Czy usunąć pole "${field.fieldName}" z tabeli "${this.selectedTable().label}"? Usunięte zostaną też wszystkie zapisane wartości.`)
    .subscribe(async (confirmed) => {
      if (!confirmed)
        return;
      this.saving.set(true);
      try {
        await this.additionalFieldsService.deleteField(field.id);
        this.fields.update(fields => fields.filter(f => f.id !== field.id));
        if (this.selectedField()?.id === field.id)
          this.selectedField.set(null);
      } finally {
        this.saving.set(false);
      }
    });
  }

  async onDrop(event: CdkDragDrop<AdditionalFieldDTO[]>) {
    if (event.previousIndex === event.currentIndex) return;

    const updated = [...this.fields()];
    moveItemInArray(updated, event.previousIndex, event.currentIndex);

    const orders: Record<number, number> = {};
    updated.forEach((f, i) => {
      f.sortOrder = i + 1;
      orders[f.id] = i + 1;
    });

    this.fields.set(updated);

    const dto: AdditionalFieldReorderDTO = {
      tableName: this.selectedTable().name,
      orders
    };

    this.saving.set(true);
    try {
      await this.additionalFieldsService.reorderFields(dto);
    } finally {
      this.saving.set(false);
    }
  }

  fieldTypeLabel(field: AdditionalFieldDTO): string {
    const map: Record<number, string> = {
      0: 'Liczba całkowita',
      1: 'Liczba dziesiętna',
      2: 'Tekst',
      3: 'Tak / Nie',
      4: 'Lista wartości'
    };
    return map[field.fieldType] ?? '—';
  }

  fieldTypeIcon(field: AdditionalFieldDTO): string {
    const map: Record<number, string> = {
      0: 'pin',
      1: 'calculate',
      2: 'text_fields',
      3: 'toggle_on',
      4: 'list'
    };
    return map[field.fieldType] ?? 'help_outline';
  }
}