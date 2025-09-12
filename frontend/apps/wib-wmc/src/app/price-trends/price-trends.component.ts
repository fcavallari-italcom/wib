import { Component } from '@angular/core';
import { NgxChartsModule } from '@swimlane/ngx-charts';

@Component({
  standalone: true,
  selector: 'wib-price-trends',
  imports: [NgxChartsModule],
  templateUrl: './price-trends.component.html'
})
export class PriceTrendsComponent {
  data = [
    { name: 'Item', series: [
      { name: 'Jan', value: 10 },
      { name: 'Feb', value: 12 }
    ] }
  ];
}
