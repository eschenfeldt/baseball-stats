import { ComponentFixture, TestBed } from '@angular/core/testing';

import { ImportScorecardDialogComponent } from './import-scorecard-dialog.component';

describe('ImportScorecardDialogComponent', () => {
  let component: ImportScorecardDialogComponent;
  let fixture: ComponentFixture<ImportScorecardDialogComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [ImportScorecardDialogComponent]
    })
    .compileComponents();
    
    fixture = TestBed.createComponent(ImportScorecardDialogComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
