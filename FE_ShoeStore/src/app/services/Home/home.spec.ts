import { TestBed } from '@angular/core/testing';

import { Home } from '../Home/home';

describe('Home', () => {
  let service: Home;

  beforeEach(() => {
    TestBed.configureTestingModule({});
    service = TestBed.inject(Home);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });
});
