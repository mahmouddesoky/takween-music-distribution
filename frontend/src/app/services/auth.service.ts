import { Injectable, inject, signal } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, tap } from 'rxjs';

import { LoginRequest, LoginResponse } from '../models/track.model';

/**
 * Holds the JWT in localStorage and exposes a reactive `isLoggedIn` signal so
 * templates can show/hide admin controls without subscribing manually.
 * An expired token counts as logged out.
 */
@Injectable({ providedIn: 'root' })
export class AuthService {
  private readonly http = inject(HttpClient);
  private readonly tokenKey = 'md_token';
  private readonly baseUrl = '/api/auth';

  readonly isLoggedIn = signal(false);

  constructor() {
    // Runs after the signal exists: an expired/malformed stored token is cleared here.
    this.isLoggedIn.set(this.getToken() !== null);
  }

  login(credentials: LoginRequest): Observable<LoginResponse> {
    return this.http
      .post<LoginResponse>(`${this.baseUrl}/login`, credentials)
      .pipe(tap(res => this.setToken(res.token)));
  }

  logout(): void {
    localStorage.removeItem(this.tokenKey);
    this.isLoggedIn.set(false);
  }

  /** Returns the stored token, or null (and clears it) when it is missing or expired. */
  getToken(): string | null {
    const token = localStorage.getItem(this.tokenKey);
    if (!token) return null;

    if (this.isExpired(token)) {
      this.logout();
      return null;
    }
    return token;
  }

  private setToken(token: string): void {
    localStorage.setItem(this.tokenKey, token);
    this.isLoggedIn.set(true);
  }

  /** Reads the `exp` claim (seconds since epoch). Malformed tokens are treated as expired. */
  private isExpired(token: string): boolean {
    try {
      const payload = JSON.parse(atob(token.split('.')[1].replace(/-/g, '+').replace(/_/g, '/')));
      return typeof payload.exp !== 'number' || payload.exp * 1000 <= Date.now();
    } catch {
      return true;
    }
  }
}
