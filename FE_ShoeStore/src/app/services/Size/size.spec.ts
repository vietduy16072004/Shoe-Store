import { TestBed } from '@angular/core/testing';

import { Size } from './size';

describe('Size', () => {
  let service: Size;

  beforeEach(() => {
    TestBed.configureTestingModule({});
    service = TestBed.inject(Size);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });
});
