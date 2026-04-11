# Design: Sistema Venta Hardware - Entrega 1

## Technical Approach

Monolito 3 capas con arquitectura cliente-servidor. Backend .NET 8 Web API con EF Core + SQLite, autenticación JWT Bearer, frontend estático en wwwroot (HTML/Tailwind/JS). Entrega 1 fociada en autenticación, gestión de productos y categorías.

## Architecture Decisions

### Decision: Authentication Strategy

**Choice**: EF Core Identity con JWT Bearer  
**Alternatives considered**: Session-based auth, pure Identity without JWT  
**Rationale**: JWT es ideal para APIs REST stateless, permite scale horizontal sin estado de sesión, y se integra naturalmente con el frontend vanilla JS

### Decision: Repository Pattern

**Choice**: Sin repositorio — acceso directo a DbContext en servicios  
**Alternatives considered**: Repository pattern completo, Unit of Work  
**Rationale**: Proyecto de alcance limitado (Entrega 1). DbContext ya es un Unit of Work. Complejidad innecesaria para CRUD básico.

### Decision: Soft Delete

**Choice**: Campo `Activo` boolean en Producto  
**Alternatives considered**: DeletedAt timestamp, tabla paralelo de eliminados  
**Rationale**: Simplicidad para Entrega 1. No perder historial de ventas vinculadas. Evita FK constraint issues.

### Decision: Pagination

**Choice**: Query strings `page` y `pageSize`  
**Alternatives considered**: Offset-based, cursor-based  
**Rationale**: Estándar para APIs .NET, fácil de implementar con LINQ Skip/Take, compatible con cualquier frontend

### Decision: Frontend Architecture

**Choice**: Vanilla JS con módulos separados + Tailwind CDN  
**Alternatives considered**: React/Vue SPA, Blazor  
**Rationale**: El usuario explicitó "HTML/Tailwind/JS". Mantiene simplicidad, sin build step adicional más alla de Tailwind

## Data Flow

```
┌─────────────────────────────────────────────────────────────┐
│                      FRONTEND (wwwroot/)                    │
│  index.html → login.html → productos.html → categorias.html │
│         │              │              │                    │
│         └──────────────┼──────────────┘                    │
│                        ▼                                    │
│              js/api.js (fetch wrapper)                     │
│                        │                                    │
└────────────────────────┼────────────────────────────────────┘
                         ▼
┌─────────────────────────────────────────────────────────────┐
│                   BACKEND (Controllers/)                    │
│   AuthController   ProductosController   CategoriasController│
│         │                   │                    │          │
│         └───────────────────┼────────────────────┘          │
│                             ▼                                │
│              js/Services/ (AuthService, etc)                │
│                             │                                │
└─────────────────────────────┼───────────────────────────────┘
                              ▼
┌─────────────────────────────────────────────────────────────┐
│                   DATA LAYER (EF Core)                       │
│              ApplicationDbContext                            │
│                        │                                    │
│                   ┌────┴────┐                                │
│                   ▼         ▼                                │
│              Categorias   Productos                         │
│              (IdentityUser)                                  │
└─────────────────────────────────────────────────────────────┘
```

## File Changes

| File | Action | Description |
|------|--------|-------------|
| `proyecto-SISIE.csproj` | Create | .NET 8 Web API project |
| `Program.cs` | Create | Entry point, DI container, middleware |
| `appsettings.json` | Create | Configuration (JWT, connection string) |
| `Controllers/AuthController.cs` | Create | Register, login, logout, me endpoints |
| `Controllers/ProductosController.cs` | Create | CRUD productos con paginación |
| `Controllers/CategoriasController.cs` | Create | CRUD categorías |
| `Services/IAuthService.cs` | Create | Interfaz servicio auth |
| `Services/AuthService.cs` | Create | Implementación JWT generation |
| `Services/IProductoService.cs` | Create | Interfaz servicio productos |
| `Services/ProductoService.cs` | Create | CRUD productos con soft delete |
| `Services/ICategoriaService.cs` | Create | Interfaz servicio categorías |
| `Services/CategoriaService.cs` | Create | CRUD categorías con validación |
| `Data/ApplicationDbContext.cs` | Create | DbContext con Identity + entidades |
| `Models/Entities/ApplicationUser.cs` | Create | User entity extendiendo IdentityUser |
| `Models/Entities/Categoria.cs` | Create | Entidad categoría |
| `Models/Entities/Producto.cs` | Create | Entidad producto |
| `Models/DTOs/LoginRequest.cs` | Create | DTO login |
| `Models/DTOs/RegisterRequest.cs` | Create | DTO registro |
| `Models/DTOs/ProductoDTO.cs` | Create | DTO producto |
| `Models/DTOs/CategoriaDTO.cs` | Create | DTO categoría |
| `wwwroot/index.html` | Create | Landing page |
| `wwwroot/login.html` | Create | Login/registro |
| `wwwroot/productos.html` | Create | Gestión productos |
| `wwwroot/categorias.html` | Create | Gestión categorías |
| `wwwroot/js/api.js` | Create | Fetch wrapper con JWT |
| `wwwroot/js/auth.js` | Create | Lógica auth |
| `wwwroot/js/productos.js` | Create | UI productos |
| `wwwroot/js/categorias.js` | Create | UI categorías |
| `wwwroot/js/main.js` | Create | Inicialización |
| `wwwroot/css/styles.css` | Create | Tailwind compilado |

