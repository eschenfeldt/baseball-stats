import { ComponentFixture, TestBed } from '@angular/core/testing';

import { ParkComponent } from './park.component';

describe('ParkComponent', () => {
  let component: ParkComponent;
  let fixture: ComponentFixture<ParkComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [ParkComponent]
    })
    .compileComponents();

    fixture = TestBed.createComponent(ParkComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
