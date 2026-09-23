// device.service.ts
import { Injectable } from '@angular/core';
import { BreakpointObserver, Breakpoints } from '@angular/cdk/layout';
import { map, distinctUntilChanged, shareReplay } from 'rxjs/operators';
import { Observable } from 'rxjs';

@Injectable({ providedIn: 'root' })
export class DeviceService {
  readonly isMobile$: Observable<boolean>;
  readonly isTablet$: Observable<boolean>;

  constructor(private bp: BreakpointObserver) {
    this.isMobile$ = this.bp.observe(Breakpoints.Handset).pipe(
        map(state => state.matches),
        distinctUntilChanged(),
        shareReplay({ bufferSize: 1, refCount: true })
    );
    this.isTablet$ = this.bp.observe([Breakpoints.Small, Breakpoints.Medium]).pipe(
      map(s => s.breakpoints[Breakpoints.Small] || s.breakpoints[Breakpoints.Medium]),
      distinctUntilChanged(),
      shareReplay({ bufferSize: 1, refCount: true })
    );
  }
}
