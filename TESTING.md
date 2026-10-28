# Testing HexCatalog

## Unit and integration

```bash
./scripts/test.sh
```

```powershell
.\scripts\test.ps1
```

- `api/tests/HexCatalog.UnitTests`: domain rules and MediatR handlers with mocked ports.
- `api/tests/HexCatalog.IntegrationTests`: the real HTTP pipeline through `WebApplicationFactory`, on EF in-memory. Covers auth, role checks, validation problems, paging, stock reservation, and user admin.
- `web/tests/HexCatalog.Admin.UnitTests`: the admin's application service and API error mapping.

CI also runs `dotnet ef migrations has-pending-model-changes` so a model change can't ship without a migration.

## End to end

Needs Docker and the Playwright CLI (`npm install -g playwright`).

```bash
./scripts/e2e.sh
```

The script brings up `infra/docker-compose.yml` and runs `e2e/ui.spec.ts` against http://localhost:5152. It signs in as the seeded admin, creates a product, reserves stock, deletes it, and checks that a new registration lands on the approval notice.

## Manual check

1. `./scripts/run.sh`
2. Open http://localhost:5152 and sign in as `catalogadmin` / `Admin@1234`.
3. Create an account in a private window, then promote it to User from the Users page.
4. Sign in as that account and reserve stock on a product.
