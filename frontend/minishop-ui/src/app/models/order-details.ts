import { OrderItem } from './order-item';
import { OrderSummary } from './order-summary';

export interface OrderDetails extends Omit<OrderSummary, 'itemCount'> {
  items: OrderItem[];
}
