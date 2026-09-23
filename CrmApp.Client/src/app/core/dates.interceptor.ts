import { Injectable } from '@angular/core';
import { HttpInterceptor, HttpRequest, HttpHandler, HttpEvent } from '@angular/common/http';
import { Observable } from 'rxjs';
import { toLocalIsoWithOffset } from './services/extensions.service';

@Injectable()
export class DatesWithOffsetInterceptor implements HttpInterceptor {
  intercept(req: HttpRequest<any>, next: HttpHandler): Observable<HttpEvent<any>> {

    const isPlainObject = (v: any): v is Record<string, unknown> =>
      v !== null && typeof v === 'object' && (v.constructor === Object || Object.getPrototypeOf(v) === Object.prototype);

    const convert = (v: any): any => {
      if (v instanceof Date) return toLocalIsoWithOffset(v);
      if (Array.isArray(v)) return v.map(convert);
      if (isPlainObject(v)) {
        const out: any = {};
        for (const [k, val] of Object.entries(v)) out[k] = convert(val);
        return out;
      }
      return v;
    };

    const body = req.body;
    if (
      body instanceof FormData ||
      body instanceof Blob ||
      (typeof File !== 'undefined' && body instanceof File) ||
      body instanceof ArrayBuffer
    ) {
      return next.handle(req);
    }

    const newBody = convert(body);
    return next.handle(newBody === body ? req : req.clone({ body: newBody }));
  }
}
