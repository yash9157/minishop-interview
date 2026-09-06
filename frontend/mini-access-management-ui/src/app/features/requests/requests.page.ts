import { DatePipe } from '@angular/common';
import { Component, inject, OnInit, signal } from '@angular/core';
import { FormControl, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { AccessApiService } from '../../core/access-api.service';
import { AccessRequest, Role, TargetSystem } from '../../models';

@Component({
  selector: 'app-requests',
  imports: [ReactiveFormsModule, DatePipe],
  templateUrl: './requests.page.html',
})
export class RequestsPage implements OnInit {
  private readonly api = inject(AccessApiService);
  readonly requests = signal<AccessRequest[]>([]);
  readonly systems = signal<TargetSystem[]>([]);
  readonly roles = signal<Role[]>([]);
  readonly totalCount = signal(0);
  readonly error = signal('');
  page = 1;
  readonly pageSize = 10;
  readonly form = new FormGroup({
    targetSystemId: new FormControl(0, { nonNullable: true, validators: Validators.min(1) }),
    requestedRoleId: new FormControl('', { nonNullable: true, validators: Validators.required }),
    businessJustification: new FormControl('', {
      nonNullable: true, validators: [Validators.required, Validators.minLength(10)],
    }),
  });
  ngOnInit(): void {
    this.api.systems().subscribe((x) => this.systems.set(x));
    this.api.roles().subscribe((x) =>
      this.roles.set(x.items.filter((role) => role.isRequestable)));
    this.load();
  }
  load(): void {
    this.api.myRequests(this.page, this.pageSize).subscribe((x) => {
      this.requests.set(x.items);
      this.totalCount.set(x.totalCount);
    });
  }
  create(): void {
    if (this.form.invalid) return;
    this.api.createRequest(this.form.getRawValue()).subscribe({
      next: () => { this.form.reset({ targetSystemId: 0, requestedRoleId: '' }); this.load(); },
      error: (e) => this.error.set(e.error?.detail ?? 'Unable to create request.'),
    });
  }
  submit(id: number): void {
    this.api.submitRequest(id).subscribe({ next: () => this.load(),
      error: (e) => this.error.set(e.error?.detail ?? 'Unable to submit request.') });
  }
  changePage(value: number): void {
    this.page = value;
    this.load();
  }
}
