import { DatePipe, KeyValuePipe } from '@angular/common';
import { Component, inject, OnInit, signal } from '@angular/core';
import { AccessApiService } from '../../core/access-api.service';
import { Dashboard } from '../../models';

@Component({
  selector: 'app-dashboard',
  imports: [DatePipe, KeyValuePipe],
  templateUrl: './dashboard.page.html',
})
export class DashboardPage implements OnInit {
  private readonly api = inject(AccessApiService);
  readonly data = signal<Dashboard | null>(null);
  ngOnInit(): void { this.api.dashboard().subscribe((x) => this.data.set(x)); }
}
