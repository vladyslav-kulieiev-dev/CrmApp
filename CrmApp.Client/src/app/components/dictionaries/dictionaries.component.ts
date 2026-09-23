import { Component, OnInit, ViewChild } from '@angular/core';
import { Location } from '@angular/common';
import { MatPaginator } from '@angular/material/paginator';
import { MatSort } from '@angular/material/sort';
import { MatTableDataSource } from '@angular/material/table';
import { Router } from '@angular/router';
import { Observable } from 'rxjs';
import { AuthService } from 'src/app/core/services/auth.service';
import { DeviceService } from 'src/app/core/services/device.service';
import { DictionariesService } from 'src/app/core/services/dictionaries.service';
import { NotificationService } from 'src/app/core/services/notification.service';
import { Dictionaries } from 'src/models/Dictionaries';
import { DictionariesElements } from 'src/models/DictionariesElements';
import { CdkDragDrop, moveItemInArray } from '@angular/cdk/drag-drop';
import { MatIconRegistry } from '@angular/material/icon';

@Component({
  selector: 'crm-dictionaries',
  standalone: false,
  templateUrl: './dictionaries.component.html',
  styleUrl: './dictionaries.component.css'
})
export class DictionariesComponent implements OnInit {
  displayedColumns: string[] = ['name', 'description', 'createdAt', 'createdBy', 'isCustom', 'isActive', 'actions'];
  elementsDisplayedColumns: string[] = ['drag', 'key', 'value', 'icon', 'isActive', 'isDefault', 'actions'];
  dataSource: MatTableDataSource<Dictionaries> = new MatTableDataSource<Dictionaries>();
  filterValue?: string;
  dictionaries: Dictionaries[] = [];
  selectedDictionary: Dictionaries = new Dictionaries();
  userId: number;
  isEditPanelOpened: boolean = false;
  showAlternativeValues: boolean = false;
  readonly canSystemConfig: boolean;
  readonly isMobile$: Observable<boolean>;

  @ViewChild(MatPaginator) paginator!: MatPaginator;
  @ViewChild(MatSort) sort!: MatSort;

  constructor(
    private auth: AuthService,
    private device: DeviceService,
    private router: Router,
    private dictionariesService: DictionariesService,
    private notificationService: NotificationService,
    private location: Location
  ) {
    this.canSystemConfig = this.auth.userConfiguration?.canSystemConfig ?? false;
    this.userId = this.auth.user?.id ?? 0;
    this.isMobile$ = this.device.isMobile$;
  }

  ngOnInit(): void {
    this.getDictionaries();
  }

  getDictionaries() {
    this.dictionariesService.getAll().then(dictionaries => {
      this.dictionaries = dictionaries ?? [];
      this.setMatTableConfig();
    }).catch(error => {
      console.error('Błąd podczas pobierania słowników:', error);
    });
  }

  applyFilter(e: Event) {
    const filterValue = (e.target as HTMLInputElement).value;
    if (this.dataSource)
      this.dataSource!.filter = filterValue.trim().toLocaleLowerCase();

    if (this.dataSource && this.dataSource.paginator)
      this.dataSource.paginator.firstPage();
  }

  resetFilter() {
    this.filterValue = "";
    if (this.dataSource)
      this.dataSource!.filter = this.filterValue;

    if (this.dataSource && this.dataSource.paginator)
      this.dataSource.paginator.firstPage();
  }

  setMatTableConfig() {
    this.dataSource.data = this.dictionaries;
    this.dataSource.paginator = this.paginator;
    this.dataSource.sort = this.sort;
  }

  addDictionary(): void {
    this.selectedDictionary = new Dictionaries();
    this.isEditPanelOpened = true;
  }

