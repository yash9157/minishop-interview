import { DatePipe } from '@angular/common';
import { Component, inject, OnInit, signal } from '@angular/core';
import { TableModule, TablePageEvent } from 'primeng/table';
import { AccessApiService } from '../../core/access-api.service';
import { AuditLog } from '../../models';

@Component({
  selector: 'app-audit',
  imports: [DatePipe, TableModule],
  templateUrl: './audit.page.html',
})
export class AuditPage implements OnInit {
  private readonly api = inject(AccessApiService);
  readonly logs = signal<AuditLog[]>([]);
  readonly totalCount = signal(0);
  page = 1;
  readonly pageSize = 20;

  ngOnInit(): void {
    this.load();
  }

  load(): void {
    this.api.auditLogs(this.page, this.pageSize).subscribe((result) => {
      this.logs.set(result.items);
      this.totalCount.set(result.totalCount);
    });
  }

  changePage(event: TablePageEvent): void {
    this.page = event.first / event.rows + 1;
    this.load();
  }
}
