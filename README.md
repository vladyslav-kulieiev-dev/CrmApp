# CrmApp

Aplikacja CRM do obsługi zadań, projektów i kontrahentów w firmie usługowej, zintegrowana z systemem zgłoszeń **Zoho Desk**.

Backend: **ASP.NET Core 8 Web API**. Frontend: **Angular 21** (Angular Material). Baza danych: **SQL Server**.

## Funkcje

- **Zadania**
  - widok „Moje zadania” oraz panel administracyjny ze statystykami i obciążeniem użytkowników,
  - automatyczna numeracja zadań (np. `ZAD/9/2026/1`), priorytety, statusy, postęp i terminy,
  - komentarze i powiązanie zadania z kontrahentem, osobą kontaktową i projektem.
- **Integracja z Zoho Desk**
  - pobieranie zgłoszeń jako zadań,
  - aktualizacja zgłoszeń, komentarze i odpowiedzi do klienta,
  - komunikacja przez serwer MCP (Model Context Protocol).
- **Kontrahenci**
  - dane firmy i dodatkowe numery NIP,
  - osoby kontaktowe,
  - umowy i pakiety godzin wraz z historią rozliczeń,
  - licencje na produkty z historią zmian,
  - import z pliku CSV.
- **Projekty**: cykl życia projektu (rozpoczęcie, zamknięcie, anulowanie) i członkowie zespołu.
- **Produkty i usługi**: katalog z kategoriami, jednostkami, cenami i stawkami VAT.
- **Konfiguracja bez zmian w kodzie**
  - słowniki (statusy, priorytety, waluty, jednostki…),
  - pola dodatkowe definiowane dla wybranych tabel, z uprawnieniami per rola lub użytkownik,
  - ustawienia systemowe.
- **Użytkownicy i uprawnienia**: role mapowane na uprawnienia, sprawdzane po stronie serwera.
- **Listy**: stronicowanie, filtrowanie, sortowanie po stronie serwera i eksport do Excela.

## Technologie

| Obszar           | Technologie                                                                  |
|------------------|------------------------------------------------------------------------------|
| Backend          | .NET 8, ASP.NET Core Web API, ASP.NET Core Identity (uwierzytelnianie cookie)|
| Dostęp do danych | Entity Framework Core 8, Dapper (zapytania list stronicowanych), SQL Server  |
| Migracje         | FluentMigrator (schemat aplikacji), EF Core Migrations (schemat Identity)    |
| Integracje       | Zoho Desk przez MCP (`ModelContextProtocol`), SMTP                           |
| Frontend         | Angular 21, Angular Material, RxJS, Signals                                  |
| Inne             | CsvHelper, SheetJS (xlsx), Swagger                                           |

## Architektura

Rozwiązanie jest podzielone według zasad Clean Architecture:

```
CrmApp.Domain          encje, DTO, enumy, role i uprawnienia
CrmApp.Application     interfejsy repozytoriów i serwisów, logika biznesowa
CrmApp.Infrastructure  EF Core, repozytoria, zapytania Dapper, Identity, rejestracja DI
CrmApp.Migrations      migracje FluentMigrator
CrmApp.Api             kontrolery REST, middleware obsługi wyjątków, konfiguracja
CrmApp.Client          aplikacja Angular
```

Najważniejsze decyzje projektowe:

- **Dwa konteksty EF Core w jednej bazie.** `ApplicationDbContext` obsługuje tabele ASP.NET Identity (migracje EF). `AppDbContext` obsługuje dane aplikacji, których schematem zarządza FluentMigrator.
- **Uprawnienia oparte na politykach.** Kontrolery używają `[Authorize(Policy = "perm:...")]`. Własny `PermissionPolicyProvider` i `PermissionAuthorizationHandler` sprawdzają uprawnienia w bazie, a nie w ciasteczku.
- **Jednolity wynik operacji.** Serwisy zwracają `ResultDTO<T>`. Wyjątki domenowe (`NotFoundException`, `ValidationException`, `ConflictException`…) są mapowane przez middleware na odpowiedzi `ProblemDetails`.
- **Listy stronicowane w SQL.** Budowniczowie zapytań oparci na Dapperze obsługują filtrowanie, sortowanie i dynamiczne pola dodatkowe.

## Uruchomienie

### Wymagania

- .NET 8 SDK
- Node.js 22 lub nowszy
- SQL Server (np. Developer lub Express)
- serwer SMTP, np. MailHog

Przy pierwszym uruchomieniu aplikacja wysyła mail powitalny do administratora, dlatego serwer SMTP musi być dostępny.

### Start

Profil `https` uruchamia API (`https://localhost:7219`, Swagger: `/swagger`) oraz serwer deweloperski Angulara. Aplikacja jest dostępna pod adresem **https://localhost:4200**.

W Visual Studio wystarczy wybrać projekt `CrmApp.Api` i profil `https`, a następnie nacisnąć F5.

Przy każdym starcie API automatycznie:

1. tworzy lub aktualizuje schemat bazy (migracje EF Core dla Identity i FluentMigrator dla danych aplikacji),
2. wypełnia słowniki danymi startowymi,
3. tworzy role, przypisuje im uprawnienia i zakłada konto administratora z konfiguracji.

### Integracja z Zoho Desk (opcjonalnie)

Po zalogowaniu jako administrator uzupełnij w **Zarządzanie → Ustawienia systemu** adres i nazwę serwera MCP Zoho Desk oraz identyfikatory organizacji i działu.
