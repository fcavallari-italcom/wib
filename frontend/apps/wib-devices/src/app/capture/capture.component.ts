import { Component } from '@angular/core';
import { HttpClient, HttpEventType } from '@angular/common/http';
import { CommonModule } from '@angular/common';

@Component({
  standalone: true,
  selector: 'wib-capture',
  imports: [CommonModule],
  templateUrl: './capture.component.html'
})
export class CaptureComponent {
  progress = 0;
  constructor(private http: HttpClient) {}

  onFile(event: Event) {
    const file = (event.target as HTMLInputElement).files?.[0];
    if (!file) return;
    const reader = new FileReader();
    reader.onload = (e: any) => {
      const img = new Image();
      img.src = e.target.result;
      img.onload = () => {
        const canvas = document.createElement('canvas');
        canvas.width = img.width;
        canvas.height = img.height;
        const ctx = canvas.getContext('2d')!;
        ctx.drawImage(img, 0, 0);
        canvas.toBlob(blob => {
          if (blob) {
            const form = new FormData();
            form.append('file', blob, file.name);
            this.http.post('/receipts', form, { reportProgress: true, observe: 'events' })
              .subscribe(ev => {
                if (ev.type === HttpEventType.UploadProgress && ev.total) {
                  this.progress = Math.round(100 * ev.loaded / ev.total);
                }
              });
          }
        }, 'image/jpeg', 0.8);
      };
    };
    reader.readAsDataURL(file);
  }
}
