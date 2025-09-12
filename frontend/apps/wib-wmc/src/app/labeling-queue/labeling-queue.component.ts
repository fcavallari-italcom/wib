import { Component, OnInit } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { CommonModule } from '@angular/common';

@Component({
  standalone: true,
  selector: 'wib-labeling-queue',
  imports: [CommonModule],
  templateUrl: './labeling-queue.component.html'
})
export class LabelingQueueComponent implements OnInit {
  receipts: any[] = [];
  selected = new Set<number>();
  constructor(private http: HttpClient) {}
  ngOnInit() {
    this.http.get<any[]>('/receipts/pending').subscribe(r => this.receipts = r);
  }
  toggle(id: number) {
    this.selected.has(id) ? this.selected.delete(id) : this.selected.add(id);
  }
  confirm() {
    this.http.post('/receipts/bulk-confirm', { ids: Array.from(this.selected) }).subscribe();
  }
}
