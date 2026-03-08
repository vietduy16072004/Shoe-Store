import { ComponentFixture, TestBed } from '@angular/core/testing';

import { PageDiscountEvent } from './page-discount-event';

describe('PageDiscountEvent', () => {
  let component: PageDiscountEvent;
  let fixture: ComponentFixture<PageDiscountEvent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [PageDiscountEvent]
    })
    .compileComponents();

    fixture = TestBed.createComponent(PageDiscountEvent);
    component = fixture.componentInstance;
    await fixture.whenStable();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
