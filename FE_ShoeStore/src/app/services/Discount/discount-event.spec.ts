import { TestBed } from '@angular/core/testing';

import { DiscountEvent } from './discount-event';

describe('DiscountEvent', () => {
  let service: DiscountEvent;

  beforeEach(() => {
    TestBed.configureTestingModule({});
    service = TestBed.inject(DiscountEvent);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });
});
