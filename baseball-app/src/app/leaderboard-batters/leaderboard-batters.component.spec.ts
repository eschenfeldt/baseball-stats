import { ComponentFixture, TestBed } from '@angular/core/testing';

import { LeaderboardBattersComponent } from './leaderboard-batters.component';

describe('LeaderboardBattersComponent', () => {
  let component: LeaderboardBattersComponent;
  let fixture: ComponentFixture<LeaderboardBattersComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [LeaderboardBattersComponent]
    })
    .compileComponents();
    
    fixture = TestBed.createComponent(LeaderboardBattersComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
