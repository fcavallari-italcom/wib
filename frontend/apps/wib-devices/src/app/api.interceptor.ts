import { HttpInterceptorFn } from '@angular/common/http';
import { inject } from '@angular/core';
import { EnvService } from './env.service';

export const apiInterceptor: HttpInterceptorFn = (req, next) => {
  const env = inject(EnvService);
  const apiReq = req.clone({ url: env.apiUrl + req.url });
  return next(apiReq);
};
