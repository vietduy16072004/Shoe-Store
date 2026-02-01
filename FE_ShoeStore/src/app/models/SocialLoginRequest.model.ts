export interface SocialLoginRequest {
  email: string;
  name: string;
  provider: string; // "Google"
  providerId: string;
}