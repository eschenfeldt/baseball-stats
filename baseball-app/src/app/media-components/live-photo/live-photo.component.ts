import { Component, ElementRef, Input, ViewChild } from '@angular/core';
import { RemoteOriginal } from '../../contracts/remote-original';
import { Utils } from '../../utils';
import { animate, state, style, transition, trigger } from '@angular/animations';
import { MatIconModule } from '@angular/material/icon';
import { MatButtonModule } from '@angular/material/button';

@Component({
    selector: 'app-live-photo',
    standalone: true,
    imports: [
        MatIconModule,
        MatButtonModule
    ],
    templateUrl: './live-photo.component.html',
    styleUrl: './live-photo.component.scss',
    animations: [
        trigger('fade', [
            state(
                'visible',
                style({ 'opacity': 1, 'z-index': 2 })
            ),
            state(
                'hidden',
                style({ 'opacity': 0, 'z-index': 1 })
            ),
            transition('visible => hidden, hidden => visible', [animate('0.5s')])
        ])
    ]
})
export class LivePhotoComponent {

    @Input({ required: true })
    photo!: RemoteOriginal

    @ViewChild('video')
    videoElement?: ElementRef

    showImage: boolean = true;

    get imgSrc(): string {
        return Utils.keyToUrl(this.photo.photo.key);
    }
    get imgType(): string {
        return `image/${this.photo.photo.extension.substring(1)}`;
    }
    get altImgSrc(): string | null {
        if (this.photo.alternatePhoto) {
            return Utils.keyToUrl(this.photo.alternatePhoto.key);
        } else {
            return null;
        }
    }
    get altImgType(): string | null {
        if (this.photo.alternatePhoto) {
            return `image/${this.photo.alternatePhoto.extension.substring(1)}`;
        } else {
            return null;
        }
    }
    get imgState(): string {
        return this.showImage ? 'visible' : 'hidden';
    }
    get videoSrc(): string {
        return Utils.keyToUrl(this.photo.video.key);
    }

    get altVideoSrc(): string | null {
        if (this.photo.alternateVideo) {
            return Utils.keyToUrl(this.photo.alternateVideo.key);
        } else {
            return null;
        }
    }

    get videoType(): string {
        return Utils.videoTypeFromExtension(this.photo.video.extension);
    }
    get altVideoType(): string | null {
        if (this.photo.alternateVideo) {
            return Utils.videoTypeFromExtension(this.photo.alternateVideo.extension);
        } else {
            return null;
        }
    }

    get videoState(): string {
        return this.showImage ? 'hidden' : 'visible';
    }
    toggleImage(): void {
        this.showImage = !this.showImage;
        if (!this.showImage && this.videoElement) {
            this.videoElement.nativeElement.play();
        }
    }

    get altText(): string {
        if (this.photo) {
            return `${this.photo.fileType} taken ${Utils.formatDateTime(this.photo.photo.dateTime)}`;
        } else {
            return "Unknown file";
        }
    }
}
