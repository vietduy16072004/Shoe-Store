import { Routes } from '@angular/router';
import { AdminLayoutComponent } from './components/admin-layout/admin-layout';
import { CustomerLayoutComponent } from './components/customer-layout/customer-layout';
import { HomeComponent } from './components/Home/home/home';

import { LoginComponent } from './components/Account/login/login';
import { RegisterComponent } from './components/Account/register/register';
import { ProfileComponent } from './components/Account/profile/profile';

import { ProductListComponent } from './components/product-list/product-list';
import { ProductDetailComponent } from './components/product-detail/product-detail';
import { EditDetailComponent } from './components/product-detail/edit-detail/edit-detail';
import { UsersComponent } from './components/user/user';


export const routes: Routes = [
  // 1. KHU VỰC CUSTOMER: Mặc định chạy Home trước
  {
    path: 'customer',
    component: CustomerLayoutComponent,
    children: [
      { path: '', redirectTo: 'home', pathMatch: 'full' },
      { path: 'home', component: HomeComponent },
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
    path: '',
    component: AdminLayoutComponent,
    children: [
      { path: 'product-list', component: ProductListComponent },
      { path: 'product-detail/:id', component: ProductDetailComponent },
      { path: 'product-detail/edit/:id', component: EditDetailComponent },
      // Sau này thêm Brand, Category... vào đây
      { path: 'user', component: UsersComponent },

      { path: 'profile', component: ProfileComponent }

    ]
  },
];