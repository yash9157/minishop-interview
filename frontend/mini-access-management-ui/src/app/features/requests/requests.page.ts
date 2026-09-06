import { DatePipe } from '@angular/common';
import { Component, inject, OnInit, signal } from '@angular/core';
import { FormControl, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { MessageService } from 'primeng/api';
import { ButtonModule } from 'primeng/button';
import { DialogModule } from 'primeng/dialog';
import { SelectModule } from 'primeng/select';
import { TableModule, TablePageEvent } from 'primeng/table';
import { TextareaModule } from 'primeng/textarea';
import { AccessApiService } from '../../core/access-api.service';
import { AccessRequest, Role, TargetSystem } from '../../models';

@Component({
  selector: 'app-requests',
  imports: [
    ReactiveFormsModule,
    DatePipe,
    ButtonModule,
    DialogModule,
    SelectModule,
    TableModule,
    TextareaModule,
  ],
  templateUrl: './requests.page.html',
})
export class RequestsPage implements OnInit {
  private readonly api = inject(AccessApiService);
  private readonly messages = inject(MessageService);
  readonly requests = signal<AccessRequest[]>([]);
  readonly systems = signal<TargetSystem[]>([]);
  readonly roles = signal<Role[]>([]);
  readonly totalCount = signal(0);
  readonly createOpen = signal(false);
  readonly saving = signal(false);
  page = 1;
  readonly pageSize = 10;

  readonly form = new FormGroup({
    targetSystemId: new FormControl(0, { nonNullable: true, validators: Validators.min(1) }),
    requestedRoleId: new FormControl('', { nonNullable: true, validators: Validators.required }),
    businessJustification: new FormControl('', {
      nonNullable: true,
      validators: [Validators.required, Validators.minLength(10)],
    }),
  });
  ngOnInit(): void {
    this.api.systems().subscribe((x) => this.systems.set(x));
    this.api.roles().subscribe((x) => this.roles.set(x.items.filter((role) => role.isRequestable)));
    this.load();
  }
  load(): void {
    this.api.myRequests(this.page, this.pageSize).subscribe((x) => {
      this.requests.set(x.items);
      this.totalCount.set(x.totalCount);
    });
  }
  openCreate(): void {
    this.form.reset({ targetSystemId: 0, requestedRoleId: '', businessJustification: '' });
    this.createOpen.set(true);
  }
  create(): void {
    if (this.form.invalid) return;
    this.saving.set(true);
    this.api.createRequest(this.form.getRawValue()).subscribe({
      next: () => {
        this.saving.set(false);
        this.createOpen.set(false);
        this.load();
        this.messages.add({
          severity: 'success',
          summary: 'Draft saved',
          detail: 'Your access request is ready to submit.',
        });
      },
      error: (e) => this.showError(e, 'Unable to create request.'),
    });
  }
  submit(id: number): void {
    this.api.submitRequest(id).subscribe({
      next: () => {
        this.load();
        this.messages.add({
          severity: 'success',
          summary: 'Request submitted',
          detail: 'The request was sent for approval.',
        });
      },
      error: (e) => this.showError(e, 'Unable to submit request.'),
    });
  }
  changePage(event: TablePageEvent): void {
    this.page = event.first / event.rows + 1;
    this.load();
  }
  private showError(error: { error?: { detail?: string } }, fallback: string): void {
    this.saving.set(false);
    this.messages.add({
      severity: 'error',
      summary: 'Action failed',
      detail: error.error?.detail ?? fallback,
    });
  }
}
