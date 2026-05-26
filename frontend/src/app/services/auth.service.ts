import { Injectable, signal, computed } from '@angular/core';
import { environment } from '../../environments/environment';

@Injectable({
  providedIn: 'root'
})
export class AuthService {
  private readonly tokenSignal = signal<string | null>(localStorage.getItem('admin_token'));

  isAuthenticated = computed(() => !!this.tokenSignal());

  async login(email: string, password: string): Promise<boolean> {
    const emailHash = await this.sha256(email.trim().toLowerCase());
    const passwordHash = await this.sha256(password);

    if (
      emailHash === environment.adminEmailHash &&
      passwordHash === environment.adminPasswordHash
    ) {
      const mockToken = 'static-admin-session-token';
      localStorage.setItem('admin_token', mockToken);
      this.tokenSignal.set(mockToken);
      return true;
    }
    return false;
  }

  logout() {
    localStorage.removeItem('admin_token');
    this.tokenSignal.set(null);
  }

  private async sha256(message: string): Promise<string> {
    const msgBuffer = new TextEncoder().encode(message);
    const hashBuffer = await crypto.subtle.digest('SHA-256', msgBuffer);
    const hashArray = Array.from(new Uint8Array(hashBuffer));
    return hashArray.map(b => b.toString(16).padStart(2, '0')).join('');
  }
}
