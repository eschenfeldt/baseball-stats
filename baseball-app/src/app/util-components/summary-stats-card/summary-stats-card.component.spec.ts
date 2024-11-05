import { ComponentFixture, TestBed } from '@angular/core/testing';

import { SummaryStatsCardComponent } from './summary-stats-card.component';

describe('SummaryStatsCardComponent', () => {
  let component: SummaryStatsCardComponent;
  let fixture: ComponentFixture<SummaryStatsCardComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [SummaryStatsCardComponent]
    })
    .compileComponents();

    fixture = TestBed.createComponent(SummaryStatsCardComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
