import { CurrencyPipe } from '@angular/common';
import { Component, inject, OnInit, signal } from '@angular/core';
import { FormArray, FormControl, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { MessageService } from 'primeng/api';
import { ButtonModule } from 'primeng/button';
import { DatePickerModule } from 'primeng/datepicker';
import { InputNumberModule } from 'primeng/inputnumber';
import { SelectModule } from 'primeng/select';
import { TableModule } from 'primeng/table';
import { CustomerService } from '../../../core/services/customer.service';
import { OrderService } from '../../../core/services/order.service';
import { ProductService } from '../../../core/services/product.service';
import { Customer, OrderStatus, Product, SaveOrder } from '../../../models';

type OrderItemForm = FormGroup<{
  productId: FormControl<number>;
  quantity: FormControl<number>;
}>;

@Component({
  selector: 'app-order-edit',
  imports: [CurrencyPipe, ReactiveFormsModule, ButtonModule, DatePickerModule, InputNumberModule, SelectModule, TableModule],
  templateUrl: './order-edit.page.html',
})
export class OrderEditPage implements OnInit {
  private readonly customerService = inject(CustomerService);
  private readonly productService = inject(ProductService);
  private readonly orderService = inject(OrderService);
  private readonly route = inject(ActivatedRoute);
  private readonly router = inject(Router);
  private readonly messageService = inject(MessageService);

  readonly customers = signal<Customer[]>([]);
  readonly products = signal<Product[]>([]);
  readonly saving = signal(false);
  readonly statuses: OrderStatus[] = ['Draft', 'Confirmed', 'Shipped', 'Cancelled'];
  readonly form = new FormGroup({
    customerId: new FormControl(0, { nonNullable: true, validators: Validators.min(1) }),
    orderDate: new FormControl(new Date(), { nonNullable: true }),
    status: new FormControl<OrderStatus>('Draft', { nonNullable: true }),
    items: new FormArray<OrderItemForm>([]),
  });

  orderId?: number;

  get items(): FormArray<OrderItemForm> {
    return this.form.controls.items;
  }

  ngOnInit(): void {
    this.loadSelectOptions();
    const routeId = this.route.snapshot.paramMap.get('id');
    if (routeId) {
      this.orderId = Number(routeId);
      this.loadOrder(this.orderId);
    } else {
      this.addItem();
    }
  }

  addItem(productId = 0, quantity = 1): void {
    this.items.push(
      new FormGroup({
        productId: new FormControl(productId, {
          nonNullable: true,
          validators: Validators.min(1),
        }),
        quantity: new FormControl(quantity, {
          nonNullable: true,
          validators: Validators.min(1),
        }),
      }),
    );
  }

  removeItem(index: number): void {
    this.items.removeAt(index);
  }

  price(productId: number): number {
    return this.products().find((product) => product.id === productId)?.price ?? 0;
  }

  total(): number {
    return this.items.controls.reduce(
      (sum, row) => sum + this.price(row.controls.productId.value) * row.controls.quantity.value,
      0,
    );
  }

  save(): void {
    if (this.form.invalid || this.items.length === 0) return;

    this.saving.set(true);
    const value = this.form.getRawValue();
    const request: SaveOrder = {
      customerId: value.customerId,
      orderDateUtc: value.orderDate.toISOString(),
      status: value.status,
      items: value.items,
    };

    this.orderService.save(request, this.orderId).subscribe({
      next: () => {
        this.messageService.add({ severity: 'success', summary: 'Order saved' });
        void this.router.navigateByUrl('/admin/orders');
      },
      error: (error) => {
        this.messageService.add({
          severity: 'error',
          summary: 'Save failed',
          detail: error.error?.detail,
        });
        this.saving.set(false);
      },
    });
  }

  cancel(): void {
    void this.router.navigateByUrl('/admin/orders');
  }

  private loadSelectOptions(): void {
    this.customerService.getPage(1, 100).subscribe((result) => this.customers.set(result.items));
    this.productService.getPage(1, 100, '', true).subscribe((result) => this.products.set(result.items));
  }

  private loadOrder(id: number): void {
    this.orderService.get(id).subscribe((order) => {
      this.form.patchValue({
        customerId: order.customerId,
        orderDate: new Date(order.orderDateUtc),
        status: order.status,
      });
      order.items.forEach((item) => this.addItem(item.productId, item.quantity));
    });
  }
}
