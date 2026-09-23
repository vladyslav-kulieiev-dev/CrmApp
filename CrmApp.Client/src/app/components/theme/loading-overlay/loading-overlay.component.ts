// src/app/shared/loading-overlay/loading-overlay.component.ts
import { ChangeDetectionStrategy, Component } from '@angular/core';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { AsyncPipe, NgIf } from '@angular/common';
import { LoadingService } from 'src/app/core/services/loading.service';

@Component({
  selector: 'crm-loading-overlay',
  standalone: false,
  template: `
    <div
      class="app-overlay"
      *ngIf="loading.isLoading$ | async"
      aria-live="polite"
      aria-busy="true"
      role="alert"
    >
      <mat-progress-spinner mode="indeterminate" diameter="64"></mat-progress-spinner>
    </div>
  `,
  styles: [`
    .app-overlay {
      position: fixed;
      inset: 0;
      display: grid;
      place-items: center;
      background: rgba(0,0,0,0.35);       /* dim the app */
      z-index: 10000;                      /* above everything */
      pointer-events: all;                 /* capture all clicks */
      /* prevent scroll underneath on some browsers */
      overscroll-behavior: contain;
    }
  `],
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class LoadingOverlayComponent {
  constructor(public loading: LoadingService) {}
}
