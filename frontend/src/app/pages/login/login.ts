import { Component, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router } from '@angular/router';

import { AuthService } from '../../services/auth.service';
import { problemMessage } from '../../utils/problem-details';

@Component({
  selector: 'app-login',
  imports: [CommonModule],
  templateUrl: './login.html',
  styleUrl: './login.css',
})
export class LoginComponent {
  private readonly auth = inject(AuthService);
  private readonly router = inject(Router);

  readonly username = signal('');
  readonly password = signal('');
  readonly loading = signal(false);
  readonly error = signal<string | null>(null);

  login(): void {
    this.loading.set(true);
    this.error.set(null);

    this.auth.login({ username: this.username(), password: this.password() }).subscribe({
      next: () => {
        this.loading.set(false);
        this.router.navigateByUrl('/');
      },
      error: err => {
        this.loading.set(false);
        this.error.set(problemMessage(err, 'Login failed. Please try again.'));
      },
    });
  }
}
