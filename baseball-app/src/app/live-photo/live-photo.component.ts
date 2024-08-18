import { AfterViewInit, Component, ElementRef, Input, ViewChild } from '@angular/core';
import * as LivePhotosKit from 'livephotoskit';
import { RemoteOriginal } from '../contracts/remote-original';
import { Utils } from '../utils';

@Component({
    selector: 'app-live-photo',
    standalone: true,
    imports: [],
    templateUrl: './live-photo.component.html',
    styleUrl: './live-photo.component.scss'
})
export class LivePhotoComponent implements AfterViewInit {

    private _photo!: RemoteOriginal;

    @Input({ required: true })
    set photo(val: RemoteOriginal) {
        this._photo = val;
        this.initPhoto();
    }
    get photo(): RemoteOriginal {
        return this._photo;
    }

    @ViewChild('livephoto')
    photoDiv?: ElementRef

    ngAfterViewInit(): void {
        this.initPhoto();
    }

    private initPhoto(): void {
        if (this.photoDiv && this.photo.photo && this.photo.video) {
            LivePhotosKit.augmentElementAsPlayer(this.photoDiv.nativeElement, {
                photoSrc: Utils.keyToUrl(this.photo.photo.key),
                videoSrc: Utils.keyToUrl(this.photo.video.key)
            });
        }
    }
}
