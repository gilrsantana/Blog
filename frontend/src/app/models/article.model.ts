export interface Article {
  id?: number;
  title: string;
  slug: string;
  summary: string;
  content: string;
  tags: string;
  coverImage: string;
  readingTime: number;
  createdAt?: string;
  updatedAt?: string | null;
  isPublished: boolean;
  fileName?: string; // Nome do arquivo .md correspondente
}
