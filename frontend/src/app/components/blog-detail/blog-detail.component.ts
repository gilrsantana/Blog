import { Component, OnInit, signal, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { DomSanitizer, SafeHtml } from '@angular/platform-browser';
import { ArticleService } from '../../services/article.service';
import { Article } from '../../models/article.model';

@Component({
  selector: 'app-blog-detail',
  standalone: true,
  imports: [CommonModule, RouterLink],
  templateUrl: './blog-detail.component.html',
  styleUrl: './blog-detail.component.css'
})
export class BlogDetailComponent implements OnInit {
  private readonly route = inject(ActivatedRoute);
  private readonly articleService = inject(ArticleService);
  private readonly sanitizer = inject(DomSanitizer);

  article = signal<Article | null>(null);
  formattedContent = signal<SafeHtml>('');
  isLoading = signal<boolean>(true);

  ngOnInit() {
    this.route.paramMap.subscribe(params => {
      const slug = params.get('slug');
      if (slug) {
        this.loadArticle(slug);
      } else {
        this.isLoading.set(false);
      }
    });
  }

  loadArticle(slug: string) {
    this.isLoading.set(true);
    this.articleService.getArticleBySlug(slug).subscribe({
      next: (data) => {
        this.article.set(data);
        const parsed = this.parseMarkdown(data.content);
        this.formattedContent.set(this.sanitizer.bypassSecurityTrustHtml(parsed));
        this.isLoading.set(false);
      },
      error: (err) => {
        console.error('Erro ao buscar artigo:', err);
        this.article.set(null);
        this.isLoading.set(false);
      }
    });
  }

  parseMarkdown(markdown: string): string {
    if (!markdown) return '';
    
    let html = markdown;
    
    // Escapar tags HTML existentes para evitar XSS
    html = html
      .replace(/&/g, "&amp;")
      .replace(/</g, "&lt;")
      .replace(/>/g, "&gt;");

    // Code blocks: ```language ... ```
    html = html.replace(/```(\w*)\n([\s\S]*?)\n```/g, (match, lang, code) => {
      return `<pre class="code-block language-${lang}"><code>${code.trim()}</code></pre>`;
    });

    // Inline code: `code`
    html = html.replace(/`([^`]+)`/g, '<code class="inline-code">$1</code>');

    // Headers
    html = html.replace(/^# (.*?)$/gm, '<h1 class="md-h1">$1</h1>');
    html = html.replace(/^## (.*?)$/gm, '<h2 class="md-h2">$1</h2>');
    html = html.replace(/^### (.*?)$/gm, '<h3 class="md-h3">$1</h3>');

    // Bold: **text**
    html = html.replace(/\*\*([^*]+)\*\*/g, '<strong>$1</strong>');

    // Bullet lists
    html = html.replace(/^-\s+(.*?)$/gm, '<li>$1</li>');
    html = html.replace(/(<li>.*<\/li>)/g, '<ul>$1</ul>');
    html = html.replace(/<\/ul>\s*<ul>/g, '');

    // Paragraphs: split by double-newline
    const blocks = html.split(/\n\n+/);
    html = blocks.map(block => {
      const trimmed = block.trim();
      if (!trimmed) return '';
      if (trimmed.startsWith('<h') || trimmed.startsWith('<pre') || trimmed.startsWith('<ul') || trimmed.startsWith('<li') || trimmed.startsWith('<p>')) {
        return trimmed;
      }
      return `<p class="md-p">${trimmed.replace(/\n/g, '<br/>')}</p>`;
    }).join('\n');

    return html;
  }
}