## Interfaces / Contracts

### API Contracts

```csharp
// Auth
POST /api/auth/register { "email", "password", "userName" } → IdentityResult
POST /api/auth/login { "email", "password" } → { "token", "user" }
POST /api/auth/logout → 204 No Content
GET  /api/auth/me → { "id", "email", "userName" }

// Productos
GET  /api/productos?page=1&pageSize=10&idCategoria=1&activo=true → { items[], total, page, pageSize }
GET  /api/productos/{id} → ProductoDTO
POST /api/productos → ProductoDTO (created)
PUT  /api/productos/{id} → ProductoDTO (updated)
DELETE /api/productos/{id} → 204 (soft delete)

// Categorías
GET  /api/categorias → CategoriaDTO[]
GET  /api/categorias/{id} → CategoriaDTO
POST /api/categorias → CategoriaDTO
PUT  /api/categorias/{id} → CategoriaDTO
DELETE /api/categorias/{id} → 409 si tiene productos, 204 si vacío
```

### Service Interfaces

```csharp
public interface IProductoService
{
    Task<(List<Producto> items, int total)> GetAllAsync(int page, int pageSize, int? idCategoria, bool? activo);
    Task<Producto?> GetByIdAsync(int id);
    Task<Producto> CreateAsync(Producto producto);
    Task<Producto> UpdateAsync(int id, Producto producto);
    Task<bool> DeleteAsync(int id); // soft delete
}

public interface ICategoriaService
{
    Task<List<Categoria>> GetAllAsync();
    Task<Categoria?> GetByIdAsync(int id);
    Task<Categoria> CreateAsync(Categoria categoria);
    Task<Categoria> UpdateAsync(int id, Categoria categoria);
    Task<bool> CanDeleteAsync(int id);
    Task<bool> DeleteAsync(int id);
}

public interface IAuthService
{
    Task<IdentityResult> RegisterAsync(RegisterRequest request);
    Task<string?> LoginAsync(LoginRequest request);
    Task<ApplicationUser?> GetCurrentUserAsync(string userId);
}
```

## Testing Strategy

| Layer | What to Test | Approach |
|-------|-------------|----------|
| Unit | Services (ProductoService, CategoriaService, AuthService) | xUnit con mock de DbContext |
| Integration | Controllers (HTTP endpoints) | WebApplicationFactory + in-memory DB |
| E2E | Frontend flows | Manual testing o Playwright |

**Nota**: Testing no incluido en alcance de Entrega 1 según el plan original.Queda como deuda técnica.

## Migration / Rollout

```bash
# Package restore
dotnet restore

# Create initial migration
dotnet ef migrations add InitialCreate

# Apply to SQLite
dotnet ef database update

# Run
dotnet run
```

**No data migration required** — proyecto greenfield.

## Open Questions

- [ ] ¿Se requiere validación de stock en venta? (fuera de alcance Entrega 1)
- [ ] ¿Roles de usuario (admin/vendedor)? Por ahora todos tienen acceso completo
- [ ] ¿Log de auditoría? No en Entrega 1
- [ ] ¿Unit tests obligatorios? Suggestido pero no planificado

---

**Next Step**: Listo para implementación (sdd-tasks → sdd-apply)