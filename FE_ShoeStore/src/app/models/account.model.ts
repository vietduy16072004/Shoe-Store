export interface  Account {
  userId: string;
  username: string;
  password: string;
  email: string;
  role: string;
  phone?: string;
  address?: string;
  provider?: string;
  providerId?: string;
}