import { ComponentFixture, TestBed } from '@angular/core/testing';

import { DiscountEvents } from './discount-events';

describe('DiscountEvents', () => {
  let component: DiscountEvents;
  let fixture: ComponentFixture<DiscountEvents>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [DiscountEvents]
    })
    .compileComponents();

    fixture = TestBed.createComponent(DiscountEvents);
    component = fixture.componentInstance;
    await fixture.whenStable();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
