import { ComponentFixture, TestBed } from '@angular/core/testing';

import { DiscountEventDetail } from './discount-event-detail';

describe('DiscountEventDetail', () => {
  let component: DiscountEventDetail;
  let fixture: ComponentFixture<DiscountEventDetail>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [DiscountEventDetail]
    })
    .compileComponents();

    fixture = TestBed.createComponent(DiscountEventDetail);
    component = fixture.componentInstance;
    await fixture.whenStable();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
