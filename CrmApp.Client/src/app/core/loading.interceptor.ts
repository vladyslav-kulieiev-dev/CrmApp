// src/app/core/loading.interceptor.ts
import { Injectable } from '@angular/core';
import { HttpEvent, HttpHandler, HttpInterceptor, HttpRequest } from '@angular/common/http';
import { Observable } from 'rxjs';
import { finalize } from 'rxjs/operators';
import { LoadingService } from './services/loading.service';

@Injectable()
export class LoadingInterceptor implements HttpInterceptor {
  constructor(private loading: LoadingService) {}

  intercept(req: HttpRequest<any>, next: HttpHandler): Observable<HttpEvent<any>> {
    // allow opt-out per request: set header 'x-no-loader' to 'true'
    const skip = req.headers.get('x-no-loader') === 'true';
    if (skip) {
      const cleanReq = req.clone({ headers: req.headers.delete('x-no-loader') });
      return next.handle(cleanReq);
    }
    const isGoogleCloud = req.url.includes("storage.googleapis.com/example-app-ingest");
    if (isGoogleCloud)
      return next.handle(req);

    this.loading.show();
    return next.handle(req).pipe(
      finalize(() => this.loading.hide())
    );
  }
}
