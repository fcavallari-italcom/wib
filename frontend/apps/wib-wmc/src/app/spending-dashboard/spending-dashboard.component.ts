import { Component } from '@angular/core';
import { NgxChartsModule } from '@swimlane/ngx-charts';

@Component({
  standalone: true,
  selector: 'wib-spending-dashboard',
  imports: [NgxChartsModule],
  templateUrl: './spending-dashboard.component.html'
})
export class SpendingDashboardComponent {
  data = [
    { name: 'Chain A', value: 100 },
    { name: 'Chain B', value: 50 }
  ];
}
