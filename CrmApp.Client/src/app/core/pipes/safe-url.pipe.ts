import { Pipe, PipeTransform } from '@angular/core';
import { DomSanitizer, SafeResourceUrl } from '@angular/platform-browser';

@Pipe({ name: 'safeUrl', standalone: true }) // jeśli używasz standalone, to już jest importowalna
export class SafeUrlPipe implements PipeTransform {
  constructor(private readonly sanitizer: DomSanitizer) {}

  transform(url: string | null | undefined): SafeResourceUrl {
    if (!url) return '' as any;

    try {
      const u = new URL(url);
      if (u.protocol !== 'https:') throw new Error('Only https allowed');
      if (!/^(storage\.googleapis\.com|.*\.storage\.googleapis\.com|twojadomena\.pl)$/.test(u.host)) {
        throw new Error('Host not allowed');
      }
    } catch {
      // jeśli URL jest nieprawidłowy – zwróć pusty
      return '' as any;
    }

    return this.sanitizer.bypassSecurityTrustResourceUrl(url);
  }
}
