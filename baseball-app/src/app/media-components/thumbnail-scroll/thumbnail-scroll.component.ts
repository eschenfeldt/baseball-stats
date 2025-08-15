import { afterNextRender, afterRender, AfterRenderPhase, AfterViewChecked, ChangeDetectionStrategy, ChangeDetectorRef, Component, ElementRef, OnInit, ViewChild, viewChild } from '@angular/core';
import { InfiniteScrollDirective } from 'ngx-infinite-scroll';
import { RemoteFileDetail } from '../../contracts/remote-file-detail';
import { ThumbnailComponent } from '../thumbnail/thumbnail.component';
import { ActivatedRoute } from '@angular/router';
import { Subscription } from 'rxjs';
import { BaseballApiService } from '../../baseball-api.service';
import { PagedResult } from '../../contracts/paged-result';
import { MediaParams, ThumbnailParams } from '../media-gallery/media-gallery.component';
import { ThumbnailSize } from '../../contracts/thumbnail-size';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';

@Component({
    selector: 'app-thumbnail-scroll',
    standalone: true,
    imports: [
        MatButtonModule,
        MatIconModule,
        InfiniteScrollDirective,
        ThumbnailComponent
    ],
    templateUrl: './thumbnail-scroll.component.html',
    styleUrl: './thumbnail-scroll.component.scss'
})
export class ThumbnailScrollComponent implements OnInit {

    gameId?: number;
    playerId?: number;
    parkId?: number;
    get queryParams(): MediaParams {
        return {
            gameId: this.gameId,
            playerId: this.playerId,
            parkId: this.parkId
        }
    }

    private readonly pageSize = 10;

    data: RemoteFileDetail[] = [];
    totalCount?: number;
    dataLoad?: Subscription;
    loading: boolean = true;

    constructor(
        private api: BaseballApiService,
        private route: ActivatedRoute
    ) { }

    ngOnInit(): void {
        this.route.queryParams.subscribe(params => {
            this.gameId = params['gameId'];
            this.playerId = params['playerId'];
            this.parkId = params['parkId'];
            this.loadData(true);
        });
    }

    onScroll(): void {
        this.loadData(false);
    }

    get allDataLoaded(): boolean {
        return this.data.length === this.totalCount;
    }

    loadData(reset: boolean): void {
        if (reset && this.dataLoad) {
            this.clearLoad();
        } else if (this.dataLoad || (!reset && this.allDataLoaded)) {
            // already loading or nothing left to load
            return;
        }

        this.loading = true;
        let thumbnailParams: ThumbnailParams = {
            size: ThumbnailSize.small,
            gameId: this.gameId,
            playerId: this.playerId,
            parkId: this.parkId,
            take: this.pageSize
        }
        if (reset) {
            thumbnailParams.skip = 0;
        } else {
            thumbnailParams.skip = this.data.length;
        }
        this.dataLoad = this.api.makeApiGet<PagedResult<RemoteFileDetail>>(
            'media/thumbnails',
            thumbnailParams
        ).subscribe(newData => {
            this.totalCount = newData.totalCount;
            if (reset) {
                this.data = newData.results;
            } else {
                this.data.push(...newData.results);
            }
            this.clearLoad();
            this.loading = false;
        });
    }

    private clearLoad(): void {
        this.dataLoad?.unsubscribe();
        this.dataLoad = undefined;
    }
}
