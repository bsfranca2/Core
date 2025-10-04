# Bsfranca2.Core

Biblioteca base com abstrações e contratos reutilizáveis para aplicações .NET modernas.  
Faz parte do conjunto de bibliotecas pessoais de Bruno França, destinadas a padronizar e acelerar o desenvolvimento de projetos com **Clean Architecture** e **Domain-Driven Design**.

---

## 📦 Instalação

```bash
dotnet add package Bsfranca2.Core
```

---

## 🧱 Conteúdo

A biblioteca fornece interfaces e tipos base usados em diversos contextos de domínio e infraestrutura.

### 🧩 Abstrações principais

| Tipo | Descrição |
|------|------------|
| `IEntity` | Interface de marcação para entidades de domínio. |
| `IRepository<TEntity>` | Contrato genérico para repositórios. |
| `ICommand` / `ICommandHandler` | Contratos para comandos e manipuladores (Command Pattern / CQRS). |
| `IQuery` / `IQueryHandler` | Contratos para consultas. |
| `IEvent` / `BaseEvent` | Abstrações para eventos de domínio e integração. |

---

## 🧠 Exemplo de uso

```csharp
public sealed record CreateOrderCommand(Guid CustomerId, decimal Amount) : ICommand;

public sealed class CreateOrderCommandHandler : ICommandHandler<CreateOrderCommand>
{
    public Task Handle(CreateOrderCommand command, CancellationToken cancellationToken)
    {
        // lógica de criação do pedido...
        return Task.CompletedTask;
    }
}
```

---

## 🛠️ Build local

```bash
dotnet build -c Release
dotnet pack -c Release
```

O pacote será gerado em `bin/Release/Bsfranca2.Core.{version}.nupkg`.

---

## 📄 Licença

Distribuído sob a licença [MIT](https://opensource.org/licenses/MIT).

---

## 🔗 Repositório

[github.com/bsfranca2/Core](https://github.com/bsfranca2/Core)
