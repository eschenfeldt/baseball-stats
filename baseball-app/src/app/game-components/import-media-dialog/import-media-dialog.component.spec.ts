import { ComponentFixture, TestBed } from '@angular/core/testing';

import { ImportMediaDialogComponent } from './import-media-dialog.component';

describe('ImportMediaDialogComponent', () => {
  let component: ImportMediaDialogComponent;
  let fixture: ComponentFixture<ImportMediaDialogComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [ImportMediaDialogComponent]
    })
    .compileComponents();

    fixture = TestBed.createComponent(ImportMediaDialogComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
