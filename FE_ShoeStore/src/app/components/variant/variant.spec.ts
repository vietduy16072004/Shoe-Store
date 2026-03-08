import { ComponentFixture, TestBed } from '@angular/core/testing';

import { Variant } from './variant';

describe('Variant', () => {
  let component: Variant;
  let fixture: ComponentFixture<Variant>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [Variant]
    })
    .compileComponents();

    fixture = TestBed.createComponent(Variant);
    component = fixture.componentInstance;
    await fixture.whenStable();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
