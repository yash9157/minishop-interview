import { Component, inject, OnInit, signal } from '@angular/core';
import { FormControl, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { ConfirmationService, MessageService } from 'primeng/api';
import { ButtonModule } from 'primeng/button';
import { DialogModule } from 'primeng/dialog';
import { InputTextModule } from 'primeng/inputtext';
import { TableModule } from 'primeng/table';
import { TextareaModule } from 'primeng/textarea';
import { CategoryService } from '../../core/services/category.service';
import { Category } from '../../models';

@Component({
  selector: 'app-categories',
  imports: [ReactiveFormsModule, ButtonModule, DialogModule, InputTextModule, TableModule, TextareaModule],
  templateUrl: './categories.page.html',
})
export class CategoriesPage implements OnInit {
  private readonly categoryService = inject(CategoryService);
  private readonly messageService = inject(MessageService);
  private readonly confirmationService = inject(ConfirmationService);

  readonly items = signal<Category[]>([]);
  readonly loading = signal(false);
  readonly form = new FormGroup({
    name: new FormControl('', { nonNullable: true, validators: Validators.required }),
    description: new FormControl('', { nonNullable: true }),
  });

  dialogVisible = false;
  selectedId?: number;

  ngOnInit(): void {
    this.load();
  }

  load(): void {
    this.loading.set(true);
    this.categoryService.getAll().subscribe({
      next: (categories) => {
        this.items.set(categories);
        this.loading.set(false);
      },
      error: () => this.loading.set(false),
    });
  }

  open(category?: Category): void {
    this.selectedId = category?.id;
    this.form.reset({
      name: category?.name ?? '',
      description: category?.description ?? '',
    });
    this.dialogVisible = true;
  }

  save(): void {
    if (this.form.invalid) return;

    this.categoryService.save(this.form.getRawValue(), this.selectedId).subscribe({
      next: () => {
        this.dialogVisible = false;
        this.messageService.add({ severity: 'success', summary: 'Category saved' });
        this.load();
      },
      error: (error) =>
        this.messageService.add({
          severity: 'error',
          summary: 'Save failed',
          detail: error.error?.detail,
        }),
    });
  }

  remove(category: Category): void {
    this.confirmationService.confirm({
      message: `Delete ${category.name}?`,
      accept: () =>
        this.categoryService.delete(category.id).subscribe({
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
