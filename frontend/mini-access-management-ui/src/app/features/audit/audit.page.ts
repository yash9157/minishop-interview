import { DatePipe } from '@angular/common';
import { Component, inject, OnInit, signal } from '@angular/core';
import { AccessApiService } from '../../core/access-api.service';
import { AuditLog } from '../../models';

@Component({
  selector: 'app-audit',
  imports: [DatePipe],
  templateUrl: './audit.page.html',
})
export class AuditPage implements OnInit {
  private readonly api = inject(AccessApiService);
  readonly logs = signal<AuditLog[]>([]);
  readonly totalCount = signal(0);
  page = 1;

  ngOnInit(): void { this.load(); }

  load(): void {
    this.api.auditLogs(this.page).subscribe((result) => {
      this.logs.set(result.items);
      this.totalCount.set(result.totalCount);
    });
  }

  changePage(value: number): void {
    this.page = value;
    this.load();
  }
}
