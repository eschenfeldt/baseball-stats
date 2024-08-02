import { APP_INITIALIZER, ApplicationConfig, provideZoneChangeDetection } from '@angular/core';
import { provideRouter, Router } from '@angular/router';

import { routes } from './app.routes';
import { provideAnimationsAsync } from '@angular/platform-browser/animations/async';
import { provideHttpClient } from '@angular/common/http';
import { routeInitializer } from './param.decorator';

export const appConfig: ApplicationConfig = {
    providers: [
        provideZoneChangeDetection({ eventCoalescing: true }),
        provideRouter(routes),
        provideAnimationsAsync(),
        provideHttpClient(),
        {
            provide: APP_INITIALIZER,
            useFactory: routeInitializer,
            deps: [Router],
            multi: true
        }
    ]
};
