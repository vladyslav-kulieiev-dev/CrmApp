export function sameSet<T extends string | number | boolean | null | undefined>(
  a: readonly T[],
  b: readonly T[]
): boolean {
  const A = new Set(a);
  const B = new Set(b);
  if (A.size !== B.size) return false;
  for (const v of A) if (!B.has(v)) return false;
  return true;
}

export function getJoinedMesseges(alternativeMessage: string, messages?: string[],) {
  return messages && messages.length ? messages.join("\n") : alternativeMessage;
}

export function formatDuration(ms: number): string {
  if (ms <= 0) return "00:00";
  const totalSec = Math.floor(ms / 1000);
  const h = Math.floor(totalSec / 3600);
  const m = Math.floor((totalSec % 3600) / 60);
  const s = totalSec % 60;
  const pad = (n: number) => n.toString().padStart(2, '0');
  return h > 0 ? `${pad(h)}:${pad(m)}:${pad(s)}` : `${pad(m)}:${pad(s)}`;
}

// export function filterTree<T extends { children?: T[] }>(nodes: T[], pred: (n: T) => boolean): T[] {
//   const out: T[] = [];
//   for (const n of nodes || []) {
//     const kids = n.children ? filterTree(n.children, pred) : [];
//     if (pred(n) || kids.length) {
//       out.push({ ...(n as any), children: kids } as T);
//     }
//   }
//   return out;
// }
export function filterTree<T extends { children?: T[] }>(
  nodes: T[],
  pred: (n: T) => boolean
): T[] {
  const out: T[] = [];

  for (const n of nodes || []) {
    const filteredChildren = n.children
      ? filterTree(n.children, pred)
      : [];

    const selfMatches = pred(n);

    if (selfMatches || filteredChildren.length) {
      out.push({
        ...(n as any),
        children: filteredChildren  
      } as T);
    }
  }

  return out;
}

import { Form } from '@angular/forms';
import { MatDateFormats } from '@angular/material/core';

export const DATE_TO_BACKEND_FORMAT = "yyyy-MM-dd HH:mm:ss";
export const PL_DOT_DATE_FORMATS: MatDateFormats = {
  parse: {
    dateInput: ['dd.MM.yyyy', 'd.M.yyyy', 'yyyy-MM-dd']
  },
  display: {
    dateInput: 'dd.MM.yyyy',
    monthYearLabel: 'MMMM yyyy',
    dateA11yLabel: 'dd.MM.yyyy',
    monthYearA11yLabel: 'MMMM yyyy'
  }
};

export function toLocalIsoWithOffset(d: Date): string {
  const pad = (n: number) => String(n).padStart(2, '0');
  const yyyy = d.getFullYear();
  const MM = pad(d.getMonth() + 1);
  const dd = pad(d.getDate());
  const HH = pad(d.getHours());
  const mm = pad(d.getMinutes());
  const ss = pad(d.getSeconds());
  const offMin = -d.getTimezoneOffset();
  const sign = offMin >= 0 ? '+' : '-';
  const oh = pad(Math.trunc(Math.abs(offMin) / 60));
  const om = pad(Math.abs(offMin) % 60);
  return `${yyyy}-${MM}-${dd}T${HH}:${mm}:${ss}${sign}${oh}:${om}`;
}

export function flatten(nodes: any[]): any[] {
  return nodes.flatMap(n => [n, ...(n.children?.length ? flatten(n.children) : [])]);
}

export function isAudio(file: File): boolean {
  return file.type?.startsWith('audio/')
    || /\.(wav|mp3|m4a|aac|flac|ogg|opus|webm)$/i.test(file.name);
}

export function isImg(file: File): boolean {
  const mime = file.type?.toLowerCase();
  const name = file.name?.toLowerCase();

  const allowedMime = [
    'image/png',
    'image/jpeg',
    'image/jpg',
    'image/webp',
    'image/gif',
    'image/bmp',
    'image/heic',
    'image/heif',
    'image/avif'
  ];

  if (mime && allowedMime.some(m => mime === m || mime.startsWith('image/'))) {
    return true;
  }

  return /\.(png|jpg|jpeg|webp|gif|bmp|heic|heif|avif)$/i.test(name);
}