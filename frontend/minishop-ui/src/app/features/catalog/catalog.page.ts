import { CurrencyPipe } from '@angular/common';
import { Component, inject, OnInit, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { InputTextModule } from 'primeng/inputtext';
import { TableModule } from 'primeng/table';
import { TagModule } from 'primeng/tag';
import { ProductService } from '../../core/services/product.service';
import { Product } from '../../models';

@Component({
  selector: 'app-catalog',
  imports: [CurrencyPipe, FormsModule, InputTextModule, TableModule, TagModule],
  templateUrl: './catalog.page.html',
})
export class CatalogPage implements OnInit {
  private readonly productService = inject(ProductService);

  readonly items = signal<Product[]>([]);
  readonly total = signal(0);
  readonly loading = signal(false);
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
    this.productService.getPage(this.pageNumber, 10, this.search, true).subscribe({
      next: (result) => {
        this.items.set(result.items);
        this.total.set(result.totalCount);
        this.loading.set(false);
      },
      error: () => this.loading.set(false),
    });
  }
}
