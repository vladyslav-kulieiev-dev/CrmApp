import { Location } from '@angular/common';
import { Component, OnDestroy, OnInit } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { ActivatedRoute } from '@angular/router';
import { distinctUntilChanged, firstValueFrom, map, Observable, Subject, takeUntil, tap } from 'rxjs';
import { AuthService } from 'src/app/core/services/auth.service';
import { CatalogItemsService } from 'src/app/core/services/catalog-items.service';
import { DeviceService } from 'src/app/core/services/device.service';
import { DictionariesService } from 'src/app/core/services/dictionaries.service';
import { getJoinedMesseges } from 'src/app/core/services/extensions.service';
import { NotificationService } from 'src/app/core/services/notification.service';
import { UsersService } from 'src/app/core/services/users.service';
import { CatalogItems } from 'src/models/CatalogItems';
import { DictionariesElements } from 'src/models/DictionariesElements';
import { UsersDTO } from 'src/models/DTO/UsersDTO';
import { ValueNameDTO } from 'src/models/DTO/ValueNameDTO';
import { EDictionaryType } from 'src/models/enums/EDictionaryType';
import { UsersProfiles } from 'src/models/UsersProfiles';

@Component({
  selector: 'crm-catalog-item',
  standalone: false,
  templateUrl: './catalog-item.component.html',
  styleUrl: './catalog-item.component.css'
})
export class CatalogItemComponent implements OnInit, OnDestroy {
  private destroy$ = new Subject<void>();
  id: number = 0;
  catalogItem: CatalogItems = new CatalogItems();
  catalogItems: CatalogItems[] = [];
  filteredCatalogItems: CatalogItems[] = [];
  headerLabel: string = '';
  productTypes: DictionariesElements[] = [];
  productCategories: DictionariesElements[] = [];
  billingUnits: DictionariesElements[] = [];
  units: DictionariesElements[] = [];
  currencies: DictionariesElements[] = [];
  vatRates: ValueNameDTO[] = [];
  allUsers: UsersDTO[] = [];
  enableParentSelection: boolean = false;
  form!: FormGroup;
  private userId: number = 0;

  readonly isMobile$: Observable<boolean>;

  constructor(
    private fb: FormBuilder,
    private activatedRoute: ActivatedRoute,
    private catalogItemsService: CatalogItemsService,
    private notificationsService: NotificationService,
    private location: Location,
    private device: DeviceService,
    private dictionariesService: DictionariesService, 
    private authService: AuthService,
    private usersService: UsersService
  ) {
    this.isMobile$ = this.device.isMobile$;

    this.form = this.fb.group({
      isActive: [true],
      name: ['', [Validators.required]],
      code: ['', [Validators.required]],
      description: [''],
      type: [0, [Validators.required]],
      unitId: [0, [Validators.required]],
      billingUnitId: [0, [Validators.required]],
      price: [0, [Validators.required, Validators.min(0)]],
      vatRate: [0, [Validators.required]],
      currency: ['', [Validators.required]],
      categoryId: [0, [Validators.required]],
      technicalSupervisorId: [null],
      implementationManagerId: [null],
      supportedSystems: [[]],
      parentItemId: [null]
    });

    this.userId = authService.user?.id!;

    this.device.isMobile$.subscribe((isMobile) => {
      if (!isMobile) this.headerLabel = this.id ? '' : 'Nowy produkt';
    });
  }

  ngOnInit(): void {
    this.detectIdFromRoute();
    this.getCatalogItems();
    this.getSystemDictionaries();
    this.getAllUsers();
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }

  get name() { return this.form.get('name'); }
  get code() { return this.form.get('code'); }
  get description() { return this.form.get('description'); }
  get categoryId() { return this.form.get('categoryId'); }
  get type() { return this.form.get('type'); }
  get unitId() { return this.form.get('unitId'); }
  get billingUnitId() { return this.form.get('billingUnitId'); }
  get price() { return this.form.get('price'); }
  get vatRate() { return this.form.get('vatRate'); }
  get currency() { return this.form.get('currency'); }
  get parentItemId() { return this.form.get('parentItemId'); }
  get isActive() { return this.form.get('isActive'); }
  get technicalSupervisorId() { return this.form.get('technicalSupervisorId'); }
  get implementationManagerId() { return this.form.get('implementationManagerId'); }
  get supportedSystems() {return this.form.get('supportedSystems')}

