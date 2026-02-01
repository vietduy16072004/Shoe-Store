import { ApplicationConfig } from '@angular/core';
import { provideRouter } from '@angular/router';
import { routes } from './app.routes';
import { provideHttpClient, withInterceptorsFromDi } from '@angular/common/http';

export const appConfig: ApplicationConfig = {
  providers: [
    // Giữ lại các dịch vụ cốt yếu để chạy Router và API
    provideRouter(routes),
    provideHttpClient(withInterceptorsFromDi()) 
  ]
};