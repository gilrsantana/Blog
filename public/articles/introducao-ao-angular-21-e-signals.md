# Introdução ao Angular 21 e Signals

Os **Signals** representam uma das maiores mudanças de paradigma na história recente do Angular. Eles introduzem um modelo de reatividade refinado que permite ao framework saber exatamente quais partes da aplicação precisam ser atualizadas quando o estado muda.

## O que é um Signal?

Um Signal é um wrapper em torno de um valor que pode notificar os consumidores interessados quando esse valor for alterado. Eles são simples de usar:

```typescript
import { signal, computed } from '@angular/core';

// Declarando um signal simples
const contador = signal(0);

// Lendo o valor do signal
console.log(contador()); // 0

// Atualizando o valor
contador.set(1);
contador.update(val => val + 1);
```

## Benefícios dos Signals
- **Performance Aprimorada:** O Angular pode ignorar a detecção de mudanças global e atualizar cirurgicamente apenas os elementos do DOM vinculados ao Signal.
- **Reatividade Declarativa:** Facilidade para derivar estados com `computed` e executar efeitos colaterais com `effect`.
- **Redução de Boilerplate:** Esqueça a complexidade do RxJS para estados locais simples.

Os Signals tornam o desenvolvimento Angular muito mais intuitivo e performático!
