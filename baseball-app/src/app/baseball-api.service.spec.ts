import { TestBed } from '@angular/core/testing';

import { BaseballApiService } from './baseball-api.service';

describe('BaseballApiService', () => {
  let service: BaseballApiService;

  beforeEach(() => {
    TestBed.configureTestingModule({});
    service = TestBed.inject(BaseballApiService);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });
});
