import { Component, Input } from '@angular/core';
import { RemoteFileDetail } from '../contracts/remote-file-detail';
import { MatTooltip } from '@angular/material/tooltip';
import { Utils } from '../utils';
import { environment } from '../../environments/environment';
import { RouterModule } from '@angular/router';
import { MediaParams } from '../media-gallery/media-gallery.component';

@Component({
    selector: 'app-thumbnail',
    standalone: true,
    imports: [
        MatTooltip,
        RouterModule
    ],
    templateUrl: './thumbnail.component.html',
    styleUrl: './thumbnail.component.scss'
})
export class ThumbnailComponent {
    @Input({ required: true })
    file!: RemoteFileDetail

    @Input()
    singleDayMode: boolean = false;

    @Input()
    queryParams?: MediaParams

    get title(): string {
        if (this.singleDayMode) {
            return Utils.formatTime(this.file.dateTime);
        } else {
            return Utils.formatDateTime(this.file.dateTime);
        }
    }

    get src(): string {
        const keyEnd = this.file.key.replace(this.file.assetIdentifier, '');
        // return `${environment.bucketUrl}/${this.file.assetIdentifier.toUpperCase()}${keyEnd}`;
        return `${environment.bucketUrl}/${this.file.key}`;
    }
}
