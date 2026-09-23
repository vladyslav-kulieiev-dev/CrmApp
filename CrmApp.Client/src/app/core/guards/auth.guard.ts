import { inject, Injectable } from '@angular/core';
import { CanActivateFn, Router } from '@angular/router';
import { AuthService } from '../services/auth.service';


export const authGuardSomePermissions: CanActivateFn = (route, state) => {
  const auth = inject(AuthService);
  const router = inject(Router);
  var permissions = route.data['permission'] as string[] ?? [];
  var isPermitted = false;

  if (auth.userConfiguration && permissions && permissions.length > 0 && 
    permissions.some(x => (auth.userConfiguration! as any)[x])) {
    isPermitted = true;
  }

  return auth.isAuthenticated && isPermitted
    ? true
    : router.createUrlTree(['/login'], { queryParams: { returnUrl: state.url } });
};

export const authGuard: CanActivateFn = (route, state) => {
  const auth = inject(AuthService);
  const router = inject(Router);
  var permissions = route.data['permission'] as string[] ?? [];
  var hasAllPermissions = true;

  if (auth.userConfiguration && permissions && permissions.length > 0 && 
    permissions.some(x => (auth.userConfiguration! as any)[x] !== true)) {
    hasAllPermissions = false;
  }

  return auth.isAuthenticated && hasAllPermissions
    ? true
    : router.createUrlTree(['/login'], { queryParams: { returnUrl: state.url } });
};
