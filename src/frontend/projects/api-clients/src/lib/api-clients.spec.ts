import { ComponentFixture, TestBed } from '@angular/core/testing';
import { ApiClients } from './api-clients';

describe('ApiClients', () => {
  let component: ApiClients;
  let fixture: ComponentFixture<ApiClients>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [ApiClients],
    }).compileComponents();

    fixture = TestBed.createComponent(ApiClients);
    component = fixture.componentInstance;
    await fixture.whenStable();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
