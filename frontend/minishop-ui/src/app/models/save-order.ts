import { OrderStatus } from './order-status';

export interface SaveOrder {
  customerId: number;
  orderDateUtc: string;
  status: OrderStatus;
  items: { productId: number; quantity: number }[];
}
