import { CurrencyPipe } from '@angular/common';
import { Component, inject, OnDestroy, OnInit, signal } from '@angular/core';
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
export class ProductsPage implements OnInit, OnDestroy {
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
  imageDialogVisible = false;
  imageLoading = false;
  selectedId?: number;
  imageProduct?: Product;
  selectedImage?: File;
  imageUrl?: string;
  search = '';
  pageNumber = 1;

  ngOnInit(): void {
    this.categoryService.getAll().subscribe((categories) => this.categories.set(categories));
    this.load();
  }

  ngOnDestroy(): void {
    this.clearImageUrl();
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

  openImage(product: Product): void {
    this.clearImageUrl();
    this.imageProduct = product;
    this.selectedImage = undefined;
    this.imageDialogVisible = true;

    if (product.hasImage) this.loadImage(product.id);
  }

  selectImage(event: Event): void {
    const input = event.target as HTMLInputElement;
    const file = input.files?.[0];
    if (!file) return;

    const allowedTypes = ['image/jpeg', 'image/png', 'image/webp'];
    if (!allowedTypes.includes(file.type) || file.size > 5 * 1024 * 1024) {
      input.value = '';
      this.selectedImage = undefined;
      this.showError('Invalid image', 'Use a JPG, PNG, or WebP image up to 5 MB.');
      return;
    }

    this.selectedImage = file;
    this.clearImageUrl();
    this.imageUrl = URL.createObjectURL(file);
  }

  uploadImage(): void {
    if (!this.imageProduct || !this.selectedImage) return;

    this.imageLoading = true;
    this.productService.uploadImage(this.imageProduct.id, this.selectedImage).subscribe({
      next: () => {
        this.imageLoading = false;
        this.selectedImage = undefined;
        this.imageProduct = { ...this.imageProduct!, hasImage: true };
        this.messageService.add({ severity: 'success', summary: 'Product image saved' });
        this.load();
        this.loadImage(this.imageProduct.id);
      },
      error: (error) => {
        this.imageLoading = false;
        this.showError('Upload failed', error.error?.detail);
      },
    });
  }

  removeImage(): void {
    if (!this.imageProduct) return;

    this.confirmationService.confirm({
      message: `Remove the image for ${this.imageProduct.name}?`,
      accept: () => {
        this.imageLoading = true;
        this.productService.deleteImage(this.imageProduct!.id).subscribe({
          next: () => {
            this.imageLoading = false;
            this.imageDialogVisible = false;
            this.clearImageUrl();
            this.messageService.add({ severity: 'success', summary: 'Product image removed' });
            this.load();
          },
          error: (error) => {
            this.imageLoading = false;
            this.showError('Delete failed', error.error?.detail);
          },
        });
      },
    });
  }

  closeImageDialog(): void {
    this.imageDialogVisible = false;
    this.selectedImage = undefined;
    this.clearImageUrl();
  }

  private loadImage(productId: number): void {
    this.imageLoading = true;
    this.productService.getImage(productId).subscribe({
      next: (blob) => {
        this.clearImageUrl();
        this.imageUrl = URL.createObjectURL(blob);
        this.imageLoading = false;
      },
      error: (error) => {
        this.imageLoading = false;
        this.showError('Image load failed', error.error?.detail);
      },
    });
  }

  private clearImageUrl(): void {
    if (this.imageUrl) URL.revokeObjectURL(this.imageUrl);
    this.imageUrl = undefined;
  }

  private showError(summary: string, detail?: string): void {
    this.messageService.add({ severity: 'error', summary, detail });
  }
}
