# OKLCH: A Nova Era de Cores no CSS Moderno

Se você trabalha com CSS há algum tempo, provavelmente já usou `HEX`, `RGB` ou `HSL`. Embora o `HSL` tenha facilitado a manipulação de cores por humanos, ele sofre de um problema de percepção de brilho. É aí que entra o **OKLCH**.

## Por que usar OKLCH?

O OKLCH é baseado na percepção humana. Nele, a luminosidade (L) é perceptualmente uniforme. Isso significa que duas cores com a mesma luminosidade parecerão igualmente brilhantes aos olhos humanos, o que não acontece no HSL (onde um amarelo parece muito mais brilhante que um azul sob a mesma luminosidade de 50%).

## Sintaxe no CSS

A sintaxe é simples e nativa nos browsers modernos:

```css
.card {
  /* oklch(L C H) */
  /* L = Luminosidade (0% a 100%) */
  /* C = Chroma (saturação/intensidade, de 0 a ~0.4) */
  /* H = Hue (matiz, ângulo de 0 a 360) */
  background-color: oklch(60% 0.15 140);
  color: oklch(98% 0.01 140);
}
```

## Vantagens
- **Acessibilidade:** É muito mais fácil gerar paletas com contraste adequado de forma programática.
- **Cores mais vivas:** Permite acessar cores que monitores modernos (P3) conseguem exibir, muito além do tradicional sRGB.
