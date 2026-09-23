// web-speech-dictate.service.ts
import { Injectable } from '@angular/core';
import { BehaviorSubject, Subject } from 'rxjs';

// src/types/web-speech.d.ts
declare var SpeechRecognition: {
  new (): SpeechRecognition;
} | undefined;

declare var webkitSpeechRecognition: {
  new (): SpeechRecognition;
} | undefined;

interface SpeechRecognition {
  lang: string;
  continuous: boolean;
  interimResults: boolean;
  start(): void;
  stop(): void;
  abort?: () => void;

  onresult?: (ev: SpeechRecognitionEvent) => void;
  onend?: (ev: Event) => void;
  onerror?: (ev: SpeechRecognitionErrorEvent) => void;
}

interface SpeechRecognitionEvent extends Event {
  resultIndex: number;
  results: SpeechRecognitionResultList;
}

interface SpeechRecognitionResultList {
  length: number;
  item(index: number): SpeechRecognitionResult;
  [index: number]: SpeechRecognitionResult;
}

interface SpeechRecognitionResult {
  isFinal: boolean;
  length: number;
  item(index: number): SpeechRecognitionAlternative;
  [index: number]: SpeechRecognitionAlternative;
  0: SpeechRecognitionAlternative;
}

interface SpeechRecognitionAlternative {
  transcript: string;
  confidence: number;
}

interface SpeechRecognitionErrorEvent extends Event {
  error:
    | 'no-speech'
    | 'audio-capture'
    | 'not-allowed'
    | 'aborted'
    | 'network'
    | 'service-not-allowed'
    | 'bad-grammar'
    | 'language-not-supported';
}


@Injectable({ providedIn: 'root' })
export class WebSpeechDictateService {
  private rec?: SpeechRecognition;
  private _active = false;

  isSupported$ = new BehaviorSubject<boolean>(false);
  isRecording$ = new BehaviorSubject<boolean>(false);
  partial$ = new BehaviorSubject<string>('');
  final$ = new Subject<string>();

  constructor() {
    const Ctor = (window as any).SpeechRecognition || (window as any).webkitSpeechRecognition;
    this.isSupported$.next(!!Ctor);
    if (!Ctor) return;

    this.rec = new Ctor();
    this.rec!.continuous = true;       // keep listening across phrases
    this.rec!.interimResults = true;   // get partials
    this.rec!.lang = 'pl-PL';          // set your default locale
    this.bindEvents();
  }

  setLanguage(lang: string) {
    if (this.rec) this.rec.lang = lang;
  }

  async start() {
    if (!this.rec || this._active) return;
    // iOS/Safari requires a direct user gesture to start()
    this._active = true;
    this.partial$.next('');
    try {
      this.rec.start();
      this.isRecording$.next(true);
    } catch {}
  }

  stop() {
    if (!this.rec) return;
    this._active = false;
    try { this.rec.stop(); } catch {}
    this.isRecording$.next(false);
    this.partial$.next('');
  }

  private bindEvents() {
    if (!this.rec) return;

    this.rec.onresult = (ev: SpeechRecognitionEvent) => {
      let interim = '';
      for (let i = ev.resultIndex; i < ev.results.length; i++) {
        const res = ev.results[i];
        if (res.isFinal) {
          const text = res[0].transcript.trim();
          if (text) this.final$.next(text);
        } else {
          interim += res[0].transcript;
        }
      }
      this.partial$.next(interim.trim());
    };

    this.rec.onend = () => {
      // Chrome fires onend frequently; auto-resume if user still recording
      if (this._active) {
        try { this.rec!.start(); } catch {}
      } else {
        this.isRecording$.next(false);
      }
    };

    this.rec.onerror = (e: any) => {
      // Common errors: 'not-allowed', 'no-speech', 'audio-capture'
      console.warn('Speech error', e?.error, e?.message);
      // If permission denied, stop the loop
      if (e?.error === 'not-allowed') this._active = false;
    };
  }
}
