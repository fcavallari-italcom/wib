import { Injectable } from '@angular/core';

export interface EnvConfig {
  apiUrl: string;
  ocrUrl: string;
  mlUrl: string;
  qdrantUrl: string;
  minioUrl: string;
}

@Injectable({ providedIn: 'root' })
export class EnvService {
  private cfg: EnvConfig = (window as any).__env || {
    apiUrl: '',
    ocrUrl: '',
    mlUrl: '',
    qdrantUrl: '',
    minioUrl: ''
  };

  get apiUrl(): string { return this.cfg.apiUrl; }
  get ocrUrl(): string { return this.cfg.ocrUrl; }
  get mlUrl(): string { return this.cfg.mlUrl; }
  get qdrantUrl(): string { return this.cfg.qdrantUrl; }
  get minioUrl(): string { return this.cfg.minioUrl; }
}
