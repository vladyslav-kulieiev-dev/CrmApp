import { Component, Inject, OnInit, signal } from "@angular/core";
import { FormGroup, FormArray, FormBuilder, Validators, FormControl } from "@angular/forms";
import { MatDialogRef, MAT_DIALOG_DATA } from "@angular/material/dialog";
import { AdditionalFieldsService, TableName } from "src/app/core/services/additional-fields.service";
import { AuthService } from "src/app/core/services/auth.service";
import { DictionariesService } from "src/app/core/services/dictionaries.service";
import { NotificationService } from "src/app/core/services/notification.service";
import { ToolsService } from "src/app/core/services/tools.service";
import { UsersService } from "src/app/core/services/users.service";
import { Dictionaries } from "src/models/Dictionaries";
import { AdditionalFieldDTO } from "src/models/DTO/AdditionalFieldDTO";
import { UsersDTO } from "src/models/DTO/UsersDTO";
import { ValueNameDTO } from "src/models/DTO/ValueNameDTO";
import { EDictionaryType } from "src/models/enums/EDictionaryType";
import { EValueType } from "src/models/enums/EValueType";

export interface FieldFormDialogData {
  mode: 'create' | 'edit';
  tableName: TableName;
  field?: AdditionalFieldDTO;
}

interface RoleOption { id: string; name: string; }
interface UserOption { id: number; name: string; }

@Component({
  selector: 'crm-field-form-dialog',
  templateUrl: './field-form-dialog.component.html',
  styleUrls: ['./field-form-dialog.component.css'],
  standalone: false
})
export class FieldFormDialogComponent implements OnInit {

  readonly EValueType = EValueType;

  form!: FormGroup;
  saving = signal(false);
  loading = signal(true);

  dictionaries: Dictionaries[] = [];
  selectedDictionary: Dictionaries | null = null;
  roles: RoleOption[] = [];
  users: UsersDTO[] = [];

  valueTypeOptions: ValueNameDTO[] = [];

  roleSelectControl = new FormControl<string[]>([]);
  userSelectControl = new FormControl<number[]>([]);
  private addedRoleIds = new Set<string>();
  private addedUserIds = new Set<number>();

  get isEdit() { return this.data.mode === 'edit'; }
  get isList() { return this.form?.get('fieldType')?.value === EValueType.List; }
  get permissionsArray() { return this.form.get('permissions') as FormArray; }

  constructor(
    private fb: FormBuilder,
    private dialogRef: MatDialogRef<FieldFormDialogComponent>,
    @Inject(MAT_DIALOG_DATA) public data: FieldFormDialogData,
    private additionalFieldsService: AdditionalFieldsService,
    private dictionariesService: DictionariesService,
    private authService: AuthService,
    private usersService: UsersService,
    private notify: NotificationService,
    private toolsService: ToolsService
  ) { 
    this.toolsService.getEnumValues('EValueType').then(values => {
      this.valueTypeOptions = values ?? [];
    });
  }

  async ngOnInit() {
    this.buildForm();
    await this.loadDependencies();

    if (this.isEdit && this.data.field) {
      this.patchForm(this.data.field);
    }

    this.roleSelectControl.valueChanges.subscribe((selectedIds: string[] | null) => {
      this.onRolesSelectionChange();
    });

    this.userSelectControl.valueChanges.subscribe((selectedIds: number[] | null) => {
      this.onUsersSelectionChange();
    });

    this.loading.set(false);
  }

  private buildForm() {
    const f = this.data.field;
    this.form = this.fb.group({
      fieldName: [f?.fieldName ?? '', Validators.required],
      fieldType: [f?.fieldType ?? EValueType.String, Validators.required],
      dictionaryId: [f?.dictionaryId ?? null],
      isMultiple: [f?.isMultiple ?? false],
      isShowOnLists: [f?.isShowOnLists ?? true],
      isRequired: [f?.isRequired ?? false],
      defaultValue: [f?.defaultValue ?? ''],
      permissions: this.fb.array([])
    });

    this.form.get('fieldType')?.valueChanges.subscribe(type => {
      if (type !== EValueType.List) {
        this.form.patchValue({ dictionaryId: null, isMultiple: false });
      }
    });

    this.form.get('dictionaryId')?.valueChanges.subscribe(dictionaryId => {
      this.onDictionaryChange();
    });
  }

  private async loadDependencies() {
    this.dictionariesService.getAll().then((data) => {
      this.dictionaries = (data ?? []).filter(d => d.isActive);
    });

    this.authService.getRoles().subscribe(roles => {
      this.roles = roles.map(r => ({ id: r.id, name: r.name }));
    });

    this.usersService.getAll().then(users => {
      this.users = users ?? [];
    });
  }

  private patchForm(field: AdditionalFieldDTO) {
    this.form.patchValue({
      fieldName: field.fieldName,
      fieldType: field.fieldType,
      dictionaryId: field.dictionaryId,
      isMultiple: field.isMultiple,
      isShowOnLists: field.isShowOnLists,
      isRequired: field.isRequired,
      defaultValue: field.defaultValue
    });

    field.permissions.forEach(p => {
        this.addPermission(p);
        if (p.roleId) this.addedRoleIds.add(p.roleId);
        if (p.userId) this.addedUserIds.add(p.userId);
    });

    this.roleSelectControl.setValue(
        field.permissions.filter(p => p.roleId).map(p => p.roleId!),
        { emitEvent: false }
    );
    this.userSelectControl.setValue(
        field.permissions.filter(p => p.userId).map(p => p.userId!),
        { emitEvent: false }
    );
  }

