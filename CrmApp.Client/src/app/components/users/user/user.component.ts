import { Location } from '@angular/common';
import { Component, OnInit } from '@angular/core';
import { AbstractControl, FormBuilder, FormGroup, Validators } from '@angular/forms';
import { ActivatedRoute } from '@angular/router';
import { distinctUntilChanged, firstValueFrom, map, Observable, Subject, takeUntil, tap } from 'rxjs';
import { AuthService } from 'src/app/core/services/auth.service';
import { DeviceService } from 'src/app/core/services/device.service';
import { flatten } from 'src/app/core/services/extensions.service';
import { NotificationService } from 'src/app/core/services/notification.service';
import { UsersService } from 'src/app/core/services/users.service';
import { RegisterDTO } from 'src/models/DTO/RegisterDTO';
import { ResetPasswordDTO } from 'src/models/DTO/ResetPasswordDTO';
import { RolesClaimsDTO } from 'src/models/DTO/RolesDTO';
import { UsersDTO } from 'src/models/DTO/UsersDTO';

function match(control: AbstractControl) {
  const p = control.get('password')?.value;
  const c = control.get('confirmPassword')?.value;
  return p && c && p === c ? null : { mismatch: true };
}
@Component({
  selector: 'crm-user',
  templateUrl: './user.component.html',
  styleUrl: './user.component.css',
  standalone: false
})
export class UserComponent implements OnInit {
  id: number = 0;
  headerLabel: string = "";
  user: UsersDTO = new UsersDTO();
  register: RegisterDTO = new RegisterDTO();
  hide = true;
  hideOldPass = true;
  form!: FormGroup;
  selectedView: string = "user-form";
  roles: RolesClaimsDTO[] = [];
  claims: RolesClaimsDTO[] = [];
  views: any[];
  loggedUserId: number = 0;
  selectedViewName: string;
  isEditMode: boolean = false;
  calendarProviders: string[] = ["Zoho"];
  readonly canUsersUpdate: boolean;
  readonly canUsersCreate: boolean;
  readonly canUsersDelete: boolean;
  readonly canUserClaimsView: boolean;
  readonly canUserClaimsManage: boolean;
  readonly canUserPasswordUpdate: boolean;
  readonly isMobile$: Observable<boolean>;
  private destroy$ = new Subject<void>();

  constructor(
    private fb: FormBuilder,
    private activatedRoute: ActivatedRoute,
    private usersService: UsersService,
    private notificationsService: NotificationService,
    private location: Location,
    private authService: AuthService,
    private device: DeviceService,
    private notificationService: NotificationService
  ) {
    this.form = this.fb.group({
      email: ['', [Validators.required, Validators.email]],
      altEmail: ['', [Validators.email]],
      calendarProvider: [''],
      calendarId: [''],
      firstName: ['', [Validators.required]],
      lastName: ['', [Validators.required]],
      displayName: [''],
      phoneNumber: [''],
      password: ['', [Validators.minLength(6)]],
      oldPassword: ['', [Validators.minLength(6)]],
      confirmPassword: ['', [Validators.minLength(6)]]
    }, { validators: match });


    this.loggedUserId = this.authService.user?.id ?? 0;
    this.canUsersCreate = this.authService.userConfiguration?.canUsersCreate ?? false;
    this.canUsersUpdate = this.authService.userConfiguration?.canUsersUpdate ?? false;
    this.canUsersDelete = this.authService.userConfiguration?.canUsersDelete ?? false;
    this.canUserClaimsView = this.authService.userConfiguration?.canUserClaimsView ?? false;
    this.canUserClaimsManage = this.authService.userConfiguration?.canUserClaimsManage ?? false;
    this.canUserPasswordUpdate = (this.authService.userConfiguration?.canUserPasswordUpdate ?? false) || (this.loggedUserId == this.id);
    this.isMobile$ = this.device.isMobile$;
    this.device.isMobile$.subscribe((isMobile) => {
      if (!isMobile) this.headerLabel = this.id ? "" : "Nowy użytkownik";
    });
    this.views = [
      { value: "user-form", name: "Właściwości " }
    ];
    if (this.canUserClaimsView) {
      this.views.push({ value: "permissions", name: "Role i uprawnienia " });
    }
    if (this.canUserPasswordUpdate) {
      this.views.push({ value: "password-change", name: "Zmiana hasła " });
    }
    this.selectedViewName = this.views[0].name;

    if (this.id) {
      this.form.get('email')?.disable();
      this.isEditMode = true;
    }
  }

