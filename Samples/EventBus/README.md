# Exemplo de Event Bus

## Requisitos

- .NET 8.0
- ASP.NET Core
- C# 12.0
- Bogus

## Estrutura do Projeto

```bash
Samples.EventBus/
│
├── Application/
│   └── Handler/
│       ├── SubmittedHandler.cs
|
├── Domain/
│   └── Events/
│       ├── Submitted.cs
|
├── Infrastructure/
│   ├── Bus/
|       ├── InMemoryEventBus.cs
│
├── appsettings.json
├── Program.cs
├── Worker
└── Properties/