  getRoleName(roleId: string | null): string {
    return this.roles.find(r => r.id === roleId)?.name ?? roleId ?? '—';
  }

  getUserName(userId: number | null): string {
    return this.users.find(u => u.id === userId)?.displayName ?? userId?.toString() ?? '—';
  }

  getUserInitials(userId: number | null): string {
    var user = this.users.find(u => u.id === userId);
    if (!user) return userId?.toString() ?? '—';
    return (user.firstName?.[0] ?? '') + (user.lastName?.[0] ?? '');
  }

  onRolesSelectionChange() {
    var rolesIds = this.roleSelectControl.value ?? [];
    this.permissionsArray.controls.forEach((ctrl, index) => {
      var roleId = ctrl.get('roleId')?.value as string | null;
      if (roleId && !rolesIds.includes(roleId)) {
        this.addedRoleIds.delete(roleId);
        this.permissionsArray.removeAt(index);
      }
    });
    var rolesToAdd = rolesIds.filter(id => !this.addedRoleIds.has(id));
    rolesToAdd.forEach(roleId => {
      this.addedRoleIds.add(roleId);
      this.addPermission({ roleId, canView: true, canEdit: false });
    });
  }

  onUsersSelectionChange() {
    var usersIds = this.userSelectControl.value ?? [];
    this.permissionsArray.controls.forEach((ctrl, index) => {
      var userId = ctrl.get('userId')?.value as number | null;
      if (userId && !usersIds.includes(userId)) {
        this.addedUserIds.delete(userId);
        this.permissionsArray.removeAt(index);
      }
    });
    var usersToAdd = usersIds.filter(id => !this.addedUserIds.has(id));
    usersToAdd.forEach(userId => {
      this.addedUserIds.add(userId);
      this.addPermission({ userId, canView: true, canEdit: false });
    });
  }

  addPermission(perm?: {
    roleId?: string; userId?: number;
    canView?: boolean; canEdit?: boolean;
  }) {
    this.permissionsArray.push(this.fb.group({
      roleId: [perm?.roleId ?? null],
      userId: [perm?.userId ?? null],
      canView: [perm?.canView ?? true],
      canEdit: [perm?.canEdit ?? false]
    }));
  }

  removePermission(index: number) {
    const ctrl = this.permissionsArray.at(index);
    const roleId = ctrl.get('roleId')?.value as string | null;
    const userId = ctrl.get('userId')?.value as number | null;

    if (roleId) {
        this.addedRoleIds.delete(roleId);
        this.roleSelectControl.setValue(
            (this.roleSelectControl.value ?? []).filter((id: string) => id !== roleId),
            { emitEvent: false }
        );
    }
    if (userId) {
        this.addedUserIds.delete(userId);
        this.userSelectControl.setValue(
            (this.userSelectControl.value ?? []).filter((id: number) => id !== userId),
            { emitEvent: false }
        );
    }

    this.permissionsArray.removeAt(index);
  }

  async save() {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }

    this.saving.set(true);
    try {
      const value = this.form.getRawValue();

      if (this.isEdit) {
        await this.additionalFieldsService.updateField(
          this.data.field!.id,
          { id: this.data.field!.id, tableName: this.data.tableName, ...value }
        ).then((res) => {
          this.saving.set(false);
          if (res?.succeeded) {
            this.notify.success('Pomyślnie zaktualizowano pole');
            this.dialogRef.close(res.data);
          } else {
            this.notify.error('Nie udało się zaktualizować pola');
          }
        }, (err) => {
          this.saving.set(false);
          this.notify.error('Nie udało się zaktualizować pola');
        });
      } else {
        await this.additionalFieldsService.createField(
          { tableName: this.data.tableName, ...value }
        ).then((res) => {
          this.saving.set(false);
          if (res?.succeeded) {
            this.notify.success('Pomyślnie utworzono pole');
            this.dialogRef.close(res.data);
          } else {
            this.notify.error('Nie udało się utworzyć pola');
          }
        }, (err) => {
          this.saving.set(false);
          this.notify.error('Nie udało się utworzyć pola');
        });
      }
    } finally {
      this.saving.set(false);
    }
  }

  cancel() {
    this.dialogRef.close(undefined);
  }

  onDictionaryChange() {
    this.selectedDictionary = this.dictionaries.find(d => d.id === this.form.get('dictionaryId')?.value) ?? null;
    if (!this.selectedDictionary) this.form.patchValue({ defaultValue: '' });
    else {
      this.dictionariesService.getDictionaryElements(this.selectedDictionary.id).then(elements => {
        this.selectedDictionary!.dictionariesElements = elements ?? [];
        const firstValue = this.selectedDictionary!.dictionariesElements.find(x => x.isDefault);
        if (firstValue) {
          this.form.patchValue({ defaultValue: firstValue.value });
        }
      });
    }
  }
}