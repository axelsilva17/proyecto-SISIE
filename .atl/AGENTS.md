# Agentes del Proyecto — proyecto-SISIE

## Agente Backend (Axel)

**Rama de trabajo:** `Axel`  
**Rama destino:** `Principal` (vía PR)

### Responsabilidades
- CRUD Productos completo (GET, POST, PUT, DELETE)
- CRUD Categorías simple
- Auth (register, login, logout, me)
- Entidades, servicios, DbContext
- Validaciones de negocio

### Stack
- .NET 8 Web API
- EF Core Identity + SQLite
- JWT Bearer

### Endpoints a mantener
- `/api/auth/register`, `/api/auth/login`, `/api/auth/logout`, `/api/auth/me`
- `/api/productos` (GET, POST), `/api/productos/{id}` (GET, PUT, DELETE)
- `/api/categorias` (GET, POST), `/api/categorias/{id}` (GET, PUT, DELETE)

---

## Agente Frontend (Nico)

**Rama de trabajo:** `Nico`  
**Rama destino:** `Principal` (vía PR)

### Responsabilidades
- Pages HTML/Tailwind
- Consumo de API
- Registro de usuarios
- Login
- Dashboard productos
- CRUD visual de productos y categorías

### Stack
- HTML
- Tailwind CSS (CDN)
- vanilla JavaScript

### Páginas a crear
- `login.html` — Login/Registro
- `register.html` — Registro de usuarios
- `index.html` — Dashboard/Listado productos
- `productos.html` — CRUD productos
- `categorias.html` — CRUD categorías

### API a consumir
- `POST /api/auth/register`
- `POST /api/auth/login`
- `GET /api/productos`
- `POST /api/productos`
- `PUT /api/productos/{id}`
- `DELETE /api/productos/{id}`
- `GET /api/categorias`
- `POST /api/categorias`
- `PUT /api/categorias/{id}`
- `DELETE /api/categorias/{id}`

### JWT Token
- Guardar en `localStorage` con key `token`
- Enviar en headers: `Authorization: Bearer {token}`

---

## Workflow

1. Cada agente trabaja en SU rama (`Axel` o `Nico`)
2. Cuando termina, crea PR hacia `Principal`
3. Review y merge a main

---

## Links
- Repo: https://github.com/axelsilva17/proyecto-SISIE
- Rama main: https://github.com/axelsilva17/proyecto-SISIE/tree/main