  detectIdFromRoute() {
    this.activatedRoute.paramMap.pipe(
      map(params => {
        const v = params.get('id');
        return v === null ? null : Number(v);
      }),
      distinctUntilChanged(),
      tap(id => {
        this.id = id ?? 0;
        if (this.id) {
          this.getCatalogItemById();
        } else {
          this.catalogItem = new CatalogItems();
          this.headerLabel = 'Nowy produkt';
          this.mapItemToForm();
        }
      }),
      takeUntil(this.destroy$)
    ).subscribe();
  }

  async getCatalogItems() {
    this.catalogItemsService.getList(false).then((result) => {
      this.catalogItems = result ?? [];
      if (this.id) this.catalogItems = this.catalogItems.filter(x => x.id != this.id);
    });
  }

  async getAllUsers(){
    this.usersService.getAll().then((result) => {
      this.allUsers = (result ?? []).sort((a, b) => a.displayName.localeCompare(b.displayName));
    });
  }

  filterCatalogItems() {
    var parentElementId = this.productTypes.find(x => x.id == this.type?.value)?.parentId;
    this.enableParentSelection = (parentElementId && parentElementId > 0) ? true : false;
    if (this.enableParentSelection) {
      this.filteredCatalogItems = this.catalogItems.filter(x => x.type == parentElementId);
      if (this.parentItemId?.value && !this.filteredCatalogItems.findIndex(x => x.id ==  this.parentItemId?.value)) {
        this.parentItemId?.setValue(null);
      }
    } else { 
      this.filteredCatalogItems = this.catalogItems; 
    }

    this.filteredCatalogItems = this.filteredCatalogItems.filter(x => x.id != this.id);
  }

  getSystemDictionaries() {
    this.dictionariesService.getSystemDictionaries().then((res) => {
      if (res && res.length) {
        const prodTypeDict = res.find(x => x.isActive && x.dictionaryType == EDictionaryType.ProductTypes);
        const prodCatDict = res.find(x => x.isActive && x.dictionaryType == EDictionaryType.ProductCategories);
        const billUnitsDict = res.find(x => x.isActive && x.dictionaryType == EDictionaryType.BillingUnits);
        const unitsDict = res.find(x => x.isActive && x.dictionaryType == EDictionaryType.UnitsOfMeasure);
        const currDict = res.find(x => x.isActive && x.dictionaryType == EDictionaryType.Currencies);
        const vatDict = res.find(x => x.isActive && x.dictionaryType == EDictionaryType.VatRates);
      
        if (prodTypeDict) this.productTypes = prodTypeDict.dictionariesElements;
        if (prodCatDict) this.productCategories = prodCatDict.dictionariesElements;
        if (billUnitsDict) this.billingUnits = billUnitsDict.dictionariesElements;
        if (unitsDict) this.units = unitsDict.dictionariesElements;
        if (currDict) this.currencies = currDict.dictionariesElements;
        if (vatDict) this.vatRates = vatDict.dictionariesElements.map(x => new ValueNameDTO(Number(x.value), x.key, "", "", "", x.isDefault));

        if (!this.id) {
          this.catalogItem.type = this.productTypes.find(x => x.isDefault)?.id ?? 0;
          this.catalogItem.categoryId = this.productCategories.find(x => x.isDefault)?.id ?? 0;
          this.catalogItem.vatRate = this.vatRates.find(x => x.isDefault)?.value ?? 0;
          this.catalogItem.currency = this.currencies.find(x => x.isDefault)?.value ?? "";
          this.catalogItem.unitId = this.units.find(x => x.isDefault)?.id ?? 0;
          this.catalogItem.billingUnitId = this.billingUnits.find(x => x.isDefault)?.id ?? 0;
          this.mapItemToForm();
        }
      } else {
        this.notificationsService.error("Nie pobrano słowników systemowych");
      }
    }, (err) => {
      this.notificationsService.error("Nie pobrano słowników systemowych");
    });
  }

