# FileMonitoring
Case Técnico – Desenvolvedor Sênior Full Stack Linx
Sistema para monitoramento de arquivos financeiros (UfCard e FagammonCard).

## 🚀 Tecnologias

- **Backend**: .NET 8/9 (ASP.NET Core Web API)
- **Frontend**: React + Vite + TypeScript
- **Banco de Dados**: SQLite
- **Estilização**: CSS Modules / Vanilla CSS

## 📋 Pré-requisitos

- [.NET SDK](https://dotnet.microsoft.com/download)
- [Node.js](https://nodejs.org/)

## 🔧 Configuração e Execução

### 1. Backend

O backend é responsável por processar os arquivos e gerenciar o banco de dados.

1. Navegue até a pasta da API:
   ```bash
   cd FileMonitoring.API
   ```

2. Execute a aplicação:
   ```bash
   dotnet run
   ```

   > A API iniciará em `http://localhost:5122`.
   > O banco de dados `filemonitoring.db` será criado automaticamente na primeira execução.

### 2. Frontend

O frontend fornece a interface para upload e visualização dos dados.

1. Navegue até a pasta do frontend (em um novo terminal):
   ```bash
   cd FileMonitoring.Web
   ```

2. Instale as dependências:
   ```bash
   npm install
   ```

3. Inicie o servidor de desenvolvimento:
   ```bash
   npm run dev
   ```

   > O frontend estará acessível em `http://localhost:5173`.

## 📦 Como Popular o Banco de Dados

O banco de dados é populado através do upload de arquivos na interface web. O sistema aceita dois formatos de arquivo posicional:

### Formatos Aceitos

#### 1. UfCard (Inicia com "0")
Exemplo de linha válida:
```text
0          20251218        20251201        20251231       0000001UFCARD   
```
*(Certifique-se de respeitar o posicionamento dos caracteres)*

#### 2. FagammonCard (Inicia com "1")
Exemplo de linha válida:
```text
120251218        FAGAMMONCARD0000001
```

### Instruções

1. Abra o frontend no navegador (`http://localhost:5173`).
2. Clique no botão **"Upload Arquivo"**.
3. Selecione um arquivo `.txt` contendo uma linha no formato acima.
4. O sistema processará o arquivo e atualizará a lista de "Arquivos Processados" e o gráfico de status.
