import { HttpClient } from '@angular/common/http';
import { inject, Injectable, signal } from '@angular/core';
import { BehaviorSubject, catchError, distinctUntilChanged, firstValueFrom, map, Observable, of, tap } from 'rxjs';
import { LoginDTO } from 'src/models/DTO/LoginDTO';
import { ErrorResultDTO, ResultDTO } from 'src/models/DTO/ResultDTO';
import { RolesClaimsDTO } from 'src/models/DTO/RolesDTO';
import { UsersDTO } from 'src/models/DTO/UsersDTO';
import { UserConfiguration } from 'src/models/identity/UserConfiguration';
import { DbService } from './db.service';
import { ApiService } from './api.service';
import { Router } from '@angular/router';

export function initAuth(auth: AuthService) {
  return () => auth.checkSession();
}
@Injectable({ providedIn: 'root' })
export class AuthService extends ApiService {
  private http = inject(HttpClient);

  private userSubject = new BehaviorSubject<UsersDTO | null>(null);
  private userConfig = new BehaviorSubject<UserConfiguration | null>(null);
  readonly user$ = this.userSubject.asObservable();
  readonly loggedIn$ = this.user$.pipe(map(u => !!u), distinctUntilChanged());
  ready = signal(false);

  get user(): UsersDTO | null { return this.userSubject.value; }
  get isAuthenticated(): boolean { return !!this.userSubject.value; }
  get userConfiguration(): UserConfiguration | null { return this.userConfig.value; }

  constructor(private router: Router) {
    super();
    this.setControllerUrl('auth');
  }

  fullUrl(url: string): string {
    return `${this.controllerUrl}/${url}`;
  }

  isLoggedIn(): Observable<boolean> {
    return this.dbService.getByParams<ResultDTO<any>>(this.fullUrl('is-logged-in')).pipe(
      map(res => res.succeeded),
      catchError(() => { this.userSubject.next(null); return of(false); })
    );
  }

  bootstrap(): Promise<void> {
    return this.dbService.getByParams<ResultDTO<UsersDTO>>(this.fullUrl('me'))
      .toPromise()
      .then(u => {
        u && u.succeeded && u.data ? this.userSubject.next(u.data) : this.userSubject.next(null);
        u && u.succeeded && u.data && u.data.userConfiguration ? this.userConfig.next(u.data.userConfiguration) : this.userConfig.next(null);
      })
      .catch(() => this.userSubject.next(null))
      .finally(() => this.ready.set(true));
  }

  isAdmin(): boolean {
    return this.userConfiguration?.isAdmin ?? false;
  }

  login(credentials: LoginDTO): Observable<ResultDTO<UsersDTO>> {
    return this.dbService.post<ResultDTO<UsersDTO>>(this.fullUrl('login'), credentials).pipe(
      tap(u => {
        this.userSubject.next((u.data != undefined) ? u.data : null);
        this.userConfig.next((u.data != undefined && u.data.userConfiguration != undefined) ? u.data.userConfiguration : null);
      }),
      catchError((err: any) => {
        console.error('Login failed:', err);
        return of(new ErrorResultDTO<UsersDTO>(err.error?.errors));
      })
    );
  }
  
  async logout(opts?: { silent?: boolean }): Promise<void> {
    await firstValueFrom(this._logout$(opts)); 
  }

  private _logout$(opts?: { silent?: boolean }): Observable<void> {
    return this.dbService.post<void>(this.fullUrl('logout'), {}).pipe(
      tap(() => {
        this.userSubject.next(null);
        this.userConfig.next(null);
        if (!opts || !opts.silent) this.router.navigate(['/login']);
      })
    );
  }

  me(): Observable<ResultDTO<UsersDTO>> {
    return this.http.get<ResultDTO<UsersDTO>>('api/' + this.fullUrl('me')).pipe(
      tap(u => {
        this.userSubject.next((u.data != undefined) ? u.data : null);
        this.userConfig.next((u.data && u.data.userConfiguration) ? u.data.userConfiguration : null);
      })
    );
  }

  checkSession(): Observable<boolean> {
    return this.http.get<ResultDTO<UsersDTO>>('api/' + this.fullUrl('me')).pipe(
      tap(u => {
        this.userSubject.next((u.data != undefined) ? u.data : null);
        this.userConfig.next((u.data && u.data.userConfiguration) ? u.data.userConfiguration : null);
      }),
      map(() => true),
      catchError(() => { 
        this.userSubject.next(null); 
        this.userConfig.next(null); 
        return of(false); })
    );
  }

  getRoles(): Observable<RolesClaimsDTO[]> {
    return this.dbService.getByParams<RolesClaimsDTO[]>('identity/roles');
  }

  forgotPassword(email: string) {
    return this.http.post('api/' + this.fullUrl('forgot-password'), { email });
  }

  resetPassword(email: string, token: string, newPassword: string) {
    return this.http.post('api/' + this.fullUrl('reset-password'), { email, token, newPassword });
  }
}   