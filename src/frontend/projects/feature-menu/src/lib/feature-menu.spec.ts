import { ComponentFixture, TestBed } from '@angular/core/testing';
import { FeatureMenu } from './feature-menu';

describe('FeatureMenu', () => {
  let component: FeatureMenu;
  let fixture: ComponentFixture<FeatureMenu>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [FeatureMenu],
    }).compileComponents();

    fixture = TestBed.createComponent(FeatureMenu);
    component = fixture.componentInstance;
    await fixture.whenStable();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
