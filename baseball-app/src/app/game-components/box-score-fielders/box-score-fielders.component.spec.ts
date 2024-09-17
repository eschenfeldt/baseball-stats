import { ComponentFixture, TestBed } from '@angular/core/testing';

import { BoxScoreFieldersComponent } from './box-score-fielders.component';

describe('BoxScoreFieldersComponent', () => {
  let component: BoxScoreFieldersComponent;
  let fixture: ComponentFixture<BoxScoreFieldersComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [BoxScoreFieldersComponent]
    })
    .compileComponents();
    
    fixture = TestBed.createComponent(BoxScoreFieldersComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
