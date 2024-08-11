import { AfterViewInit, Component, ElementRef, Input, ViewChild } from '@angular/core';
import { LivePhoto } from '../contracts/live-photo';
import * as LivePhotosKit from 'livephotoskit';

@Component({
    selector: 'app-live-photo',
    standalone: true,
    imports: [],
    templateUrl: './live-photo.component.html',
    styleUrl: './live-photo.component.scss'
})
export class LivePhotoComponent implements AfterViewInit {

    @Input({ required: true })
    photo!: LivePhoto

    @ViewChild('livephoto')
    photoDiv?: ElementRef

    ngAfterViewInit(): void {
        if (this.photoDiv) {
            LivePhotosKit.augmentElementAsPlayer(this.photoDiv.nativeElement, {
                photoSrc: "https://eschenfeldt-baseball-media.nyc3.cdn.digitaloceanspaces.com/live-photos/IMG_4316.HEIC",
                videoSrc: "https://eschenfeldt-baseball-media.nyc3.cdn.digitaloceanspaces.com/live-photos/IMG_4316.mov"
            });
        }
    }
}