  ngOnInit(): void {
    this.detectUserIdFromRoute();
    this.getRoles();
  }

  get email() { return this.form.get('email'); }
  get altEmail() { return this.form.get('altEmail'); }
  get calendarProvider() { return this.form.get('calendarProvider'); }
  get calendarId() { return this.form.get('calendarId') }
  get firstName() { return this.form.get('firstName'); }
  get lastName() { return this.form.get('lastName'); }
  get displayName() { return this.form.get('displayName'); }
  get phoneNumber() { return this.form.get('phoneNumber'); }
  get password() { return this.form.get('password'); }
  get confirmPassword() { return this.form.get('confirmPassword'); }
  get oldPassword() { return this.form.get('oldPassword'); }

  childrenAccessor = (node: RolesClaimsDTO) => node.children ?? [];
  hasChild = (_: number, node: RolesClaimsDTO) => !!node.children && node.children.length > 0;

  selectedViewChange() {
    this.selectedViewName = this.views.find(x => x.value == this.selectedView)?.name ?? "";
  }

  checkRole(role: string, selected: boolean) {
    if (selected && !this.user.userConfiguration?.roles.includes(role)) {
      this.user.userConfiguration?.roles.push(role);
      var roleDTO = this.roles.find(x => x.name == role);
      for (var claim of (roleDTO?.children ?? [])) {
        if (!this.isClaimSelected(claim.key))
          this.checkClaim(claim, true);
      }
    }
    if (!selected && this.user.userConfiguration?.roles.includes(role)) {
      this.user.userConfiguration!.roles = this.user.userConfiguration?.roles.filter(x => x !== role);
      var roleDTO = this.roles.find(x => x.name == role);
      for (var claim of (roleDTO?.children ?? [])) {
        if (this.isClaimSelected(claim.key) && !this.isClaimInSelectedRoles(claim, role))
          this.checkClaim(claim, false);
      }
    }
  }

  isClaimInSelectedRoles(claim: RolesClaimsDTO, role: string) {
    var rolesWithClaim = this.roles.filter(x => this.user.userConfiguration?.roles.includes(x.name) && x.name !== role)
      .flatMap(x => x.children).filter(x => x.key == claim.key);
    return rolesWithClaim && rolesWithClaim.length > 0;
  }

  checkClaim(claim: RolesClaimsDTO, selected: boolean) {
    if (selected && !this.user.userConfiguration?.claims.some(x => x == claim.key))
      this.user.userConfiguration?.claims.push(claim.key);
    if (!selected && this.user.userConfiguration?.claims.some(x => x == claim.key))
      this.user.userConfiguration!.claims = this.user.userConfiguration?.claims.filter(x => x !== claim.key);
  }

  isSelected(node: RolesClaimsDTO) {
    return (node.isRole && this.isRoleSelected(node.name)) || (node.isClaim && this.isClaimSelected(node.name));
  }

  isRoleSelected(role: string) {
    return this.user.userConfiguration?.roles.includes(role);
  }

  isClaimSelected(claim: string) {
    return this.user.userConfiguration?.claims.some(x => x == claim);
  }

  isInSelectedRoles(claim: RolesClaimsDTO) {
    return this.roles.filter(x => this.user.userConfiguration?.roles.includes(x.name))
      .flatMap(x => x.children).some(x => x.key == claim.key) && this.isClaimSelected(claim.key);
  }

  getRoles() {
    this.authService.getRoles().toPromise().then((result) => {
      this.roles = result ?? [];
      this.mapClaimsList();
    });
  }

  mapClaimsList() {
    this.claims = [];
    for (var role of this.roles) {
      for (var claim of role.children) {
        if (!this.claims.some(x => x.key == claim.key))
          this.claims.push(claim);
      }
    }
  }

  async getUserById() {
    const isMobile = await firstValueFrom(this.device.isMobile$);
    if (!this.id) return;

    this.usersService.getById(this.id).then((result) => {
      if (result?.succeeded && result.data) {
        this.user = result.data;
        this.headerLabel = isMobile ? "" : "Profil użytkownika " + this.user.displayName;
        this.mapUserParams();
      } else {
        this.notificationsService.error(result?.errors ? result.errors.join("\n") : "Nie znaleziono profilu użytkownika");
      }
    }, (err) => {
      this.notificationsService.error("Nie znaleziono profilu użytkownika");
    })
  }

