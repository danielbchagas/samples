# Guia de Execução do Projeto Benchmarks

Este guia fornece instruções para configurar, compilar e executar o projeto **Benchmarks**.

## Pré-requisitos

Antes de começar, certifique-se de que você possui os seguintes itens instalados:

- .NET 8.0 SDK ou superior
- Ferramentas de linha de comando do .NET (`dotnet`)

## Configuração do Ambiente

1. **Navegue até o diretório do projeto**:
   Acesse o diretório onde o projeto está localizado:

   ```sh
   cd samples/Samples/Benchmarks
   ```

2. **Restaure as dependências**:
   Restaure todas as dependências necessárias para o projeto:

   ```sh
   dotnet restore
   ```

3. **Compile o projeto**:
   Compile o projeto para garantir que todos os arquivos estão prontos para execução:

   ```sh
   dotnet build
   ```

## Executando o Projeto

Para executar o projeto **Benchmarks**, utilize o seguinte comando:

```sh
dotnet run --project Benchmarks\Samples.Benchmarks\Samples.Benchmarks.Http.csproj -c Release
```
