import { CurrencyPipe, DatePipe } from '@angular/common';
import { Component, inject, OnInit, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { ConfirmationService, MessageService } from 'primeng/api';
import { ButtonModule } from 'primeng/button';
import { InputTextModule } from 'primeng/inputtext';
import { SelectModule } from 'primeng/select';
import { TableModule } from 'primeng/table';
import { TagModule } from 'primeng/tag';
import { OrderService } from '../../../core/services/order.service';
import { OrderStatus, OrderSummary } from '../../../models';

@Component({
  selector: 'app-orders',
  imports: [CurrencyPipe, DatePipe, FormsModule, ButtonModule, InputTextModule, SelectModule, TableModule, TagModule],
  templateUrl: './orders.page.html',
})
export class OrdersPage implements OnInit {
  private readonly orderService = inject(OrderService);
  private readonly router = inject(Router);
  private readonly confirmationService = inject(ConfirmationService);
  private readonly messageService = inject(MessageService);

  readonly items = signal<OrderSummary[]>([]);
  readonly total = signal(0);
  readonly loading = signal(false);
  readonly statuses: OrderStatus[] = ['Draft', 'Confirmed', 'Shipped', 'Cancelled'];
  search = '';
  status?: OrderStatus;
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
    this.orderService.getPage(this.pageNumber, 10, this.search, this.status).subscribe({
      next: (result) => {
        this.items.set(result.items);
        this.total.set(result.totalCount);
        this.loading.set(false);
      },
      error: () => this.loading.set(false),
    });
  }

  newOrder(): void {
    void this.router.navigateByUrl('/admin/orders/new');
  }

  edit(id: number): void {
    void this.router.navigate(['/admin/orders', id]);
  }

  remove(order: OrderSummary): void {
    this.confirmationService.confirm({
      message: `Delete ${order.orderNumber}?`,
      accept: () =>
        this.orderService.delete(order.id).subscribe({
          next: () => this.load(),
          error: (error) =>
            this.messageService.add({
              severity: 'error',
              summary: 'Delete failed',
              detail: error.error?.detail,
            }),
        }),
    });
  }
}
