import { Injectable } from '@angular/core';

export interface EnvConfig { apiUrl: string; }

@Injectable({ providedIn: 'root' })
export class EnvService {
  private cfg: EnvConfig = (window as any).__env || { apiUrl: '' };
  get apiUrl(): string { return this.cfg.apiUrl; }
}
