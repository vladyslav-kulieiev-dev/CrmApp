import { Injectable, Inject } from '@angular/core';
import { DOCUMENT } from '@angular/common';
import { BehaviorSubject, fromEvent, startWith } from 'rxjs';

export type Theme = 'light' | 'dark' | 'system';

export const LS_KEY = 'theme';

@Injectable({ providedIn: 'root' })
export class ThemeService {
  private systemDarkQuery = window.matchMedia?.('(prefers-color-scheme: dark)');
  private theme$ = new BehaviorSubject<Theme>(this.readInitialTheme());

  constructor(@Inject(DOCUMENT) private doc: Document) {
    // Reaguj na zmianę systemowego motywu, jeśli wybrano "system"
    if (this.systemDarkQuery) {
      fromEvent(this.systemDarkQuery, 'change').subscribe(() => {
        if (this.theme$.value === 'system') this.applyTheme('system');
      });
    }
    this.applyTheme(this.theme$.value);
  }

  get current$() { return this.theme$.asObservable().pipe(startWith(this.theme$.value)); }
  get current() { return this.theme$.value; }

  setTheme(next: Theme) {
    localStorage.setItem(LS_KEY, next);
    this.theme$.next(next);
    this.applyTheme(next);
  }

  private readInitialTheme(): Theme {
    const saved = localStorage.getItem(LS_KEY) as Theme | null;
    return saved ?? 'system';
  }

  private applyTheme(theme: Theme) {
    const root = this.doc.documentElement;
    // jeśli "system", ustaw atrybut wg media query
    const effective = theme === 'system'
      ? (this.systemDarkQuery?.matches ? 'dark' : 'light')
      : theme;

    root.setAttribute('data-theme', effective);
    root.style.colorScheme = effective; // natywne komponenty dopasują kolory
  }
}
