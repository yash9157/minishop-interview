import { Component, inject, OnInit, signal } from '@angular/core';
import { FormControl, FormGroup, FormsModule, ReactiveFormsModule, Validators } from '@angular/forms';
import { ConfirmationService, MessageService } from 'primeng/api';
import { ButtonModule } from 'primeng/button';
import { DialogModule } from 'primeng/dialog';
import { InputTextModule } from 'primeng/inputtext';
import { TableModule } from 'primeng/table';
import { CustomerService } from '../../core/services/customer.service';
import { Customer } from '../../models';

@Component({
  selector: 'app-customers',
  imports: [FormsModule, ReactiveFormsModule, ButtonModule, DialogModule, InputTextModule, TableModule],
  templateUrl: './customers.page.html',
})
export class CustomersPage implements OnInit {
  private readonly customerService = inject(CustomerService);
  private readonly messageService = inject(MessageService);
  private readonly confirmationService = inject(ConfirmationService);

  readonly items = signal<Customer[]>([]);
  readonly total = signal(0);
  readonly loading = signal(false);
  readonly form = new FormGroup({
    name: new FormControl('', { nonNullable: true, validators: Validators.required }),
    email: new FormControl('', {
      nonNullable: true,
      validators: [Validators.required, Validators.email],
    }),
    phone: new FormControl('', { nonNullable: true }),
  });

  dialogVisible = false;
  selectedId?: number;
  search = '';
  pageNumber = 1;

  ngOnInit(): void {
    this.load();
  }

  page(event: { first?: number | null; rows?: number | null }): void {
    this.pageNumber = Math.floor((event.first ?? 0) / (event.rows ?? 10)) + 1;
    this.load();
  }

  load(): void {
    this.loading.set(true);
    this.customerService.getPage(this.pageNumber, 10, this.search).subscribe({
      next: (result) => {
        this.items.set(result.items);
        this.total.set(result.totalCount);
        this.loading.set(false);
      },
      error: () => this.loading.set(false),
    });
  }

  open(customer?: Customer): void {
    this.selectedId = customer?.id;
    this.form.reset({
      name: customer?.name ?? '',
      email: customer?.email ?? '',
      phone: customer?.phone ?? '',
    });
    this.dialogVisible = true;
  }

  save(): void {
    if (this.form.invalid) return;

    this.customerService.save(this.form.getRawValue(), this.selectedId).subscribe({
      next: () => {
        this.dialogVisible = false;
        this.messageService.add({ severity: 'success', summary: 'Customer saved' });
        this.load();
      },
      error: (error) => this.showError('Save failed', error.error?.detail),
    });
  }

  remove(customer: Customer): void {
    this.confirmationService.confirm({
      message: `Delete ${customer.name}?`,
      accept: () =>
        this.customerService.delete(customer.id).subscribe({
          next: () => this.load(),
          error: (error) => this.showError('Delete failed', error.error?.detail),
        }),
    });
  }

  private showError(summary: string, detail?: string): void {
    this.messageService.add({ severity: 'error', summary, detail });
  }
}
