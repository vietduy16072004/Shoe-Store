export interface User {
  userId: string; //
  username: string; //
  password?: string; //
  email: string; //
  phone?: string; //
  address?: string; //
  role: string; //
  provider?: string; // Nguồn đăng nhập (Google/Facebook)
  providerId?: string; // ID từ Social
}