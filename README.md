# Solução de Microsserviços com .NET e Oracle

token
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

Este projeto foi desenvolvido para atender aos requisitos de uma arquitetura de microsserviços, demonstrando a separação de responsabilidades, comunicação entre serviços, persistência em banco de dados Oracle e consumo de APIs públicas com políticas de resiliência.

## System Design

A arquitetura da solução é composta por uma aplicação front-end (MVC) que consome duas WebAPIs independentes. Uma API gerencia os dados de produtos, persistindo em um banco Oracle, enquanto a outra busca dados de cotações de uma API pública externa.

```mermaid
graph TD
    subgraph Browser
        A[Aplicacao MVC <br> (Interface do Usuario)]
    end

    subgraph "Microsservicos (Backend)"
        B[WebAPI de Produtos <br> (ProductApi)]
        C[WebAPI de Cotacoes <br> (QuotationApi)]
    end

    subgraph "Fontes de Dados"
        D[(Banco de Dados Oracle)]
        E[API Publica de Cotacoes <br> (AwesomeAPI)]
    end

    A --"Requisicoes HTTP"--> B
    A --"Requisicoes HTTP"--> C
    B --"Leitura/Escrita via EF Core"--> D
    C --"Requisicao HTTP com Polly"--> E
```

## Pré-requisitos

* [.NET 8 SDK](https://dotnet.microsoft.com/download) ou superior.
* Uma instância de Banco de Dados Oracle acessível pela rede.
* Git.

## Como Executar Localmente

Siga os passos abaixo para configurar e executar a solução em seu ambiente local.

#### 1. Clonar o Repositório
```bash
git clone <https://github.com/Felipafaa/checkpoint4dotnet.git>
cd fiap-microservices-solution
```

#### 2. Configurar Variáveis de Ambiente

É crucial configurar as conexões com o banco de dados e as URLs das APIs.

* **API de Produtos:** Edite o arquivo `src/ProductApi/appsettings.json` e configure a `ConnectionStrings:OracleConnection` com os dados do seu banco Oracle.
    ```json
    "ConnectionStrings": {
      "OracleConnection": "User Id=SEU_USUARIO;Password=SUA_SENHA;Data Source=SEU_HOST:PORTA/SEU_SERVICE_NAME"
    }
    ```

* **Aplicação Web:** Edite o arquivo `src/WebApp/appsettings.json` e verifique se as portas das APIs correspondem às que estão definidas nos arquivos `Properties/launchSettings.json` de cada API.
    ```json
    "ApiEndpoints": {
      "ProductApi": "https://localhost:7001",
      "QuotationApi": "https://localhost:7002"
    }
    ```

#### 3. Executar as Migrations do Banco de Dados

Com a string de conexão configurada, execute o comando abaixo a partir da **pasta raiz da solução** para criar as tabelas no Oracle.

* Este comando usa o parâmetro `--project` (ou `-p`) para apontar para a library `Infrastructure` (onde o `DbContext` está) e o parâmetro `--startup-project` (ou `-s`) para apontar para a `ProductApi` (que contém a string de conexão).

```bash
dotnet ef database update --project src/Infrastructure --startup-project src/ProductApi
```

#### 4. Iniciar as Aplicações

Para que a solução funcione, os 3 projetos principais devem ser executados simultaneamente.

Abra **3 terminais diferentes** na pasta raiz da solução e execute um comando em cada um:

* **Terminal 1 (API de Produtos):**
    ```bash
    dotnet run --project src/ProductApi
    ```
* **Terminal 2 (API de Cotações):**
    ```bash
    dotnet run --project src/QuotationApi
    ```
* **Terminal 3 (Aplicação Web):**
    ```bash
    dotnet run --project src/WebApp
    ```

## URLs da Aplicação e Swagger

* **Aplicação Web (Front-end):** `https://localhost:7111` (verifique a porta no `launchSettings.json` do WebApp)
* **ProductApi Swagger UI:** `https://localhost:7001/swagger`
* **QuotationApi Swagger UI:** `https://localhost:7002/swagger`

## Princípios SOLID Aplicados

A estrutura do projeto foi pensada para aplicar os princípios SOLID de forma evidente:

1.  **SRP (Single Responsibility Principle - Princípio da Responsabilidade Única):** Cada camada e classe tem uma responsabilidade bem definida. A classe `ProductRepository` (`Infrastructure`), por exemplo, tem a única responsabilidade de gerenciar a persistência de dados para a entidade `Product`. Os `Controllers` são responsáveis apenas por expor os endpoints e orquestrar as requisições, delegando a lógica de negócio.

2.  **OCP (Open/Closed Principle - Princípio Aberto/Fechado):** O uso de interfaces como `IProductRepository` permite que a aplicação seja estendida sem modificar o código existente. Se precisássemos trocar o Oracle por outro banco, bastaria criar uma nova classe que implementa `IProductRepository`, sem alterar as classes da camada de `Application` ou da `ProductApi` que a consomem.

3.  **DIP (Dependency Inversion Principle - Princípio da Inversão de Dependência):** Os módulos de alto nível (como a `ProductApi`) não dependem de módulos de baixo nível (como a `Infrastructure`). Ambos dependem de abstrações (interfaces como `IProductRepository`, definidas na camada `Application`). A injeção de dependência, configurada no `Program.cs`, é a ferramenta que "conecta" a abstração à sua implementação concreta em tempo de execução.

## Endpoints Principais

#### ProductApi

* **`GET /api/products`**
    * **Descrição:** Retorna uma lista de todos os produtos cadastrados.
    * **Exemplo de Resposta (200 OK):**
        ```json
        [
          {
            "id": 1,
            "name": "Teclado Mecânico RGB",
            "price": 299.9,
            "stockQuantity": 30
          },
          {
            "id": 2,
            "name": "Mouse Gamer",
            "price": 150.99,
            "stockQuantity": 50
          }
        ]
        ```

* **`POST /api/products`**
    * **Descrição:** Cria um novo produto no banco de dados.
    * **Exemplo de Request Body:**
        ```json
        {
          "name": "Monitor Ultrawide 29",
          "price": 1200.00,
          "stockQuantity": 15
        }
        ```
    * **Exemplo de Resposta (201 Created):** Retorna o objeto do produto recém-criado.

#### QuotationApi

* **`GET /api/quotations/usd-brl`**
    * **Descrição:** Busca a última cotação do Dólar para o Real, consumindo uma API pública externa. A comunicação com a API externa é protegida com políticas de resiliência (Retry e Timeout) usando Polly.
    * **Endpoint Público Consumido:** `https://economia.awesomeapi.com.br/json/last/USD-BRL`
    * **Exemplo de Resposta (200 OK):**
        ```json
        {
          "USDBRL": {
            "code": "USD",
            "codein": "BRL",
            "name": "Dólar Americano/Real Brasileiro",
            "high": "5.30",
            "low": "5.25",
            "varBid": "0.01",
            "pctChange": "0.20",
            "bid": "5.28",
            "ask": "5.28",
            "timestamp": "1756186800",
            "create_date": "2025-08-25 21:00:00"
          }
        }
        ```