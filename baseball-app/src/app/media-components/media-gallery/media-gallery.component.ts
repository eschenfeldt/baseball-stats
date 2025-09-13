import { AfterViewInit, Component, Input, OnInit } from '@angular/core';
import { Observable, Subscription } from 'rxjs';
import { PagedResult } from '../../contracts/paged-result';
import { RemoteFileDetail } from '../../contracts/remote-file-detail';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { PagedApiParameters } from '../../paged-api-parameters';
import { BaseballApiService } from '../../baseball-api.service';
import { ThumbnailComponent } from '../thumbnail/thumbnail.component';
import { BreakpointObserver, Breakpoints, BreakpointState } from '@angular/cdk/layout';
import { ThumbnailSize } from '../../contracts/thumbnail-size';
import { InfiniteScrollDirective } from 'ngx-infinite-scroll';
import { MatIcon } from '@angular/material/icon';

export interface MediaParams extends PagedApiParameters {
    gameId?: number,
    playerId?: number,
    parkId?: number
}

export interface ThumbnailParams extends MediaParams {
    size: ThumbnailSize
}

@Component({
    selector: 'app-media-gallery',
    imports: [
        MatProgressSpinnerModule,
        ThumbnailComponent,
        InfiniteScrollDirective,
        MatIcon
    ],
    templateUrl: './media-gallery.component.html',
    styleUrl: './media-gallery.component.scss'
})
export class MediaGalleryComponent implements OnInit, AfterViewInit {

    @Input()
    gameId?: number;

    @Input()
    playerId?: number;

    @Input()
    parkId?: number;

    private breakpoints$?: Observable<BreakpointState>;

    data: RemoteFileDetail[] = [];
    thumbnailSize: ThumbnailSize = ThumbnailSize.small;
    dataLoad?: Subscription;
    totalCount?: number;
    loading: boolean = true;
    private readonly pageSize = 30;

    constructor(
        private api: BaseballApiService,
        private breakpointObserver: BreakpointObserver
    ) { }

    ngOnInit(): void {
        this.breakpoints$ = this.breakpointObserver.observe([
            Breakpoints.Small,
            Breakpoints.Medium,
            Breakpoints.XLarge,
        ]);
    }

    ngAfterViewInit(): void {
        if (this.breakpointObserver.isMatched(Breakpoints.Medium) || this.breakpointObserver.isMatched(Breakpoints.Large)) {
            this.thumbnailSize = ThumbnailSize.medium;
        } else if (this.breakpointObserver.isMatched(Breakpoints.XLarge)) {
            this.thumbnailSize = ThumbnailSize.large;
        }
        this.registerLoad();
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
            gameId: this.gameId,
            playerId: this.playerId,
            parkId: this.parkId,
            size: this.thumbnailSize,
            take: this.pageSize
        };
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

    private registerLoad(): void {
        if (this.breakpoints$) {
            this.breakpoints$.subscribe(breakpointState => {
                if (breakpointState) {
                    this.updateSize(breakpointState);
                    this.loadData(true);
                }
            });
        }
    }

    private updateSize(state: BreakpointState): void {
        if (state.breakpoints[Breakpoints.Small]) {
            this.thumbnailSize = ThumbnailSize.small;
        } else if (state.breakpoints[Breakpoints.Medium]) {
            this.thumbnailSize = ThumbnailSize.medium;
        } else if (state.breakpoints[Breakpoints.Large]) {
            this.thumbnailSize = ThumbnailSize.large;
        }
    }

    get mediaQueryParams(): MediaParams {
        return {
            gameId: this.gameId,
            playerId: this.playerId,
            parkId: this.parkId
        };
    }
}
