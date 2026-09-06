import { DatePipe } from '@angular/common';
import { Component, inject, OnInit, signal } from '@angular/core';
import { AccessApiService } from '../../core/access-api.service';
import { AccessRequest } from '../../models';

@Component({
  selector: 'app-provisioning',
  imports: [DatePipe],
  templateUrl: './provisioning.page.html',
})
export class ProvisioningPage implements OnInit {
  private readonly api = inject(AccessApiService);
  readonly approvedRequests = signal<AccessRequest[]>([]);
  readonly provisionedRequests = signal<AccessRequest[]>([]);
  readonly totalCount = signal(0);
  readonly error = signal('');
  page = 1;
  readonly pageSize = 10;
  ngOnInit(): void { this.load(); }
  load(): void {
    this.api.requests('Approved', this.page, this.pageSize).subscribe((x) => {
      this.approvedRequests.set(x.items);
      this.totalCount.set(x.totalCount);
    });
    this.api.requests('Provisioned', 1, 10)
      .subscribe((x) => this.provisionedRequests.set(x.items));
  }
  provision(id: number): void {
    this.api.provision(id).subscribe({ next: () => this.load(),
      error: (e) => this.error.set(e.error?.detail ?? 'Unable to provision access.') });
  }
  changePage(value: number): void {
    this.page = value;
    this.load();
  }
}
