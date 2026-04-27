# 🍔 Good Hamburger - Sistema de Gestão de Pedidos

Esta solução foi desenvolvida como parte de um desafio técnico para a **Good Hamburger**, com o objetivo de demonstrar a aplicação de padrões arquiteturais modernos, design de software orientado a domínio (**DDD**) e uma experiência de usuário fluida e responsiva.

## Desafio

O objetivo é construir um sistema de registro de pedidos para uma lanchonete, com as seguintes definições:

### Cardápio e Preços
- **Sanduíches**: X Burger (R$ 5,00), X Egg (R$ 4,50), X Bacon (R$ 7,00)
- **Acompanhamentos**: Batata frita (R$ 2,00), Refrigerante (R$ 2,50)

### Regras de Negócio
- **Combos de Desconto**:
  - Sanduíche + Batata + Refrigerante → **20% de desconto**
  - Sanduíche + Refrigerante → **15% de desconto**
  - Sanduíche + Batata → **10% de desconto**
- **Restrições**: Cada pedido pode conter no máximo um item de cada categoria. Itens duplicados retornam erro.

### Requisitos Técnicos
- API REST em C# / .NET para o CRUD completo de pedidos.
- Cálculo automático de descontos, subtotal e total.
- Validação de erros e respostas claras.
- Endpoint para consulta do cardápio.
- **Diferenciais Implementados**: Frontend em **Blazor** e **Testes Automatizados**.

---

## Como Executar

A solução foi totalmente containerizada para facilitar a execução e garantir a paridade entre ambientes.

### Pré-requisitos
- **Docker Desktop** instalado (Windows/Mac) ou Docker Engine + Compose (Linux).

### Passo a Passo
1. Abra o terminal na pasta raiz do projeto.
2. Inicialize o ecossistema com o comando:
   ```bash
   docker-compose up --build
   ```
3. O sistema utiliza um *healthcheck* para garantir que o banco MySQL esteja pronto antes de expor os serviços.
4. Acesse as aplicações:
   - **Interface Web**: [http://localhost:5002](http://localhost:5002)
   - **Documentação da API**: [http://localhost:5001](http://localhost:5001)

---

## Arquitetura e Decisões Técnicas

O projeto foi estruturado seguindo os princípios da **Clean Architecture**, garantindo que a lógica de negócio seja independente de frameworks e fácil de manter.

### Camadas do Projeto
- **Domain**: Contém as entidades principais (`Order`), objetos de valor e a lógica pura de precificação (`OrderPricingCalculator`). Aqui residem as regras de negócio de descontos (10%, 15%, 20%).
- **Application**: Define os casos de uso do sistema, interfaces de repositório e serviços de coordenação.
- **Infrastructure**: Implementação da persistência utilizando **Entity Framework Core** com **MySQL** (via Docker) e segurança.
- **Api**: Exposição dos recursos através de **.NET Minimal APIs**, focando em performance e simplicidade.
- **Web**: Interface rica construída com **Blazor Interactive Server**, oferecendo uma experiência de SPA (Single Page Application) com estados em tempo real.

### Pilares Técnicos
1. **Domain-Driven Design (DDD)**: O coração do sistema reside na camada de domínio, onde as entidades e o `OrderPricingCalculator` garantem a integridade das regras de negócio (descontos de 10%, 15% e 20%) de forma isolada e testável.
2. **Minimal APIs com AOT Readiness**: A API utiliza o padrão de Minimal APIs do .NET 10 para reduzir o overhead de middlewares e garantir alta performance, estando preparada para compilação Ahead-of-Time (AOT).
3. **Stateful UI com Blazor**: A escolha pelo Blazor Interactive Server permite uma interface rica com sincronização de estado em tempo real e uma base de código unificada em C#.
4. **Segurança (JWT)**: Autenticação baseada em claims com tokens JWT, utilizando cookies HttpOnly para mitigar ataques de XSS e CSRF.

---

## Testes Automatizados

Foram implementados testes de unidade e integração focados nas partes mais críticas do sistema:
- **Domain Tests**: Testes de unidade para validação das regras de desconto e restrições de negócio.
- **Integration Tests**: Validação do pipeline de persistência e repositórios utilizando banco de dados em memória.

Para rodar os testes:
```bash
dotnet test
```

---

## Considerações para Produção (Out of Scope)

Este projeto foi desenvolvido como um desafio técnico, focando na arquitetura e regras de negócio. Para um ambiente produtivo real, os seguintes pontos seriam considerados:

- **Observabilidade Avançada**: Integração com ferramentas de APM (como Elastic APM ou Application Insights) e exportação de métricas via OpenTelemetry para Prometheus/Grafana.
- **Estratégias de Caching**: Implementação de cache distribuído com Redis para reduzir a latência em consultas de cardápio e autenticação.
- **Segurança e Identidade**: Integração com Identity Providers externos (Auth0, Keycloak) e implementação de Rate Limiting e WAF (Web Application Firewall).
- **Pipeline de CI/CD**: Automação completa de build, testes e deploy utilizando GitHub Actions ou Azure DevOps, com estratégias de Blue/Green Deployment.
- **Controle de Acesso (RBAC)**: Diferenciação de perfis (ex: Admin vs. Cliente). Atualmente o sistema opera em modelo flat, mas em produção haveria restrições baseadas em roles.
- **Isolamento de Dados**: Garantir que clientes visualizem apenas seus próprios pedidos (Multi-tenancy) e que administradores tenham visão consolidada.
- **Paginação de Resultados**: Implementação de paginação (`Skip`/`Take`) nos endpoints de listagem de pedidos para suportar grandes volumes de dados de forma eficiente.
- **Gestão de Segredos**: Armazenamento de chaves e conexões em cofres seguros como Azure Key Vault ou HashiCorp Vault.

---