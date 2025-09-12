import { Component, OnInit } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';

@Component({
  standalone: true,
  selector: 'wib-receipts-list',
  imports: [CommonModule, RouterModule],
  templateUrl: './receipts-list.component.html'
})
export class ReceiptsListComponent implements OnInit {
  receipts: any[] = [];
  constructor(private http: HttpClient) {}
  ngOnInit() {
    this.http.get<any[]>('/receipts').subscribe(r => this.receipts = r);
  }
}
