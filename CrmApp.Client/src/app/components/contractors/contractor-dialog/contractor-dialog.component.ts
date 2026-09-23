import { Component, inject, OnInit } from '@angular/core';
import { MatDialogRef, MAT_DIALOG_DATA } from '@angular/material/dialog';
import { Observable } from 'rxjs';
import { AuthService } from 'src/app/core/services/auth.service';
import { ContractorsService } from 'src/app/core/services/contractors.service';
import { DeviceService } from 'src/app/core/services/device.service';
import { NotificationService } from 'src/app/core/services/notification.service';
import { Contractors } from 'src/models/Contractors';
import { ResultDTO } from 'src/models/DTO/ResultDTO';

@Component({
  selector: 'crm-contractor-dialog',
  standalone: false,
  templateUrl: './contractor-dialog.component.html',
  styleUrl: './contractor-dialog.component.css'
})
export class ContractorDialogComponent implements OnInit {
  contractor: Contractors = new Contractors();
  originalDisplayName?: string;
  altNip?: string;
  readonly canContractorsCreate: boolean;
  readonly canContractorsUpdate: boolean;
  readonly isMobile$: Observable<boolean>;
  readonly dialogRef = inject(MatDialogRef<ContractorDialogComponent>);
  data = inject(MAT_DIALOG_DATA);

  constructor(
    private auth: AuthService,
    private contractorsService: ContractorsService,
    private deviceService: DeviceService,
    private notificationService: NotificationService
  ) {
    this.canContractorsCreate = this.auth.userConfiguration?.canContractorsCreate ?? false;
    this.canContractorsUpdate = this.auth.userConfiguration?.canContractorsUpdate ?? false;
    this.isMobile$ = this.deviceService.isMobile$;
  }

  ngOnInit() {
    if (this.data.contractor) {
      this.contractor = this.data.contractor;
      if (this.contractor.id) this.originalDisplayName = this.contractor.displayName;
    }
  }

  onContractorNameChange() {
    if (this.contractor?.code && !this.contractor.displayName)
      this.contractor.displayName = this.contractor.code;
  }

  addAlternativeNip() {
    if (!this.altNip) {
      return;
    }
    if (!this.contractor?.alternativeNipNumbers) {
      this.contractor!.alternativeNipNumbers = [];
    }
    if (!this.contractor?.alternativeNipNumbers?.includes(this.altNip) && this.contractor?.nip != this.altNip) {
      this.contractor?.alternativeNipNumbers?.push(this.altNip);
      this.altNip = "";
    } else if (this.contractor?.alternativeNipNumbers?.includes(this.altNip)) {
      this.notificationService.warning("Numer Nip został już dodany jako alternatywny");
    } else if (this.contractor?.nip == this.altNip) {
      this.notificationService.warning("Numer Nip został ustawiony jako główny Nip kontrahenta");
    }
  }

  removeNip(idx: number) {
    this.contractor!.alternativeNipNumbers?.splice(idx, 1);
  }

  close(contractorSaved?: Contractors) {
    this.dialogRef.close({ 
      succeeded: contractorSaved != null && contractorSaved != undefined, 
      data: contractorSaved 
    } as ResultDTO<Contractors>)
  }

  saveChanges() {
    if (!this.contractor?.code || !this.contractor.name || !this.contractor.nip) {
      this.notificationService.error("Zdefiniuj kod, nazwę i NIP kontrahenta");
      return;
    }

    if (this.altNip && this.altNip.length && !this.contractor.alternativeNipNumbers?.includes(this.altNip)){
      this.addAlternativeNip();
    }

    if (this.contractor && this.contractor.id) {
      this.contractorsService.update(this.contractor).then((result) => {
        if (result && result.succeeded) {
          this.notificationService.success("Zaktualizowano kontrahenta " + this.contractor?.displayName);
          this.close(this.contractor);
        } else {
          this.notificationService.error(result?.errors ? result.errors.join("\n") :
            "Nie zaktualizowano kontrahenta " + this.originalDisplayName);
        }
      }, (err) => {
        this.notificationService.error("Nie zaktualizowano kontrahenta " + this.originalDisplayName);
      });
    } else if (this.contractor) {
      this.contractorsService.add(this.contractor).then(async (result) => {
        if (result && result.succeeded) {
          if (result.data) this.contractor = result.data;
          this.notificationService.success("Dodano kontrahenta " + this.contractor?.displayName);
          this.close(this.contractor);
        } else {
          this.notificationService.error(result?.errors ? result.errors.join("\n") :
            "Nie dodano kontrahenta " + this.contractor?.displayName);
        }
      }, (err) => {
        this.notificationService.error("Nie dodano kontrahenta " + this.contractor?.displayName);
      });
    }
  }
}
