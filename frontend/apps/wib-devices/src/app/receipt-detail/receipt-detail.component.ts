import { Component, OnInit } from '@angular/core';
import { ActivatedRoute } from '@angular/router';
import { HttpClient } from '@angular/common/http';
import { CommonModule } from '@angular/common';

@Component({
  standalone: true,
  selector: 'wib-receipt-detail',
  imports: [CommonModule],
  templateUrl: './receipt-detail.component.html'
})
export class ReceiptDetailComponent implements OnInit {
  receipt: any;
  constructor(private http: HttpClient, private route: ActivatedRoute) {}
  ngOnInit() {
    const id = this.route.snapshot.paramMap.get('id');
    this.http.get('/receipts/' + id).subscribe(r => this.receipt = r);
  }
  setType(line: any, type: string) { line.type = type; }
  setCategory(line: any, cat: string) { line.category = cat; }
  confirm() {
    this.http.post(`/receipts/${this.receipt.id}/confirm`, this.receipt).subscribe();
  }
}
