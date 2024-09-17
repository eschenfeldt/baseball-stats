import { ComponentFixture, TestBed } from '@angular/core/testing';

import { PlayerBattingStatsComponent } from './player-batting-stats.component';

describe('PlayerBattingStatsComponent', () => {
  let component: PlayerBattingStatsComponent;
  let fixture: ComponentFixture<PlayerBattingStatsComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [PlayerBattingStatsComponent]
    })
    .compileComponents();
    
    fixture = TestBed.createComponent(PlayerBattingStatsComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
