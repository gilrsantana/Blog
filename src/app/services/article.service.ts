import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, of, throwError } from 'rxjs';
import { map, switchMap, catchError } from 'rxjs/operators';
import { Article } from '../models/article.model';

@Injectable({
  providedIn: 'root'
})
export class ArticleService {
  private readonly http = inject(HttpClient);
  // Caminho relativo a partir da pasta 'public' servida na raiz
  private readonly indexUrl = 'articles/articles.json';

  getArticles(search?: string, tag?: string, onlyPublished = true): Observable<Article[]> {
    return this.http.get<Article[]>(this.indexUrl).pipe(
      map(articles => {
        let filtered = [...articles];

        if (onlyPublished) {
          filtered = filtered.filter(a => a.isPublished);
        }

        if (search) {
          const searchLower = search.toLowerCase();
          filtered = filtered.filter(a => 
            a.title.toLowerCase().includes(searchLower) ||
            a.summary.toLowerCase().includes(searchLower) ||
            a.tags.toLowerCase().includes(searchLower)
          );
        }

        if (tag) {
          const tagLower = tag.toLowerCase();
          filtered = filtered.filter(a => 
            a.tags.toLowerCase().includes(tagLower)
          );
        }

        // Ordenar decrescente por data
        return filtered.sort((a, b) => {
          const dateA = a.createdAt ? new Date(a.createdAt).getTime() : 0;
          const dateB = b.createdAt ? new Date(b.createdAt).getTime() : 0;
          return dateB - dateA;
        });
      }),
      catchError(err => {
        console.error('Erro ao ler index de artigos:', err);
        return of([]);
      })
    );
  }

  getArticle(id: number): Observable<Article> {
    return this.http.get<Article[]>(this.indexUrl).pipe(
      switchMap(articles => {
        const article = articles.find(a => a.id === id);
        if (!article) {
          return throwError(() => new Error('Artigo não encontrado'));
        }
        
        return this.loadMarkdownContent(article);
      })
    );
  }

  getArticleBySlug(slug: string): Observable<Article> {
    return this.http.get<Article[]>(this.indexUrl).pipe(
      switchMap(articles => {
        const article = articles.find(a => a.slug.toLowerCase() === slug.toLowerCase());
        if (!article) {
          return throwError(() => new Error('Artigo não encontrado'));
        }
        
        return this.loadMarkdownContent(article);
      })
    );
  }

  private loadMarkdownContent(article: Article): Observable<Article> {
    if (!article.fileName) {
      return of({ ...article, content: '<em>Este artigo não possui um arquivo Markdown configurado.</em>' });
    }

    return this.http.get(`articles/${article.fileName}`, { responseType: 'text' }).pipe(
      map(content => ({
        ...article,
        content
      })),
      catchError(err => {
        console.error(`Erro ao carregar arquivo Markdown: articles/${article.fileName}`, err);
        return of({
          ...article,
          content: '# Erro ao carregar o conteúdo\n\nNão foi possível ler o arquivo Markdown deste artigo.'
        });
      })
    );
  }

  // Simulações para manter compatibilidade com o editor frontend (Jamstack)
  createArticle(article: Article): Observable<Article> {
    return of({
      ...article,
      id: Math.floor(Math.random() * 1000) + 10,
      createdAt: new Date().toISOString()
    });
  }

  updateArticle(id: number, article: Article): Observable<void> {
    return of(undefined);
  }

  deleteArticle(id: number): Observable<void> {
    return of(undefined);
  }
}
