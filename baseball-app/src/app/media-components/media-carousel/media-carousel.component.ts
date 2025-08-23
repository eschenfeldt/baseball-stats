import { Component, OnInit } from '@angular/core';
import { param } from '../../param.decorator';
import { BASEBALL_ROUTES } from '../../app.routes';
import { BaseballApiService } from '../../baseball-api.service';
import { Observable, switchMap } from 'rxjs';
import { AsyncPipe } from '@angular/common';
import { LivePhotoComponent } from '../live-photo/live-photo.component';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { RemoteOriginal } from '../../contracts/remote-original';
import { Utils } from '../../utils';
import { ThumbnailScrollComponent } from '../thumbnail-scroll/thumbnail-scroll.component';

@Component({
    selector: 'app-media-carousel',
    imports: [
        MatProgressSpinnerModule,
        AsyncPipe,
        LivePhotoComponent,
        ThumbnailScrollComponent
    ],
    templateUrl: './media-carousel.component.html',
    styleUrl: './media-carousel.component.scss'
})
export class MediaCarouselComponent implements OnInit {

    @param<typeof BASEBALL_ROUTES.MEDIA>('assetIdentifier')
    assetIdentifier$!: Observable<string>;
    focusedOriginal$?: Observable<RemoteOriginal>;

    constructor(
        private api: BaseballApiService
    ) { }

    ngOnInit(): void {
        this.focusedOriginal$ = this.assetIdentifier$.pipe(
            switchMap((assetIdentifier) => {
                return this.api.makeApiGet<RemoteOriginal>(`media/original/${assetIdentifier}`);
            }));
    }

    photoUrl(focusedItem: RemoteOriginal): string | null {
        if (focusedItem.photo) {
            return Utils.keyToUrl(focusedItem.photo.key);
        } else {
            return null;
        }
    }

    photoType(focusedItem: RemoteOriginal): string | null {
        if (focusedItem.photo) {
            return `image/${focusedItem.photo.extension.substring(1)}`;
        } else {
            return null;
        }
    }

    altPhotoUrl(focusedItem: RemoteOriginal): string | null {
        if (focusedItem.alternatePhoto) {
            return Utils.keyToUrl(focusedItem.alternatePhoto.key);
        } else {
            return null;
        }
    }

    videoUrl(focusedItem: RemoteOriginal): string | null {
        if (focusedItem.video) {
            return Utils.keyToUrl(focusedItem.video.key);
        } else {
            return null;
        }
    }

    alternateVideoUrl(focusedItem: RemoteOriginal): string | null {
        if (focusedItem.alternateVideo) {
            return Utils.keyToUrl(focusedItem.alternateVideo.key);
        } else {
            return null;
        }
    }

    videoType(focusedItem: RemoteOriginal): string {
        return Utils.videoTypeFromExtension(focusedItem.video.extension);
    }

    altVideoType(focusedItem: RemoteOriginal): string | null {
        if (focusedItem.alternateVideo) {
            return Utils.videoTypeFromExtension(focusedItem.alternateVideo.extension);
        } else {
            return null;
        }
    }

    altText(focusedItem: RemoteOriginal): string {
        const file = focusedItem.photo || focusedItem.video;
        if (file) {
            return `${file.fileType} taken ${Utils.formatDateTime(file.dateTime)}`;
        } else {
            return "Unknown file";
        }
    }

    headerText(focusedItem: RemoteOriginal): string {
        const file = focusedItem.photo || focusedItem.video;
        if (focusedItem.gameName) {
            return `${focusedItem.gameName} - ${Utils.formatTime(file.dateTime)}`;
        } else {
            return Utils.formatDateTime(file.dateTime);
        }
    }
}
