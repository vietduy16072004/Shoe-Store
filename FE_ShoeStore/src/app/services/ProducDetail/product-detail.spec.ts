import { TestBed } from '@angular/core/testing';

import { ProductDetail } from './product-detail';

describe('ProductDetail', () => {
  let service: ProductDetail;

  beforeEach(() => {
    TestBed.configureTestingModule({});
    service = TestBed.inject(ProductDetail);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });
});
