import { Component, inject, signal } from '@angular/core';
import { RouterOutlet, RouterLink, RouterLinkActive, Router } from '@angular/router';
import { CommonModule } from '@angular/common';
import { AuthService } from './services/auth.service';

@Component({
	selector: 'app-root',
	standalone: true,
	imports: [CommonModule, RouterOutlet, RouterLink, RouterLinkActive],
	templateUrl: './app.html',
	styleUrl: './app.css'
})
export class App {
	readonly authService = inject(AuthService);
	private readonly router = inject(Router);

	isLightTheme = signal<boolean>(localStorage.getItem('theme') === 'light');

	constructor() {
		if (this.isLightTheme()) {
			document.body.classList.add('light-theme');
		} else {
			document.body.classList.remove('light-theme');
		}
	}

	logout() {
		this.authService.logout();
		this.router.navigate(['/']);
	}

	toggleTheme() {
		this.isLightTheme.update(light => {
			const next = !light;
			if (next) {
				document.body.classList.add('light-theme');
				localStorage.setItem('theme', 'light');
			} else {
				document.body.classList.remove('light-theme');
				localStorage.setItem('theme', 'dark');
			}
			return next;
		});
	}

	title = 'DevBlog';

	get getCurrentYear(): number {
		return new Date().getFullYear();
	}
}
