import { AfterViewChecked, AfterViewInit, Component, Input, OnInit, ViewChild } from '@angular/core';
import { MatPaginator, MatPaginatorModule } from '@angular/material/paginator';
import { combineLatest, merge, Observable, startWith, switchMap } from 'rxjs';
import { PagedResult } from '../../contracts/paged-result';
import { RemoteFileDetail } from '../../contracts/remote-file-detail';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { AsyncPipe } from '@angular/common';
import { PagedApiParameters } from '../../paged-api-parameters';
import { BaseballApiService } from '../../baseball-api.service';
import { ThumbnailComponent } from '../thumbnail/thumbnail.component';
import { BreakpointObserver, Breakpoints, BreakpointState } from '@angular/cdk/layout';
import { ThumbnailSize } from '../../contracts/thumbnail-size';

export interface MediaParams extends PagedApiParameters {
    gameId?: number,
    playerId?: number,
}

export interface ThumbnailParams extends MediaParams {
    size: ThumbnailSize
}

@Component({
    selector: 'app-media-gallery',
    standalone: true,
    imports: [
        MatPaginatorModule,
        MatProgressSpinnerModule,
        AsyncPipe,
        ThumbnailComponent
    ],
    templateUrl: './media-gallery.component.html',
    styleUrl: './media-gallery.component.scss'
})
export class MediaGalleryComponent implements OnInit, AfterViewInit {

    @Input()
    gameId?: number;

    @Input()
    playerId?: number;

    @ViewChild(MatPaginator)
    private paginator!: MatPaginator;
    private breakpoints$?: Observable<BreakpointState>;

    data$?: Observable<PagedResult<RemoteFileDetail>>
    defaultPageSize = 20;
    thumbnailSize: ThumbnailSize = ThumbnailSize.small;

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

    private load(): Observable<PagedResult<RemoteFileDetail>> {
        let params: ThumbnailParams = {
            gameId: this.gameId,
            playerId: this.playerId,
            size: this.thumbnailSize
        };
        if (this.paginator) {
            params.skip = this.paginator.pageIndex * this.paginator.pageSize;
            params.take = this.paginator.pageSize;
        } else {
            params.skip = 0;
            params.take = this.defaultPageSize;
        }
        return this.api.makeApiGet<PagedResult<RemoteFileDetail>>('media/thumbnails', params);
    }

    private registerLoad(): void {
        if (this.paginator && this.breakpoints$) {
            this.data$ = combineLatest([
                this.paginator.page.pipe(startWith(null)),
                this.breakpoints$.pipe(startWith(null))
            ]).pipe(
                switchMap(([, breakpointState], _) => {
                    if (breakpointState) {
                        this.updateSize(breakpointState);
                    }
                    return this.load();
                })
            );
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
            playerId: this.playerId
        };
    }
}
