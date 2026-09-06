import { Component, inject, OnInit, signal } from '@angular/core';
import { FormControl, FormRecord, ReactiveFormsModule, Validators } from '@angular/forms';
import { MessageService } from 'primeng/api';
import { ButtonModule } from 'primeng/button';
import { InputTextModule } from 'primeng/inputtext';
import { TableModule, TablePageEvent } from 'primeng/table';
import { AccessApiService } from '../../core/access-api.service';
import { AccessRequest } from '../../models';

@Component({
  selector: 'app-approvals',
  imports: [ReactiveFormsModule, ButtonModule, InputTextModule, TableModule],
  templateUrl: './approvals.page.html',
})
export class ApprovalsPage implements OnInit {
  private readonly api = inject(AccessApiService);
  private readonly messages = inject(MessageService);
  readonly requests = signal<AccessRequest[]>([]);
  readonly totalCount = signal(0);
  readonly remarks = new FormRecord<FormControl<string>>({});
  page = 1;
  readonly pageSize = 10;

  ngOnInit(): void {
    this.load();
  }
  load(): void {
    this.api.pendingApprovals(this.page, this.pageSize).subscribe((x) => {
      this.requests.set(x.items);
      this.totalCount.set(x.totalCount);
      for (const request of x.items) this.remarkControl(request.id);
    });
  }
  remarkControl(id: number): FormControl<string> {
    const key = id.toString();
    if (!this.remarks.contains(key))
      this.remarks.addControl(
        key,
        new FormControl('', {
          nonNullable: true,
          validators: [Validators.required, Validators.minLength(3), Validators.maxLength(500)],
        }),
      );
    return this.remarks.controls[key];
  }
  decide(id: number, action: 'approve' | 'reject'): void {
    const remarks = this.remarkControl(id);
    if (remarks.invalid) {
      remarks.markAsTouched();
      return;
    }
    this.api.decide(id, action, remarks.value).subscribe({
      next: () => {
        this.load();
        this.messages.add({
          severity: 'success',
          summary: action === 'approve' ? 'Request approved' : 'Request rejected',
          detail: 'The decision and remarks were recorded.',
        });
      },
      error: (e) =>
        this.messages.add({
          severity: 'error',
          summary: 'Action failed',
          detail: e.error?.detail ?? 'Unable to record decision.',
        }),
    });
  }
  changePage(event: TablePageEvent): void {
    this.page = event.first / event.rows + 1;
    this.load();
  }
}
