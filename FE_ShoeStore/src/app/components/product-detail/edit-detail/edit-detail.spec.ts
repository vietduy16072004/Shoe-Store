import { ComponentFixture, TestBed } from '@angular/core/testing';

import { EditDetail } from './edit-detail';

describe('EditDetail', () => {
  let component: EditDetail;
  let fixture: ComponentFixture<EditDetail>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [EditDetail]
    })
    .compileComponents();

    fixture = TestBed.createComponent(EditDetail);
    component = fixture.componentInstance;
    await fixture.whenStable();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
