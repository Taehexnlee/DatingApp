import { ApplicationConfig, importProvidersFrom, provideZoneChangeDetection } from '@angular/core';
import { provideRouter } from '@angular/router';
import { ModalModule } from 'ngx-bootstrap/modal';
import { routes } from './app.routes';
import { provideHttpClient, withInterceptors } from '@angular/common/http';
import { provideAnimations } from '@angular/platform-browser/animations';
import { provideToastr } from 'ngx-toastr';
import { jwtInterceptor } from './_interceptors/jwt.interceptor';
import {NgxSpinnerModule} from 'ngx-spinner';
import { loadingInterceptor } from './_interceptors/loading.interceptor';
import { TimeagoModule } from "ngx-timeago";
export const appConfig: ApplicationConfig = {
  providers: [
    provideRouter(routes),
    provideHttpClient(withInterceptors([jwtInterceptor, loadingInterceptor])),
    provideAnimations(),
    provideToastr({
      positionClass: 'toast-bottom-right'
    }),
    importProvidersFrom(NgxSpinnerModule, TimeagoModule.forRoot(), ModalModule.forRoot())
  ]
    
};
