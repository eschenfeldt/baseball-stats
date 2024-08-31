import { ComponentFixture, TestBed } from '@angular/core/testing';

import { ImportConstantsDialogComponent } from './import-constants-dialog.component';

describe('ImportConstantsDialogComponent', () => {
  let component: ImportConstantsDialogComponent;
  let fixture: ComponentFixture<ImportConstantsDialogComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [ImportConstantsDialogComponent]
    })
    .compileComponents();
    
    fixture = TestBed.createComponent(ImportConstantsDialogComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
