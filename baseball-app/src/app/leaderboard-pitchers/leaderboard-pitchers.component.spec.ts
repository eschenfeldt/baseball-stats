import { ComponentFixture, TestBed } from '@angular/core/testing';

import { LeaderboardPitchersComponent } from './leaderboard-pitchers.component';

describe('LeaderboardPitchersComponent', () => {
  let component: LeaderboardPitchersComponent;
  let fixture: ComponentFixture<LeaderboardPitchersComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [LeaderboardPitchersComponent]
    })
    .compileComponents();
    
    fixture = TestBed.createComponent(LeaderboardPitchersComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
