export interface ProductDetail {
  productDetail_Id: number;
  product_Id: number;
  size_Id: number;
  sizeName?: string; // Khớp với mapping trong Controller
  variants_Id: number;
  variantName?: string;
  quantity: number;
  images?: ProductImage[];
}

export interface ProductImage {
  image_Id: number;
  imagePath: string;
  displayOrder: number;
}