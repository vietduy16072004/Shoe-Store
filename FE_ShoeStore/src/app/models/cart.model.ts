export interface CartItem {
  cart_Id: number;
  productDetail_Id: number;
  productId: number;
  productName: string;
  imageUrl: string;
  sizeName: string;
  variantName: string;
  quantity: number;
  unitPrice: number;
  totalPrice: number;
  stock: number;
}