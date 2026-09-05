import { Component, inject, OnInit, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { AccessApiService } from '../../core/access-api.service';
import { AccessRequest } from '../../models';

@Component({ selector: 'app-approvals', imports: [FormsModule], templateUrl: './approvals.page.html' })
export class ApprovalsPage implements OnInit {
  private readonly api = inject(AccessApiService);
  readonly requests = signal<AccessRequest[]>([]);
  readonly error = signal('');
  remarks: Record<number, string> = {};
  ngOnInit(): void { this.load(); }
  load(): void { this.api.pendingApprovals().subscribe((x) => this.requests.set(x)); }
  decide(id: number, action: 'approve' | 'reject'): void {
    this.api.decide(id, action, this.remarks[id] ?? '').subscribe({
      next: () => this.load(),
      error: (e) => this.error.set(e.error?.detail ?? 'Unable to record decision.'),
    });
  }
}
