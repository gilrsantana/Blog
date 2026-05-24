import { Component, OnInit, signal, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink } from '@angular/router';
import { ArticleService } from '../../services/article.service';
import { Article } from '../../models/article.model';

@Component({
  selector: 'app-admin-dashboard',
  standalone: true,
  imports: [CommonModule, RouterLink],
  templateUrl: './admin-dashboard.component.html',
  styleUrl: './admin-dashboard.component.css'
})
export class AdminDashboardComponent implements OnInit {
  private readonly articleService = inject(ArticleService);

  articles = signal<Article[]>([]);
  isLoading = signal<boolean>(true);

  ngOnInit() {
    this.loadArticles();
  }

  loadArticles() {
    this.isLoading.set(true);
    this.articleService.getArticles(undefined, undefined, false).subscribe({
      next: (data) => {
        this.articles.set(data);
        this.isLoading.set(false);
      },
      error: (err) => {
        console.error('Erro ao buscar artigos no painel:', err);
        this.isLoading.set(false);
      }
    });
  }

  deleteArticle(article: Article) {
    const message = `Para excluir este artigo permanentemente:
    
1. Abra o arquivo 'public/articles/articles.json' e delete a entrada correspondente.
2. Delete o arquivo físico 'public/articles/${article.fileName || article.slug + '.md'}'.`;

    alert(message);
  }
}
