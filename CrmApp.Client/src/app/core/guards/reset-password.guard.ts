// reset-password-entry.guard.ts
import { Injectable } from '@angular/core';
import { ActivatedRouteSnapshot, CanActivate } from '@angular/router';
import { AuthService } from '../services/auth.service';
import { from, map } from 'rxjs';

@Injectable({ providedIn: 'root' })
export class ResetPasswordEntryGuard implements CanActivate {
    constructor(private auth: AuthService) {}
  canActivate(_: ActivatedRouteSnapshot) {
    return from(this.auth.logout({ silent: true })).pipe(map(() => true));
  }
}