  mapUserParams() {
    this.firstName?.setValue(this.user.firstName);
    this.lastName?.setValue(this.user.lastName);
    this.displayName?.setValue(this.user.displayName);
    this.email?.setValue(this.user.email);
    this.altEmail?.setValue("");
    this.calendarProvider?.setValue(this.user.calendarProvider);
    this.calendarId?.setValue(this.user.calendarId);
    this.phoneNumber?.setValue(this.user.phoneNumber);
  }

  setUserParams() {
    this.user.firstName = this.firstName?.value;
    this.user.lastName = this.lastName?.value;
    this.user.displayName = this.displayName?.value;
    this.user.email = this.email?.value;
    this.user.phoneNumber = this.phoneNumber?.value;
    this.user.calendarProvider = this.calendarProvider?.value;
    this.user.calendarId = this.calendarId?.value;
  }

  detectUserIdFromRoute() {
    this.activatedRoute.paramMap.pipe(
      map(params => {
        const v = params.get('id');
        return v === null ? null : Number(v);
      }),
      distinctUntilChanged(),
      tap(id => {
        this.id = id ?? 0;
        this.getUserById();
      }),
      takeUntil(this.destroy$)
    ).subscribe();
  }

  goBack() {
    this.location.back();
  }

  submit() {
    if (!this.id) {
      this.addUser();
    } else {
      this.updateUser();
    }
  }

  updateUser() {
    this.setUserParams();
    this.usersService.update(this.user).then((result) => {
      if (result?.succeeded) {
        this.notificationsService.success("Zaktualizowano użytkownika!");
        this.goBack();
      } else {
        this.notificationsService.error(result?.errors ? result.errors.join("\n") : "Nie zaktualizowano użytkownika");
      }
    }, (err) => {
      this.notificationsService.error("Nie zaktualizowano użytkownika");
    });
  }

  addUser() {
    if (this.password?.value != this.confirmPassword?.value) {
      this.notificationsService.error("Podane hasła są różne!");
      return;
    }

    this.setUserParams();
    this.register = new RegisterDTO();
    this.register.userDTO = this.user;
    this.register.password = this.password?.value;
    this.usersService.add(this.register).then((result) => {
      if (result?.succeeded) {
        this.notificationsService.success("Utworzono użytkownika!");
        this.goBack();
      } else {
        this.notificationsService.error(result?.errors ? result.errors.join("\n") : "Nie utworzono użytkownika");
      }
    }, (err) => {
      this.notificationsService.error("Nie utworzono użytkownika");
    });
  }

  addAlternativeEmail() {
    const value = (this.altEmail?.value ?? '').trim();
    if (!value) return;

    this.altEmail?.markAsTouched();
    this.altEmail?.updateValueAndValidity();

    if (!this.altEmail || this.altEmail?.invalid || this.altEmail.value == "") return;
    
    if (!this.user.alternativeEmails) this.user.alternativeEmails = [];
    
    if (this.altEmail.value != this.user.email && !this.user.alternativeEmails.includes(this.altEmail.value)) {
      this.user.alternativeEmails?.push(this.altEmail.value);
      this.altEmail.setValue('');
    } else if (this.user.alternativeEmails?.includes(this.altEmail.value)) {
      this.notificationService.warning("E-mail został już dodany jako alternatywny");
    } else if (this.altEmail.value == this.user.email) {
      this.notificationService.warning("E-mail został ustawiony jako główny e-mail użytkownika");
    }
  }

  removeAlternativeEmail(index: number) {
    this.user.alternativeEmails?.splice(index, 1);
  }

  changePassword() {
    if (this.password?.value != this.confirmPassword?.value) {
      this.notificationsService.error("Podane hasła są różne!");
      return;
    }

    var passwordChange = new ResetPasswordDTO(this.id);
    passwordChange.oldPassword = this.oldPassword?.value;
    passwordChange.newPassword = this.password?.value;

    if (this.loggedUserId != this.id) {
      this.usersService.changeOthersPassword(passwordChange).then((result) => {
        if (result?.succeeded) {
          this.notificationsService.success("Zmieniono hasło!");
          this.goBack();
        } else {
          this.notificationsService.error(result?.errors ? result.errors.join("\n") : "Nie zmieniono hasła");
        }
      }, (err) => {
        this.notificationsService.error("Nie zmieniono hasła");
      });
    } else {
      this.usersService.changeOwnPassword(passwordChange).then((result) => {
        if (result?.succeeded) {
          this.notificationsService.success("Zmieniono hasło!");
          this.goBack();
        } else {
          this.notificationsService.error(result?.errors ? result.errors.join("\n") : "Nie zmieniono hasła");
        }
      }, (err) => {
        this.notificationsService.error("Nie zmieniono hasła");
      });
    }
  }
}
