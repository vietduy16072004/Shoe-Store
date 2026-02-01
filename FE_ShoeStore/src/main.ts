// src/main.ts
import { bootstrapApplication } from '@angular/platform-browser';
import { appConfig } from './app/app.config';
import { App } from './app/app'; // Sửa từ 'App' thành 'AppComponent'

bootstrapApplication(App, appConfig) // Truyền AppComponent vào đây
  .catch((err) => console.error(err));