import { Component, OnInit, signal, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { DomSanitizer, SafeHtml } from '@angular/platform-browser';
import { ArticleService } from '../../services/article.service';
import { Article } from '../../models/article.model';

@Component({
  selector: 'app-article-editor',
  standalone: true,
  imports: [CommonModule, RouterLink, ReactiveFormsModule],
  templateUrl: './article-editor.component.html',
  styleUrl: './article-editor.component.css'
})
export class ArticleEditorComponent implements OnInit {
  private readonly fb = inject(FormBuilder);
  private readonly route = inject(ActivatedRoute);
  private readonly articleService = inject(ArticleService);
  private readonly sanitizer = inject(DomSanitizer);

  editorForm!: FormGroup;
  isEditMode = signal<boolean>(false);
  isLoading = signal<boolean>(false);
  activeTab = signal<'write' | 'preview'>('write');
  previewHtml = signal<SafeHtml>('');
  
  // Código gerado para exportação
  generatedCode = signal<{ json: string; markdown: string; fileName: string } | null>(null);

  private isSlugManuallyEdited = false;
  private articleId: number | null = null;
  private articleCreatedAt: string | null = null;

  ngOnInit() {
    this.initForm();
    this.checkRoute();
  }

  private initForm() {
    this.editorForm = this.fb.group({
      title: ['', Validators.required],
      slug: ['', Validators.required],
      summary: ['', Validators.required],
      content: ['', Validators.required],
      tags: [''],
      coverImage: [''],
      readingTime: [0],
      isPublished: [false]
    });

    // Auto gerar slug a partir do título
    this.editorForm.get('title')?.valueChanges.subscribe(title => {
      if (!this.isSlugManuallyEdited && title) {
        this.editorForm.get('slug')?.setValue(this.slugify(title), { emitEvent: false });
      }
    });

    // Monitorar se o usuário editou o slug manualmente
    this.editorForm.get('slug')?.valueChanges.subscribe(() => {
      this.isSlugManuallyEdited = true;
    });
  }

  private checkRoute() {
    this.route.paramMap.subscribe(params => {
      const id = params.get('id');
      if (id) {
        this.isEditMode.set(true);
        this.articleId = +id;
        this.loadArticle(this.articleId);
      }
    });
  }

  private loadArticle(id: number) {
    this.isLoading.set(true);
    this.articleService.getArticle(id).subscribe({
      next: (article) => {
        this.editorForm.patchValue({
          title: article.title,
          slug: article.slug,
          summary: article.summary,
          content: article.content,
          tags: article.tags,
          coverImage: article.coverImage,
          readingTime: article.readingTime,
          isPublished: article.isPublished
        });
        this.articleCreatedAt = article.createdAt || null;
        this.isSlugManuallyEdited = true;
        this.isLoading.set(false);
      },
      error: (err) => {
        console.error('Erro ao obter artigo:', err);
        alert('Não foi possível obter os dados do artigo.');
        this.isLoading.set(false);
      }
    });
  }

  setTab(tab: 'write' | 'preview') {
    this.activeTab.set(tab);
    if (tab === 'preview') {
      const rawContent = this.editorForm.get('content')?.value || '';
      const parsed = this.parseMarkdown(rawContent);
      this.previewHtml.set(this.sanitizer.bypassSecurityTrustHtml(parsed));
    }
  }

  onCoverError(event: any) {
    event.target.style.display = 'none';
  }

  onSubmit() {
    if (this.editorForm.invalid) return;

    const formVal = this.editorForm.value;
    const slug = formVal.slug || this.slugify(formVal.title);
    const fileName = `${slug}.md`;
    
    const readingTime = formVal.readingTime > 0 
      ? formVal.readingTime 
      : this.calculateReadingTime(formVal.content);

    const jsonMetadata = {
      id: this.articleId || Math.floor(Math.random() * 1000) + 10,
      title: formVal.title,
      slug: slug,
      summary: formVal.summary,
      tags: formVal.tags || '',
      coverImage: formVal.coverImage || '',
      readingTime: readingTime,
      createdAt: this.articleCreatedAt || new Date().toISOString(),
      fileName: fileName,
      isPublished: formVal.isPublished
    };

    const jsonString = JSON.stringify(jsonMetadata, null, 2);
    
    // Força adicionar h1 com título caso o markdown comece sem ele
    const markdownContent = formVal.content.startsWith('#')
      ? formVal.content 
      : `# ${formVal.title}\n\n${formVal.content}`;

    this.generatedCode.set({
      json: jsonString,
      markdown: markdownContent,
      fileName: fileName
    });
  }

  copyText(text: string) {
    navigator.clipboard.writeText(text);
    alert('Conteúdo copiado com sucesso!');
  }

  private slugify(text: string): string {
    return text
      .toString()
      .toLowerCase()
      .normalize('NFD')
      .replace(/[\u0300-\u036f]/g, '')
      .replace(/[^a-z0-9\s-]/g, '')
      .replace(/\s+/g, '-')
      .replace(/-+/g, '-')
      .trim()
      .replace(/^-+|-+$/g, '');
  }

  private calculateReadingTime(content: string): number {
    if (!content) return 1;
    const words = content.split(/\s+/).length;
    return Math.max(1, Math.ceil(words / 200));
  }

  private parseMarkdown(markdown: string): string {
    if (!markdown) return '<em>Nenhum conteúdo para visualizar.</em>';
    
    let html = markdown;
    
    html = html
      .replace(/&/g, "&amp;")
      .replace(/</g, "&lt;")
      .replace(/>/g, "&gt;");

    html = html.replace(/```(\w*)\n([\s\S]*?)\n```/g, (match, lang, code) => {
      return `<pre class="code-block language-${lang}"><code>${code.trim()}</code></pre>`;
    });

    html = html.replace(/`([^`]+)`/g, '<code class="inline-code">$1</code>');

    html = html.replace(/^# (.*?)$/gm, '<h1 class="md-h1">$1</h1>');
    html = html.replace(/^## (.*?)$/gm, '<h2 class="md-h2">$1</h2>');
    html = html.replace(/^### (.*?)$/gm, '<h3 class="md-h3">$1</h3>');

    html = html.replace(/\*\*([^*]+)\*\*/g, '<strong>$1</strong>');

    html = html.replace(/^-\s+(.*?)$/gm, '<li>$1</li>');
    html = html.replace(/(<li>.*<\/li>)/g, '<ul>$1</ul>');
    html = html.replace(/<\/ul>\s*<ul>/g, '');

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
