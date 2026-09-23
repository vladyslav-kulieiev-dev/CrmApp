import { Component, ContentChild, EventEmitter, Input, OnInit, Output } from '@angular/core';
import { Router } from '@angular/router';
import { AuthService } from 'src/app/core/services/auth.service';
import { Location } from '@angular/common';
import { Observable } from 'rxjs';
import { DeviceService } from 'src/app/core/services/device.service';
import { CrmTemplateDirective } from 'src/app/core/crm-template.directive';

@Component({
  selector: 'crm-header',
  templateUrl: './header.component.html',
  styleUrls: ['./header.component.css'],
  standalone: false
})
export class HeaderComponent implements OnInit {
  @Input() headerSubpageTitle?: string;
  @Input() showReturnButton: boolean = false;
  @Input() customReturnBehaviour: boolean = false;
  @Input() focusLabel?: string;
  @Input() focusIcon?: string;

  @Output() onReturnClick: EventEmitter<boolean> = new EventEmitter<boolean>();

  isAdmin: boolean = true;
  loggedUserDisplayName: string = "";
  readonly canUsersView: boolean = false;
  readonly canContractorsView: boolean = false;
  readonly canSystemConfig: boolean = false;
  readonly canProjectsView: boolean = false;
  userId: number = 0;
  readonly isMobile$: Observable<boolean>;

  @ContentChild(CrmTemplateDirective) template?: CrmTemplateDirective;

  constructor(
    private authService: AuthService,
    private router: Router,
    private location: Location,
    private device: DeviceService
  ) {
    this.canUsersView = (this.authService.userConfiguration?.canUsersView ?? false);
    this.canContractorsView = (this.authService.userConfiguration?.canContractorsView ?? false);
    this.canSystemConfig = (this.authService.userConfiguration?.canSystemConfig ?? false);
    this.canProjectsView = (this.authService.userConfiguration?.canProjectsView ?? false);
    this.isMobile$ = this.device.isMobile$;
    this.authService.loggedIn$.subscribe(loggedIn => {
      this.isAdmin = loggedIn && this.authService.isAdmin();
    });

    this.loggedUserDisplayName = this.authService.user?.displayName ?? "";
  }

  ngOnInit(): void {
    this.userId = this.authService.user?.id!;
  }

  async logout() {
    await this.authService.logout();
  }

  navigateToView(route: string) {
    this.router.navigate(['/' + route]);
  }

  navigateHome() {
    this.router.navigate(['/']);
  }

  navigateToUserProfile() {
    this.router.navigate(['/my-account', this.userId]);
  }

  goBack() {
    this.onReturnClick.emit(true);
    if (!this.customReturnBehaviour) this.location.back();
  }
}
