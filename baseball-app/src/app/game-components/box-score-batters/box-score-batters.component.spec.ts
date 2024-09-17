import { ComponentFixture, TestBed } from '@angular/core/testing';

import { BoxScoreBattersComponent } from './box-score-batters.component';

describe('BoxScoreBattersComponent', () => {
  let component: BoxScoreBattersComponent;
  let fixture: ComponentFixture<BoxScoreBattersComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [BoxScoreBattersComponent]
    })
    .compileComponents();
    
    fixture = TestBed.createComponent(BoxScoreBattersComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