  async getCatalogItemById() {
    const isMobile = await firstValueFrom(this.device.isMobile$);
    this.catalogItemsService.getById(this.id).then((result) => {
      if (result) {
        this.catalogItem = result;
        this.headerLabel = isMobile ? "" : `Produkt ${this.catalogItem.name} (${this.catalogItem.code})`;
        this.mapItemToForm();
      } else {
        this.notificationsService.error("Nie znaleziono produktu");
      }
    }, (err) => {
      this.notificationsService.error("Nie znaleziono produktu");
    })
  }

  mapItemToForm() {
    this.name?.setValue(this.catalogItem.name);
    this.code?.setValue(this.catalogItem.code);
    this.description?.setValue(this.catalogItem.description ?? '');
    this.categoryId?.setValue(this.catalogItem.categoryId ?? 0);
    this.type?.setValue(this.catalogItem.type ?? 0);
    this.unitId?.setValue(this.catalogItem.unitId ?? 0);
    this.billingUnitId?.setValue(this.catalogItem.billingUnitId ?? 0);
    this.price?.setValue(this.catalogItem.price ?? 0);
    this.vatRate?.setValue(this.catalogItem.vatRate ?? 0);
    this.currency?.setValue(this.catalogItem.currency ?? '');
    this.parentItemId?.setValue(this.catalogItem.parentItemId ?? null);
    this.isActive?.setValue(this.catalogItem.isActive ?? true);
    this.technicalSupervisorId?.setValue(this.catalogItem.technicalSupervisorId ?? null);
    this.implementationManagerId?.setValue(this.catalogItem.implementationManagerId ?? null);
    this.supportedSystems?.setValue(this.catalogItem.supportedSystems ?? []);
  }

  mapFormToItem() {
    this.catalogItem.name = this.name?.value;
    this.catalogItem.code = this.code?.value;
    this.catalogItem.description = this.description?.value;
    this.catalogItem.categoryId = Number(this.categoryId?.value ?? 0);
    this.catalogItem.type = Number(this.type?.value ?? 0);
    this.catalogItem.unitId = Number(this.unitId?.value ?? 0);
    this.catalogItem.billingUnitId = Number(this.billingUnitId?.value ?? 0);
    this.catalogItem.price = Number(this.price?.value ?? 0);
    this.catalogItem.vatRate = Number(this.vatRate?.value ?? 0);
    this.catalogItem.currency = this.currency?.value;
    this.catalogItem.parentItemId = this.parentItemId?.value;
    this.catalogItem.isActive = !!this.isActive?.value;
    this.catalogItem.technicalSupervisorId = this.technicalSupervisorId?.value;
    this.catalogItem.implementationManagerId = this.implementationManagerId?.value;
    this.catalogItem.supportedSystems = this.supportedSystems?.value;

    if (this.catalogItem.unitId) this.catalogItem.unitName = this.units.find(x => x.id == this.catalogItem.unitId)?.value ?? "";
    if (this.catalogItem.billingUnitId) this.catalogItem.billingUnitName = this.billingUnits.find(x => x.id == this.catalogItem.billingUnitId)?.value ?? "";
  }

  goBack() {
    this.location.back();
  }

  submit() {
    this.form.markAllAsTouched();
    this.form.updateValueAndValidity();
    if (this.form.invalid) return;

    if (!this.id) this.addCatalogItem();
    else this.updateCatalogItem();
  }

  addCatalogItem() {
    this.mapFormToItem();
    this.catalogItem.createdBy = this.userId;
    this.catalogItemsService.add(this.catalogItem).then((res) => {
      if (res && res.succeeded) {
        this.notificationsService.success("Utworzono produkt " + this.catalogItem.name);
        this.location.back();
      } else {
        this.notificationsService.error(getJoinedMesseges("Nie utworzono produktu " + this.catalogItem.name, res?.errors));
      }
    }, (err) => {
      this.notificationsService.error("Nie utworznono produktu " + this.catalogItem.name);
    });
  }

  updateCatalogItem() {
    this.mapFormToItem();
    this.catalogItemsService.update(this.catalogItem).then((res) => {
      if (res && res.succeeded) {
        this.notificationsService.success("Zaktualizowano produkt " + this.catalogItem.name);
        this.location.back();
      } else {
        this.notificationsService.error(getJoinedMesseges("Nie zaktualizowano produktu " + this.catalogItem.name, res?.errors));
      }
    }, (err) => {
      this.notificationsService.error("Nie zaktualizowano produktu " + this.catalogItem.name);
    });
  }
}
