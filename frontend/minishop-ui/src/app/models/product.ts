export interface Product {
  id: number;
  categoryId: number;
  categoryName: string;
  sku: string;
  name: string;
  price: number;
  stockQuantity: number;
  isActive: boolean;
}
