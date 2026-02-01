export interface Product {
  product_Id: number;
  product_Name: string;
  price: number;
  discount: number;
  finalPrice: number;
  imageUrl: string;
  description?: string;
  status: number;
  category_Id: number;
  category_Name?: string;
  brand_Id: number;
  brand_Name?: string;
}