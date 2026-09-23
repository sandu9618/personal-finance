# Personal Finance

Personal Finance is an ASP.NET Core Web API for tracking your own money. After you create an account and sign in, you can keep multiple wallets (cash, bank, savings, credit card), record income and expenses, put each transaction in a category, and see a simple summary of where the money went.

Each person’s data is private. The API identifies you from the login token, so one user cannot see another user’s accounts or transactions.

The product stays small on purpose: one API and one PostgreSQL database. It does not include budgets, recurring payments, or notifications.

## What it does

- **Sign up and sign in** — Identity stores the user and password; a JWT is used for later requests.
- **Accounts** — Named balances such as Cash, Bank, Savings, or Credit Card, typically in LKR.
- **Categories** — Labels like Food, Transport, Salary, or custom ones you add.
- **Transactions** — An amount, income or expense, the account it hit, the category, an optional description, and the date. Creating a transaction updates that account’s balance.
- **Summary** — Totals for income, expenses, remaining balance, spending by category, and recent activity.

## How it is organized

The API sits in front of a domain model and PostgreSQL:

```text
Client
  → ASP.NET Core API
      → Application (use cases)
      → Domain (Account, Category, Transaction, ApplicationUser)
      → Infrastructure (EF Core, Identity, JWT)
          → PostgreSQL
```

Users come from ASP.NET Core Identity (`ApplicationUser`). Finance records belong to that user:

```text
ApplicationUser
  ├── Accounts
  │      └── Transactions
  ├── Categories
  └── Transactions
```

Identity handles authentication. Accounts, categories, and transactions are the finance domain.

## Stack

ASP.NET Core, ASP.NET Core Identity, Entity Framework Core, PostgreSQL, JWT authentication, and a Razor Pages UI.

## Run with Docker

From this folder, with Docker running:

```bash
cp .env.example .env   # optional; change JWT_KEY before a real deploy
docker compose up --build
```

Then open **http://localhost:8080** (the web app). The API is at **http://localhost:8082**. Postgres stays on the Compose network.

The API applies EF migrations on startup. Stop with `docker compose down`. `docker compose down -v` also deletes the database volume.
