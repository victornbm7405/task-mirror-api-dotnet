# TaskMirror API – Gestão de Tarefas com Feedback Automatizado

API REST em .NET 8 para gestão de tarefas entre líderes e colaboradores, com autenticação JWT, banco Oracle e geração automática de feedbacks por IA.

Toda a documentação técnica dos endpoints (rota, parâmetros, responses) pode ser consultada no Swagger público:

- Swagger UI: `https://task-mirror-api-dotnet.onrender.com/swagger/index.html`

Abaixo estão apenas os JSONs mínimos necessários para chamar os principais endpoints (corpos de requisição), junto com as permissões de acesso (roles). Não há instruções de execução local para evitar problemas com o banco de dados Oracle compartilhado.

> Observação: todas as chamadas autenticadas devem enviar o header  
> `Authorization: Bearer <seu_token_jwt>`.

---

## 1. Autenticação (Auth)

Base: `/api/v1/auth`

### 1.1 Login

- Método: `POST /api/v1/auth/login`  
- Permissão: público (`AllowAnonymous`)

Body:

```json
{
  "username": "lider",
  "password": "123456"
}
```

### 1.2 Usuário atual

- Método: `GET /api/v1/auth/me`  
- Permissão: `LIDER` e `USER` (qualquer usuário autenticado)

Sem corpo de requisição.

---

## 2. Usuários

Base: `/api/v1/usuarios`  
Permissão geral do controller: somente `LIDER`.

> Somente usuários com role `LIDER` podem listar, criar, atualizar ou deletar usuários.

### 2.1 Criar usuário

- Método: `POST /api/v1/usuarios`  
- Permissão: `LIDER`

Body:

```json
{
  "username": "novouser",
  "password": "SenhaForte123",
  "roleUsuario": "USER",
  "funcao": "Analista",
  "idLider": 1
}
```

### 2.2 Atualizar usuário

- Método: `PUT /api/v1/usuarios/{idUsuario}`  
- Permissão: `LIDER`

Body:

```json
{
  "username": "novouseratualizado",
  "password": "NovaSenha123",
  "roleUsuario": "USER",
  "funcao": "Analista Sênior",
  "idLider": 1
}
```

### 2.3 Endpoints de consulta/remoção (sem corpo)

- `GET /api/v1/usuarios?page=1&pageSize=10` – listar usuários (apenas `LIDER`)  
- `GET /api/v1/usuarios/{idUsuario}` – detalhes de usuário (apenas `LIDER`)  
- `DELETE /api/v1/usuarios/{idUsuario}` – remover usuário (apenas `LIDER`)

---

## 3. Tipos de Tarefa

Base: `/api/v1/tipos-tarefa`  
Permissão geral do controller: `LIDER` e `USER` autenticados.

### 3.1 Endpoints (sem corpo)

- `GET /api/v1/tipos-tarefa?page=1&pageSize=10` – listar tipos de tarefa (`LIDER` e `USER`)  
- `GET /api/v1/tipos-tarefa/resumo` – resumo para dashboards (`LIDER` e `USER`)

---

## 4. Status de Tarefa

Base: `/api/status`  
Permissão geral do controller: `LIDER` e `USER` autenticados.

### 4.1 Endpoint (sem corpo)

- `GET /api/status` – lista apenas os status em uso (`LIDER` e `USER`)

---

## 5. Tarefas

Base: `/api/v1/tarefas`  
Permissão geral do controller: `LIDER` e `USER` autenticados.

Regras principais implementadas no controller e no serviço:

- `LIDER`:
  - Vê todas as tarefas (GET lista e GET por id).
  - Pode criar, atualizar e excluir tarefas.
- `USER`:
  - Vê somente as suas tarefas.
  - Só pode iniciar/finalizar tarefas atribuídas a ele.

### 5.1 Criar tarefa

- Método: `POST /api/v1/tarefas`  
- Permissão: somente `LIDER`

Body (`TarefaCreateRequest`):

```json
{
  "descricao": "Revisar PR do backend",
  "tempoEstimado": 45,
  "idUsuario": 2,
  "idLider": 1,
  "idTipoTarefa": 2
}
```

### 5.2 Atualizar tarefa

- Método: `PUT /api/v1/tarefas/{idTarefa}`  
- Permissão: somente `LIDER`

Body (`TarefaUpdateRequest` – campos opcionais):

```json
{
  "descricao": "Revisar PR do backend e ajustar documentação",
  "tempoEstimado": 60,
  "idUsuario": 2,
  "idLider": 1,
  "idTipoTarefa": 2,
  "idStatusTarefa": 2,
  "dataInicio": "2025-11-21T18:00:00Z",
  "dataFim": null,
  "tempoReal": null
}
```

### 5.3 Iniciar tarefa

- Método: `POST /api/v1/tarefas/{idTarefa}/iniciar`  
- Permissão: somente `USER`  
  - O usuário autenticado precisa ser o dono da tarefa.

Sem corpo de requisição.

### 5.4 Finalizar tarefa (gera feedback por IA)

- Método: `POST /api/v1/tarefas/{idTarefa}/finalizar`  
- Permissão: somente `USER`  
  - O usuário autenticado precisa ser o dono da tarefa.