  getDictionaryElements() {
    if (this.selectedDictionary && this.selectedDictionary.id) {
      this.dictionariesService.getDictionaryElements(this.selectedDictionary.id).then((elements) => {
        this.selectedDictionary.dictionariesElements = elements ?? [];
        this.showAlternativeValues = this.selectedDictionary.dictionariesElements
          .some(e => e.alternativeValuesForMapping && e.alternativeValuesForMapping.length);
        this.updateElementsVisibleColumns();
      }).catch((error) => {
        console.error('Błąd podczas pobierania elementów słownika:', error);
      });
    }
  }

  deactivateDictionary(dictionary: Dictionaries): void {
    console.log('Dezaktywuj słownik', dictionary); // TO DO
  }

  activateDictionary(dictionary: Dictionaries): void {
    console.log('Aktywuj słownik', dictionary); // TO DO
  }

  editDictionary(dictionary: Dictionaries): void {
    this.selectedDictionary = { ...dictionary };
    this.getDictionaryElements();
    this.isEditPanelOpened = true;
  }

  closeEditPanel() {
    this.isEditPanelOpened = false;
    this.selectedDictionary = new Dictionaries();
  }

  addDictionaryElement() {
    this.selectedDictionary.dictionariesElements = [...this.selectedDictionary.dictionariesElements, {
      id: 0,
      key: '',
      value: '',
      isActive: true,
      createdAt: new Date(),
      createdBy: this.userId,
      dictionaryId: this.selectedDictionary.id
    } as DictionariesElements];
  }

  removeDictionaryElement(index: number) {
    this.selectedDictionary.dictionariesElements = this.selectedDictionary.dictionariesElements.filter((_, i) => i !== index);
  }

  onIsDefaultChange(element: DictionariesElements) {
    this.selectedDictionary.dictionariesElements.forEach(e => {
      if (e !== element) {
        e.isDefault = false;
      }
    });
  }

  saveChanges() {
    if (this.selectedDictionary.name === "") {
      this.notificationService.error("Pole 'Nazwa' nie może być puste");
      return;
    }
    this.selectedDictionary.dictionariesElements = this.selectedDictionary.dictionariesElements.filter(e => e.key?.trim());
    if (this.selectedDictionary.id) {
      this.dictionariesService.update(this.selectedDictionary, this.userId).then((res) => {
        if (res && res.succeeded) {
          this.notificationService.success("Zaktualizowano słownik: " + this.selectedDictionary.name);
          this.isEditPanelOpened = false;
          this.getDictionaries();
        }
        else {
          this.notificationService.error("Nie zaktualizowano słownika: " + this.selectedDictionary.name);
        }
      }, (err) => {
        this.notificationService.error("Nie zaktualizowano słownika: " + this.selectedDictionary.name);
      })
    } else {
      this.dictionariesService.add(this.selectedDictionary, this.userId).then((res) => {
        if (res && res.succeeded) {
          this.notificationService.success("Dodano słownik: " + this.selectedDictionary.name);
          this.isEditPanelOpened = false;
          this.getDictionaries();
        } else {
          this.notificationService.error("Nie dodano słownika: " + this.selectedDictionary.name + ". Błąd: " + res?.errors);
        }
      }, (err) => {
        this.notificationService.error("Nie dodano słownika: " + this.selectedDictionary.name + ". Błąd: " + err);
      })
    }
  }

  onDrop(event: CdkDragDrop<any[]>): void {
    moveItemInArray(
      this.selectedDictionary.dictionariesElements,
      event.previousIndex,
      event.currentIndex
    );
    this.selectedDictionary.dictionariesElements.forEach((el, index) => {
      el.ordinalNumber = index + 1;
    });
    this.selectedDictionary.dictionariesElements = [...this.selectedDictionary.dictionariesElements];
  }

  updateElementsVisibleColumns() {
    this.elementsDisplayedColumns = this.showAlternativeValues ? 
      ['drag', 'key', 'value', 'alternativeValuesForMapping', 'icon', 'isActive', 'isDefault', 'actions'] : 
      ['drag', 'key', 'value', 'icon', 'isActive', 'isDefault', 'actions'];
  }
}
