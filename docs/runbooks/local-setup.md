# Local setup

Runs PostgreSQL, `core` and `gateway-bff` on your machine with the production images and the production database roles ([ADR-018](../adr/adr-018-environments-and-configuration.md)).

## Prerequisites

- Docker with Compose v2 (`docker compose version`)
- Nothing else: the .NET SDK runs inside the build image

## Start

From `infra/`:

```bash
cp .env.example .env
# Give every password its own random value (letters and digits only):
for key in $(grep -o '^[A-Z_]*_PASSWORD' .env); do sed -i "s/^$key=$/$key=$(openssl rand -hex 24)/" .env; done
docker compose up --build --wait
```

`--wait` returns once every container is healthy, or fails if one is not.

| Service | Reachable from your machine | Notes |
|---|---|---|
| `gateway-bff` | `http://localhost:5000` | The only entry point, as in production |
| `core` | no | Only the other containers reach it |
| `postgres` | `localhost:5432`, or `POSTGRES_HOST_PORT` if set | Bound to 127.0.0.1 only |

If port 5432 is already used by another project, set `POSTGRES_HOST_PORT=5440` (or any free port) in `.env`.

## Databases and roles

`postgres/init/01-databases.sql` runs on the first start only, when the `postgres-data` volume is empty. For each of `bff_db`, `account_db`, `menu_db`, `calendar_db`:

- `{db}_migrator` owns the database and its schema, for migrations (DDL)
- `{db}_app` is what the services use: SELECT, INSERT, UPDATE, DELETE, nothing else
- no other role can connect

After changing a password in `.env` or the script itself, recreate the volume (deletes local data):

```bash
docker compose down --volumes
docker compose up --build --wait
```

## Checks

The database rules are also covered by `tests/Infrastructure.IntegrationTests`, which run the same script against a throwaway PostgreSQL. These manual checks confirm the running environment.

Load the passwords into your shell first: `set -a; . ./.env; set +a`

**1. The three containers are healthy**

```bash
docker compose ps
```
Expected: `(healthy)` on `postgres`, `core` and `gateway-bff`. Then `curl -i localhost:5000/health/live` answers `200`.

**2. `menu_db_app` cannot create a table**

```bash
docker compose exec -e PGPASSWORD="$MENU_DB_APP_PASSWORD" postgres \
  psql -h 127.0.0.1 -U menu_db_app -d menu_db -c 'CREATE TABLE intruder (id int);'
```
Expected: `ERROR: permission denied for schema public`.

**3. `menu_db_app` cannot connect to `calendar_db`**

```bash
docker compose exec -e PGPASSWORD="$MENU_DB_APP_PASSWORD" postgres \
  psql -h 127.0.0.1 -U menu_db_app -d calendar_db -c 'SELECT 1;'
```
Expected: `FATAL: permission denied for database "calendar_db"`.

**4. The services do not run as root**

The images have no shell, so `docker exec core whoami` cannot run. Ask Docker instead:

```bash
docker compose top core gateway-bff
```
Expected: `UID` is `1654` (the `app` user of the .NET images), never `0` (root).

## Stop

```bash
docker compose down            # keeps the data
docker compose down --volumes  # deletes the data too
```

## Secrets

`.env` is gitignored; only `.env.example`, with empty values, is committed. Once #9 is done, inject secrets from Infisical instead of `.env`: `infisical run -- docker compose up`.

## Frontend

From `src/frontend/`, with Node from `.nvmrc` ([nvm](https://github.com/nvm-sh/nvm)):

```bash
nvm use
npm ci          # installs exactly what package-lock.json lists
npm start       # http://localhost:4200
npm run lint    # includes the library boundary rules (ADR-015)
npm test        # unit tests of every project, then the boundary rule tests
```

The SPA does not call the BFF yet (#17).
