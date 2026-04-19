# AGENTS.md

## Run the app
```bash
dotnet run
```
- API: `http://localhost:5000`
- Swagger: `http://localhost:5000/swagger`
- Frontend fallback: serves `wwwroot/index.html` at root

## Database
- SQLite: `SISIE_db.db`
- Migrations run automatically on startup (via `db.Database.Migrate()`)
- Seed data created on first run (categorías y productos)

## Auth
- JWT Bearer tokens
- Config in `appsettings.json` under `Jwt:`
- Token key: `SisieSecretKey2026EsteEsUnKeyMuyLargoParaJWT!`

## API Endpoints
- `POST /api/auth/register`
- `POST /api/auth/login`
- `GET /api/productos`, `POST /api/productos`, `PUT /api/productos/{id}`, `DELETE /api/productos/{id}`
- `GET /api/categorias`, `POST /api/categorias`, `PUT /api/categorias/{id}`, `DELETE /api/categorias/{id}`

## Frontend
Static files in `wwwroot/`. JWT stored in `localStorage` key `token`.

### Pages
- `index.html` — Dashboard principal
- `login.html` — Login/Registro
- `productos.html` — CRUD Productos
- `categorias.html` — CRUD Categorías
- `ventas.html` — Nueva Venta

### Keyboard Shortcuts (index.html)
- `Ctrl+P` — Productos
- `Ctrl+V` — Ventas
- `Ctrl+K` — Categorías
- `Ctrl+L` — Logout

## Useful commands
```bash
dotnet build           # Build
dotnet ef migrations list   # List migrations
dotnet ef database update   # Apply migrations
```