import { OrderStatus } from './order-status';

export interface OrderSummary {
  id: number;
  orderNumber: string;
  customerId: number;
  customerName: string;
  orderDateUtc: string;
  status: OrderStatus;
  totalAmount: number;
  itemCount: number;
}
