import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { TestBed } from '@angular/core/testing';
import { ProductService } from './product.service';

describe('ProductService', () => {
  it('sends product paging and search parameters', () => {
    TestBed.configureTestingModule({
      providers: [provideHttpClient(), provideHttpClientTesting()],
    });
    const service = TestBed.inject(ProductService);
    const http = TestBed.inject(HttpTestingController);

    service.getPage(2, 25, 'mouse', true).subscribe();

    const request = http.expectOne((candidate) => candidate.url.endsWith('/api/products'));
    expect(request.request.params.get('page')).toBe('2');
    expect(request.request.params.get('pageSize')).toBe('25');
    expect(request.request.params.get('search')).toBe('mouse');
    expect(request.request.params.get('isActive')).toBe('true');
    request.flush({ items: [], totalCount: 0, page: 2, pageSize: 25 });
    http.verify();
  });

  it('uploads an image as multipart form data', () => {
    TestBed.configureTestingModule({
      providers: [provideHttpClient(), provideHttpClientTesting()],
    });
    const service = TestBed.inject(ProductService);
    const http = TestBed.inject(HttpTestingController);
    const file = new File(['image'], 'product.webp', { type: 'image/webp' });

    service.uploadImage(42, file).subscribe();

    const request = http.expectOne((candidate) => candidate.url.endsWith('/api/products/42/image'));
    expect(request.request.method).toBe('POST');
    expect(request.request.body).toBeInstanceOf(FormData);
    expect((request.request.body as FormData).get('file')).toBe(file);
    request.flush(null);
    http.verify();
  });
});
