
## Componentes Principais

### EventBus

O componente EventBus é responsável por gerenciar a comunicação entre diferentes partes do sistema através de eventos. Ele está localizado em [Samples/EventBus](Samples/EventBus).

### Orchestrator

O componente Orchestrator coordena e gerencia os fluxos de trabalho e processos do sistema. Ele está localizado em [Samples/Orchestrator](Samples/Orchestrator).

## Configuração e Instalação

1. Clone o repositório:
    ```sh
    git clone https://github.com/seu-usuario/samples.git
    ```

2. Navegue até o diretório do projeto:
    ```sh
    cd samples
    ```

3. Restaure as dependências do projeto:
    ```sh
    dotnet restore
    ```

4. Compile o projeto:
    ```sh
    dotnet build
    ```

## Executando o Projeto

Para executar o projeto, utilize o seguinte comando:

```sh
dotnet run --project Samples/EventBus/Samples.EventBus.csproj# Samples