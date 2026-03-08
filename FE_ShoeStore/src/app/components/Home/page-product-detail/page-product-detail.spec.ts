import { ComponentFixture, TestBed } from '@angular/core/testing';

import { PageProductDetail } from './page-product-detail';

describe('PageProductDetail', () => {
  let component: PageProductDetail;
  let fixture: ComponentFixture<PageProductDetail>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [PageProductDetail]
    })
    .compileComponents();

    fixture = TestBed.createComponent(PageProductDetail);
    component = fixture.componentInstance;
    await fixture.whenStable();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
