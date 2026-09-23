import { Component, EventEmitter, Input, OnChanges, Output, SimpleChanges } from '@angular/core';
import { Router } from '@angular/router';
import { AuthService } from 'src/app/core/services/auth.service';
import { ContractorsService } from 'src/app/core/services/contractors.service';
import { ProjectsService } from 'src/app/core/services/projects.service';
import { getJoinedMesseges } from 'src/app/core/services/extensions.service';
import { NotificationService } from 'src/app/core/services/notification.service';
import { Contractors } from 'src/models/Contractors';
import { ContractorContacts } from 'src/models/ContractorContacts';
import { ContractorLicenses } from 'src/models/ContractorLicenses';
import { ContractorContracts } from 'src/models/ContractorContracts';
import { Projects } from 'src/models/Projects';
import { EEngagementType } from 'src/models/enums/EEngagementType';

@Component({
  selector: 'crm-contractor-summary',
  standalone: false,
  templateUrl: './contractor-summary.component.html',
  styleUrl: './contractor-summary.component.css'
})
export class ContractorSummaryComponent implements OnChanges {
  @Input() contractorId!: number;
  @Output() closed = new EventEmitter<void>();

  private userId = 0;
  loading = false;
  contractor: Contractors | null = null;
  contacts: ContractorContacts[] = [];
  licenses: ContractorLicenses[] = [];
  contracts: ContractorContracts[] = [];
  projects: Projects[] = [];
  EEngagementType = EEngagementType;

  constructor(
    private auth: AuthService,
    private contractorsService: ContractorsService,
    private projectsService: ProjectsService,
    private notificationService: NotificationService,
    private router: Router
  ) { this.userId = this.auth.user?.id ?? 0; }

  ngOnChanges(ch: SimpleChanges) {
    if (ch['contractorId'] && this.contractorId) this.load();
  }

  private load() {
    const id = this.contractorId;
    this.loading = true;
    this.contractor = null;
    this.contacts = []; this.licenses = []; this.contracts = []; this.projects = [];

    this.contractorsService.getByIdLight(id).then(res => {
      if (id !== this.contractorId) return;
      this.loading = false;
      if (res?.succeeded && res.data) this.contractor = res.data;
      else this.notificationService.error(getJoinedMesseges('Nie znaleziono kontrahenta', res?.errors));
    });

    this.contractorsService.getContractorContacts(id).then(r => { if (id === this.contractorId) this.contacts = r ?? []; });
    this.contractorsService.getContractorLicenses(id).then(r => { if (id === this.contractorId) this.licenses = r ?? []; });
    this.contractorsService.getContractorContracts(id).then(r => { if (id === this.contractorId) this.contracts = r?.items ?? []; });
    this.projectsService.getByContractorId(id).then(r => { if (id === this.contractorId) this.projects = r ?? []; });
  }

  get initials(): string {
    const n = this.contractor?.displayName || this.contractor?.name || '';
    const p = n.trim().split(/\s+/);
    return (p.length >= 2 ? p[0][0] + p[1][0] : n.slice(0, 2)).toUpperCase();
  }

  contractIcon(t: EEngagementType): string {
    switch (t) {
      case EEngagementType.HoursPackage: return 'timer';
      case EEngagementType.MonthlyBilling: return 'calendar_month';
      case EEngagementType.Contract: return 'description';
      default: return 'handshake';
    }
  }

  hoursPercent(c: ContractorContracts): number {
    if (!c.hoursLimit) return 0;
    return Math.min(100, Math.round(((c.hoursUsed ?? 0) / c.hoursLimit) * 100));
  }

  openFullCard() {
     const url = this.router.serializeUrl(
      this.router.createUrlTree(['/contractor-pane', this.contractorId])
    );
    window.open(url, '_blank');
  }

  close() { this.closed.emit(); }
}