import { Component, OnInit, signal, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { ArticleService } from '../../services/article.service';
import { Article } from '../../models/article.model';

@Component({
  selector: 'app-blog-list',
  standalone: true,
  imports: [CommonModule, RouterLink, FormsModule],
  templateUrl: './blog-list.component.html',
  styleUrl: './blog-list.component.css'
})
export class BlogListComponent implements OnInit {
  private readonly articleService = inject(ArticleService);

  articles = signal<Article[]>([]);
  tags = signal<string[]>([]);
  selectedTag = signal<string>('');
  searchQuery = signal<string>('');
  isLoading = signal<boolean>(true);

  ngOnInit() {
    // Carregar todas as tags a partir de todos os posts na inicialização
    this.articleService.getArticles(undefined, undefined, true).subscribe({
      next: (data) => {
        const allTags = data
          .flatMap(a => a.tags.split(','))
          .map(t => t.trim())
          .filter(t => t.length > 0);
        this.tags.set(Array.from(new Set(allTags)));
        this.articles.set(data);
        this.isLoading.set(false);
      },
      error: (err) => {
        console.error('Erro ao buscar artigos semente:', err);
        this.isLoading.set(false);
      }
    });
  }

  loadArticles() {
    this.isLoading.set(true);
    this.articleService.getArticles(this.searchQuery(), this.selectedTag(), true).subscribe({
      next: (data) => {
        this.articles.set(data);
        this.isLoading.set(false);
      },
      error: (err) => {
        console.error('Erro ao filtrar artigos:', err);
        this.isLoading.set(false);
      }
    });
  }

  filterByTag(tag: string) {
    this.selectedTag.set(tag);
    this.loadArticles();
  }

  onSearchChange(query: string) {
    this.searchQuery.set(query);
    this.loadArticles();
  }
}
