import { Component, inject, OnInit, signal } from '@angular/core';
import { FormControl, FormRecord, ReactiveFormsModule, Validators } from '@angular/forms';
import { AccessApiService } from '../../core/access-api.service';
import { AccessRequest } from '../../models';

@Component({
  selector: 'app-approvals',
  imports: [ReactiveFormsModule],
  templateUrl: './approvals.page.html',
})
export class ApprovalsPage implements OnInit {
  private readonly api = inject(AccessApiService);
  readonly requests = signal<AccessRequest[]>([]);
  readonly totalCount = signal(0);
  readonly error = signal('');
  readonly remarks = new FormRecord<FormControl<string>>({});
  page = 1;
  readonly pageSize = 10;
  ngOnInit(): void { this.load(); }
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
      this.remarks.addControl(key, new FormControl('', {
        nonNullable: true,
        validators: [Validators.required, Validators.minLength(3), Validators.maxLength(500)],
      }));
    return this.remarks.controls[key];
  }
  decide(id: number, action: 'approve' | 'reject'): void {
    const remarks = this.remarkControl(id);
    if (remarks.invalid) {
      remarks.markAsTouched();
      return;
    }
    this.api.decide(id, action, remarks.value).subscribe({
      next: () => this.load(),
      error: (e) => this.error.set(e.error?.detail ?? 'Unable to record decision.'),
    });
  }
  changePage(value: number): void {
    this.page = value;
    this.load();
  }
}
