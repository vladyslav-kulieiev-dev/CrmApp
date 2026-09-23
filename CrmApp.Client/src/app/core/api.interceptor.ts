import { HttpEvent, HttpHandler, HttpInterceptor, HttpRequest } from "@angular/common/http";
import { inject, Injectable } from "@angular/core";
import { API_BASE_URL } from "../api-tokens";
import { Observable } from "rxjs";

@Injectable()
export class ApiInterceptor implements HttpInterceptor {
  private baseUrl = inject(API_BASE_URL) as string;

  intercept(req: HttpRequest<unknown>, next: HttpHandler): Observable<HttpEvent<unknown>> {
    const isAbsolute = /^[a-z][a-z0-9+.-]*:\/\//i.test(req.url);
    const isApi = req.url.startsWith('/api') || req.url.startsWith('api/');
    const isGoogleCloud = req.url.includes("storage.googleapis.com/example-app-ingest");
    // Prefiksuj tylko /api
  
    if (!isAbsolute && isApi && this.baseUrl && !isGoogleCloud) {
      const url = `${this.baseUrl.replace(/\/+$/, '')}/${req.url.replace(/^\/+/, '')}`;
      return next.handle(req.clone({ url, withCredentials: true }));
    }

    // Zasoby (assets, i18n, itp.) zostaw jak są; bez cookies
    return next.handle(req);
  }
}