Sem corpo de requisição.

Ao finalizar, a API:

- Atualiza status da tarefa para finalizada.  
- Calcula `tempoReal` com base nas datas.  
- Chama o serviço de IA para gerar feedback.  
- Cria um registro em `tbl_feedbacks`.

### 5.5 Consultar tarefas e tempo médio

Endpoints de leitura (sem corpo), acessíveis por `LIDER` e `USER`:

- `GET /api/v1/tarefas?page=1&pageSize=10` – lista paginada de tarefas  
- `GET /api/v1/tarefas/{idTarefa}` – detalhe de uma tarefa  
- `GET /api/v1/tarefas/tempo-medio-finalizadas` – tempo médio em minutos das tarefas finalizadas

### 5.6 Remover tarefa

- Método: `DELETE /api/v1/tarefas/{idTarefa}`  
- Permissão: somente `LIDER`  
- Sem corpo de requisição.

---

## 6. Feedbacks

Base: `/api/v1/feedbacks`  
Permissão geral do controller: `LIDER` e `USER` autenticados.

Regras de negócio:

- `LIDER`: vê feedbacks de todas as tarefas.  
- `USER`: só vê feedbacks das tarefas dele; o código bloqueia acesso a feedback de tarefa de outro usuário com `Forbid()`.

### 6.1 Lista de feedbacks

- Método: `GET /api/v1/feedbacks?page=1&pageSize=10`  
- Permissão: `LIDER` e `USER`  
- Sem corpo de requisição.

### 6.2 Feedback por id

- Método: `GET /api/v1/feedbacks/{idFeedback}`  
- Permissão: `LIDER` e `USER`  
- Sem corpo de requisição (com validação de dono para `USER`).

### 6.3 Feedback por tarefa

- Método: `GET /api/v1/feedbacks/por-tarefa/{idTarefa}`  
- Permissão: `LIDER` e `USER`  
- Sem corpo de requisição (também respeita as mesmas regras de acesso).

---

## 7. IA (Análise e Previsão)

Base: `/api/v1/ia`  
Permissão geral do controller: somente `LIDER`.

A API utiliza o `OllamaIaService`, que por sua vez usa o Semantic Kernel para conversar com um modelo compatível com OpenAI a partir de prompts construídos usando dados de tarefas e feedbacks.

### 7.1 Análise consolidada de colaborador

- Método: `POST /api/v1/ia/analise-usuario/{idUsuario}`  
- Permissão: somente `LIDER`  
- Sem corpo de requisição.

Retorna um texto de análise agregando feedbacks e histórico do colaborador.

### 7.2 Previsão de atraso de tarefa

- Método: `POST /api/v1/ia/prever-atraso/{idTarefa}`  
- Permissão: somente `LIDER`  
- Sem corpo de requisição.

Retorna um texto com a previsão de risco de atraso para a tarefa selecionada, usando histórico recente do colaborador.

---

## 8. Health Checks

### 8.1 Endpoints abertos

Estes endpoints são configurados diretamente no `Program.cs` usando `MapHealthChecks` e não exigem autenticação:

- `GET /health/live` – liveness  
- `GET /health/ready` – readiness (inclui check de Oracle)  

Além disso, há o controller:

- `GET /api/v1/health` – retorna um JSON consolidado com o status dos health checks registrados (também aberto, anotado com `[AllowAnonymous]`).

Todos são apenas de leitura e não possuem corpo de requisição.

---

## 9. Resumo de Uso e Observação sobre ORA-02391

Para testar a API e consultar todos os detalhes técnicos (rotas, parâmetros, códigos de resposta), utilize exclusivamente o Swagger oficial hospedado:

- `https://task-mirror-api-dotnet.onrender.com/swagger/index.html`

Com o Swagger você pode:

- Visualizar todos os endpoints disponíveis.  
- Conferir parâmetros de rota e query string.  
- Enviar requisições de teste usando os JSONs de exemplo deste README.

É importante **não** tentar subir a API localmente apontando para o mesmo banco Oracle utilizado. O Oracle aplicado tem limite de sessões simultâneas por usuário (`SESSIONS_PER_USER`). Quando esse limite é ultrapassado, ocorre o erro:

> `ORA-02391: exceeded simultaneous SESSIONS_PER_USER limit`

Como o deploy em .NET no Render já consome sessões do banco para atender o Swagger e as integrações com outros projetos da disciplina, abrir mais sessões a partir de várias instâncias locais pode estourar esse limite. Isso pode:

- Causar falhas nas outras entregas que dependem das mesmas tabelas / schemas.  
- Derrubar momentaneamente as conexões da aplicação já publicada.  

Por esse motivo, a recomendação para avaliação é:

1. Usar apenas o endpoint publicado no Render (`https://task-mirror-api-dotnet.onrender.com`) como backend.  
2. Testar todas as rotas diretamente pelo Swagger UI.  
3. Utilizar os JSONs deste README como referência mínima para montar as requisições.

Dessa forma, o banco de dados compartilhado não é sobrecarregado com sessões extras, e todas as entregas que consomem essa API continuam funcionando corretamente.
