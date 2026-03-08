export interface DiscountEventList {
  id: number;
  eventName: string;
  description: string;
  discountDisplay: string;
  startDate: Date;
  endDate: Date;
  isActive: boolean;
  productCount: number;
  statusLabel: string;
}

export interface DiscountEventForm {
  id: number;
  eventName: string;
  description: string;
  discountValue: number;
  discountType: number;
  startDate: string;
  endDate: string;
  isActive: boolean;
  selectedProductIds: number[];
}

export interface ProductInEvent {
  productId: number;
  productName: string;
  originalPrice: number;
  imageUrl: string;
  categoryName: string;
}

export interface DiscountEventDetail {
  id: number;
  eventName: string;
  description: string;
  discountDisplay: string;
  startDate: Date;
  endDate: Date;
  isActive: boolean;
  statusLabel: string;
  products: ProductInEvent[];
}