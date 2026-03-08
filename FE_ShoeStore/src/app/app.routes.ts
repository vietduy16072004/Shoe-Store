import { Routes } from '@angular/router';
import { AdminLayoutComponent } from './components/admin-layout/admin-layout';
import { CustomerLayoutComponent } from './components/customer-layout/customer-layout';
import { HomeComponent } from './components/Home/home/home';
import { PageDiscountEventComponent } from './components/Home/page-discount-event/page-discount-event';
import { PageProductDetailComponent } from './components/Home/page-product-detail/page-product-detail';
import { CartComponent } from './components/Home/cart/cart';


import { LoginComponent } from './components/Account/login/login';
import { RegisterComponent } from './components/Account/register/register';
import { ProfileComponent } from './components/Account/profile/profile';


import { ProductListComponent } from './components/product-list/product-list';
import { ProductDetailComponent } from './components/product-detail/product-detail';
import { BrandComponent } from './components/brand/brand';
import { CategoryComponent } from './components/category/category';
import { SizeComponent } from './components/size/size';
import { VariantComponent } from './components/variant/variant';
import { EditDetailComponent } from './components/product-detail/edit-detail/edit-detail';
import { UsersComponent } from './components/user/user';
import { DiscountEventsComponent } from './components/discount-events/discount-events/discount-events';
import { DiscountEventDetailComponent } from './components/discount-events/discount-event-detail/discount-event-detail';


export const routes: Routes = [
  // 1. KHU VỰC CUSTOMER: Mặc định chạy Home trước
  {
    path: '',
    component: CustomerLayoutComponent,
    children: [
      { path: '', redirectTo: 'home', pathMatch: 'full' },
      { path: 'home', component: HomeComponent },
      { path: 'uu-dai', component: PageDiscountEventComponent },
      { path: 'uu-dai/:id', component: PageDiscountEventComponent },
      { path: 'product/:id', component: PageProductDetailComponent },
      { path: 'cart', component: CartComponent },
      // Sau này bạn có thể thêm { path: 'cart', component: CartComponent } vào đây
      { path: 'profile', component: ProfileComponent },
    ]
  },

  { 
    path: 'account', children: [
      { path: 'login', component: LoginComponent },
      { path: 'register', component: RegisterComponent },
    ]
  },
  {
    path: 'admin',
    component: AdminLayoutComponent,
    children: [
      { path: 'product-list', component: ProductListComponent },
      { path: 'product-detail/:id', component: ProductDetailComponent },
      { path: 'product-detail/edit/:id', component: EditDetailComponent },
      // Sau này thêm Brand, Category... vào đây
      { path: 'brand', component: BrandComponent },
      { path: 'category', component: CategoryComponent },
      { path: 'size', component: SizeComponent },
      { path: 'variant', component: VariantComponent },

      { path: 'user', component: UsersComponent },

      { path: 'discount-events', component: DiscountEventsComponent },
      { path: 'discount-event-detail/:id', component: DiscountEventDetailComponent },

      { path: 'profile', component: ProfileComponent },
    ]
  },
];