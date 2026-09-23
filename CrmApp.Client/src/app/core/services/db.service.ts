import { HttpClient, HttpHeaders } from "@angular/common/http";
import { inject, Injectable } from "@angular/core";

@Injectable({ providedIn: 'root' })
export class DbService {
    public http = inject(HttpClient);

    getById<T>(url: string, objectId?: string, params?: any) {
        return this.http.get<T>(`api/${url}/${objectId}`, { params, withCredentials: true });
    }

    getByIdSkipLoader<T>(url: string, objectId?: string, params?: any) {
        return this.http.get<T>(`api/${url}/${objectId}`, { params, withCredentials: true, headers: { 'x-no-loader': 'true' } });
    }

    getArrayBufferAsPdf(url: string, params?: any) {
        return this.http.get(`api/${url}`, { responseType: 'blob', params, withCredentials: true });
    }

    getArrayBufferByParams(url: string, params?: any) {
        return this.http.get(`api/${url}`, { responseType: 'arraybuffer', params, withCredentials: true });
    }

    getByParams<T>(url: string, params?: any) {
        return this.http.get<T>(`api/${url}`, { params, withCredentials: true });
    }

    getByParamsSkipLoader<T>(url: string, params?: any) {
        return this.http.get<T>(`api/${url}`, { params, withCredentials: true, headers: { 'x-no-loader': 'true' } });
    }

    post<T>(url: string, body: any, params?: any) {
        if (params) {
            return this.http.post<T>(`api/${url}`, body, { params, withCredentials: true, headers: { 'x-no-loader': 'true' }  });
        }
        return this.http.post<T>(`api/${url}`, body, { withCredentials: true, headers: { 'x-no-loader': 'true' }  });
    }
    
    postWithCancelOption<T>(url: string, body: any, signal: AbortSignal, params?: any) {
        const options = {
            withCredentials: true,
            headers: new HttpHeaders({ 'x-no-loader': 'true' }),
            params,
            signal,
            observe: 'body' as const  
        };

        return this.http.post<T>(`api/${url}`, body, options);
    }

    postSkipLoader<T>(url: string, body: any, params?: any) {
        if (params) {
            return this.http.post<T>(`api/${url}`, body, { params, withCredentials: true, headers: { 'x-no-loader': 'true' } });
        }
        return this.http.post<T>(`api/${url}`, body, { withCredentials: true, headers: { 'x-no-loader': 'true' } });
    }

    putSkipLoader<T>(url: string, body: any, params?: any) {
        if (params) {
            return this.http.put<T>(`api/${url}`, body, { params, withCredentials: true, headers: { 'x-no-loader': 'true' } });
        }
        return this.http.put<T>(`api/${url}`, body, { withCredentials: true, headers: { 'x-no-loader': 'true' } });
    }

    put<T>(url: string, body: any, params?: any) {
        if (params) {
            return this.http.put<T>(`api/${url}`, body, { params, withCredentials: true });
        }
        return this.http.put<T>(`api/${url}`, body, { withCredentials: true });
    }

    delete<T>(url: string, objectId: string, params?: any) {
        return this.http.delete<T>(`api/${url}/${objectId}`, { params, withCredentials: true });
    }
    
    upload<T>(url: string, file: File, params?: any) {
        const form = new FormData();
        form.append('file', file, file.name);

        if (params)
            return this.http.post<T>(`api/${url}`, form, { params, withCredentials: true });
        
        return this.http.post<T>(`api/${url}`, form, { withCredentials: true });
    }
    
    uploadBlob<T>(url: string, blob: Blob, filename: string, params?: any) {
        const form = new FormData();
        form.append('file', blob, filename);

        if (params)
            return this.http.post<T>(`api/${url}`, form, { params, withCredentials: true });
        
        return this.http.post<T>(`api/${url}`, form, { withCredentials: true });
    }
    
    postFormData<T>(url: string, fd: FormData, reportProgress?: boolean) {
        return this.http.post<T>(`api/${url}`, fd, 
            {                 
                withCredentials: true,
                reportProgress: reportProgress, 
                // observe: "events",
                headers: { 'x-no-loader': 'true' }
            });
    }
    
    postFormDataWithLoader<T>(url: string, fd: FormData, reportProgress?: boolean) {
        return this.http.post<T>(`api/${url}`, fd, 
            {                 
                withCredentials: true,
                reportProgress: reportProgress, 
                // observe: "events"
            });
    }
}