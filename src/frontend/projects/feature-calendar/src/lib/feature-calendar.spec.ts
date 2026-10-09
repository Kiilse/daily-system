import { ComponentFixture, TestBed } from '@angular/core/testing';
import { FeatureCalendar } from './feature-calendar';

describe('FeatureCalendar', () => {
  let component: FeatureCalendar;
  let fixture: ComponentFixture<FeatureCalendar>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [FeatureCalendar],
    }).compileComponents();

    fixture = TestBed.createComponent(FeatureCalendar);
    component = fixture.componentInstance;
    await fixture.whenStable();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
