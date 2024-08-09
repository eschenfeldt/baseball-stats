import { ComponentFixture, TestBed } from '@angular/core/testing';

import { BoxScorePitchersComponent } from './box-score-pitchers.component';

describe('BoxScorePitchersComponent', () => {
  let component: BoxScorePitchersComponent;
  let fixture: ComponentFixture<BoxScorePitchersComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [BoxScorePitchersComponent]
    })
    .compileComponents();
    
    fixture = TestBed.createComponent(BoxScorePitchersComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
