// loading.service.ts
import { Injectable } from '@angular/core';
import { BehaviorSubject, Observable } from 'rxjs';
import { debounceTime, distinctUntilChanged, map } from 'rxjs/operators';

@Injectable({ providedIn: 'root' })
export class LoadingService {
  private activeRequests = 0;
  private readonly _counter$ = new BehaviorSubject<number>(0);

  readonly isLoading$: Observable<boolean> = this._counter$.pipe(
    debounceTime(200),
    map(count => count > 0),
    distinctUntilChanged()
  );

  show(): void {
    this.activeRequests++;
    this._counter$.next(this.activeRequests);
  }

  hide(): void {
    this.activeRequests = Math.max(0, this.activeRequests - 1);
    this._counter$.next(this.activeRequests);
  }

  set(isLoading: boolean): void {
    isLoading ? this.show() : this.hide();
  }
}
