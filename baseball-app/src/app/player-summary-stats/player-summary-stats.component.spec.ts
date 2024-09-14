import { ComponentFixture, TestBed } from '@angular/core/testing';

import { PlayerSummaryStatsComponent } from './player-summary-stats.component';

describe('PlayerSummaryStatsComponent', () => {
  let component: PlayerSummaryStatsComponent;
  let fixture: ComponentFixture<PlayerSummaryStatsComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [PlayerSummaryStatsComponent]
    })
    .compileComponents();
    
    fixture = TestBed.createComponent(PlayerSummaryStatsComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
