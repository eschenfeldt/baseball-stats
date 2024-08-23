import { ComponentFixture, TestBed } from '@angular/core/testing';

import { ThumbnailScrollComponent } from './thumbnail-scroll.component';

describe('ThumbnailScrollComponent', () => {
  let component: ThumbnailScrollComponent;
  let fixture: ComponentFixture<ThumbnailScrollComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [ThumbnailScrollComponent]
    })
    .compileComponents();
    
    fixture = TestBed.createComponent(ThumbnailScrollComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
