import { CurrencyPipe } from '@angular/common';
import { Component, inject, OnInit, signal } from '@angular/core';
import { FormControl, FormGroup, FormsModule, ReactiveFormsModule, Validators } from '@angular/forms';
import { ConfirmationService, MessageService } from 'primeng/api';
import { ButtonModule } from 'primeng/button';
import { CheckboxModule } from 'primeng/checkbox';
import { DialogModule } from 'primeng/dialog';
import { InputNumberModule } from 'primeng/inputnumber';
import { InputTextModule } from 'primeng/inputtext';
import { SelectModule } from 'primeng/select';
import { TableModule } from 'primeng/table';
import { TagModule } from 'primeng/tag';
import { CategoryService } from '../../core/services/category.service';
import { ProductService } from '../../core/services/product.service';
import { Category, Product } from '../../models';

@Component({
  selector: 'app-products',
  imports: [
    CurrencyPipe,
    FormsModule,
    ReactiveFormsModule,
    ButtonModule,
    CheckboxModule,
    DialogModule,
    InputNumberModule,
    InputTextModule,
    SelectModule,
    TableModule,
    TagModule,
  ],
  templateUrl: './products.page.html',
})
export class ProductsPage implements OnInit {
  private readonly categoryService = inject(CategoryService);
  private readonly productService = inject(ProductService);
  private readonly messageService = inject(MessageService);
  private readonly confirmationService = inject(ConfirmationService);

  readonly items = signal<Product[]>([]);
  readonly categories = signal<Category[]>([]);
  readonly total = signal(0);
  readonly loading = signal(false);
  readonly form = new FormGroup({
    categoryId: new FormControl(0, { nonNullable: true, validators: Validators.min(1) }),
    sku: new FormControl('', { nonNullable: true, validators: Validators.required }),
    name: new FormControl('', { nonNullable: true, validators: Validators.required }),
    price: new FormControl(0, { nonNullable: true, validators: Validators.min(0.01) }),
    stockQuantity: new FormControl(0, { nonNullable: true, validators: Validators.min(0) }),
    isActive: new FormControl(true, { nonNullable: true }),
  });

  dialogVisible = false;
  selectedId?: number;
  search = '';
  pageNumber = 1;

  ngOnInit(): void {
    this.categoryService.getAll().subscribe((categories) => this.categories.set(categories));
    this.load();
  }

  page(event: { first?: number | null; rows?: number | null }): void {
    this.pageNumber = Math.floor((event.first ?? 0) / (event.rows ?? 10)) + 1;
    this.load();
  }

  load(): void {
    this.loading.set(true);
    this.productService.getPage(this.pageNumber, 10, this.search).subscribe({
      next: (result) => {
        this.items.set(result.items);
        this.total.set(result.totalCount);
        this.loading.set(false);
      },
      error: () => this.loading.set(false),
    });
  }

  open(product?: Product): void {
    this.selectedId = product?.id;
    this.form.reset({
      categoryId: product?.categoryId ?? 0,
      sku: product?.sku ?? '',
      name: product?.name ?? '',
      price: product?.price ?? 0,
      stockQuantity: product?.stockQuantity ?? 0,
      isActive: product?.isActive ?? true,
    });
    this.dialogVisible = true;
  }

  save(): void {
    if (this.form.invalid) return;

    this.productService.save(this.form.getRawValue(), this.selectedId).subscribe({
      next: () => {
        this.dialogVisible = false;
        this.messageService.add({ severity: 'success', summary: 'Product saved' });
        this.load();
      },
      error: (error) => this.showError('Save failed', error.error?.detail),
    });
  }

  remove(product: Product): void {
    this.confirmationService.confirm({
      message: `Delete ${product.name}?`,
      accept: () =>
        this.productService.delete(product.id).subscribe({
          next: () => this.load(),
          error: (error) => this.showError('Delete failed', error.error?.detail),
        }),
    });
  }

  private showError(summary: string, detail?: string): void {
    this.messageService.add({ severity: 'error', summary, detail });
  }
}
