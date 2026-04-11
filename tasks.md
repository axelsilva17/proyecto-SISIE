# Tasks: SISIE Entrega 1 - Login + Productos + Categorías

## Phase 1: Setup Inicial

- [ ] 1.1 Crear proyecto .NET 8 Web API con dotnet new webapi
- [ ] 1.2 Agregar paquete NuGet Microsoft.EntityFrameworkCore.Sqlite
- [ ] 1.3 Agregar paquete NuGet Microsoft.AspNetCore.Identity.EntityFrameworkCore
- [ ] 1.4 Agregar paquete NuGet System.IdentityModel.Tokens.Jwt
- [ ] 1.5 Agregar paquete NuGet Microsoft.EntityFrameworkCore.Design (para migrations)
- [ ] 1.6 Configurar appsettings.json con ConnectionString SQLite
- [ ] 1.7 Configurar appsettings.json con JWT Settings (Key, Issuer, Audience, Expiry)
- [ ] 1.8 Configurar Program.cs con DbContext, Identity, JWT, CORS, StaticFiles

## Phase 2: Modelo de Datos

- [ ] 2.1 Crear entidad Models/Entities/Categoria.cs (Id, Nombre, Descripcion, Activo,timestamps)
- [ ] 2.2 Crear entidad Models/Entities/Producto.cs (Id, Nombre, Descripcion, Precio, Stock, CategoriaId, Activo, timestamps)
- [ ] 2.3 Crear Models/Entities/ApplicationUser.cs (extiende IdentityUser)
- [ ] 2.4 Crear Data/ApplicationDbContext.cs (DbContext + IdentityDbContext)
- [ ] 2.5 Configurar relaciones en OnModelCreating (Producto-Categoria, Identity)
- [ ] 2.6 Ejecutar dotnet ef migrations add InitialCreate
- [ ] 2.7 Ejecutar dotnet ef database update
- [ ] 2.8 Verificar tablas creadas en SQLite con sqlite3

## Phase 3: Servicios (Capa Negocio)

- [ ] 3.1 Crear Services/Interfaces/ICategoriaService.cs
- [ ] 3.2 Crear Services/Implementations/CategoriaService.cs (GetAll, GetById, Create, Update, Delete)
- [ ] 3.3 Crear Services/Interfaces/IProductoService.cs
- [ ] 3.4 Crear Services/Implementations/ProductoService.cs (GetAll, GetById, GetByCategoria, Create, Update, Delete)
- [ ] 3.5 Crear Services/Interfaces/IAuthService.cs
- [ ] 3.6 Crear Services/Implementations/AuthService.cs (Register, Login, GenerateJWT)
- [ ] 3.7 Implementar soft delete en servicios (marcar Activo = false)

## Phase 4: Controllers API

- [ ] 4.1 Crear Controllers/AuthController.cs (POST /register, POST /login, GET /me)
- [ ] 4.2 Crear Controllers/CategoriasController.cs (GET, POST, PUT, DELETE con authorize)
- [ ] 4.3 Crear Controllers/ProductosController.cs (GET, POST, PUT, DELETE con authorize)
- [ ] 4.4 Agregar [Authorize] attribute a endpoints protegidos
- [ ] 4.5 Configurar JWT Bearer authentication en Program.cs
- [ ] 4.6 Probar endpoints con Postman (registro, login, CRUD productos, CRUD categorías)

## Phase 5: Frontend wwwroot/

- [ ] 5.1 Configurar Tailwind CSS via CDN en index.html
- [ ] 5.2 Crear wwwroot/css/styles.css (custom styles)
- [ ] 5.3 Crear wwwroot/login.html (formulario login, register, JS)
- [ ] 5.4 Crear wwwroot/index.html (dashboard con menú navegación)
- [ ] 5.5 Crear wwwroot/productos.html (lista + modal/formulario CRUD)
- [ ] 5.6 Crear wwwroot/categorias.html (lista + modal/formulario CRUD)
- [ ] 5.7 Crear wwwroot/js/api.js (wrapper fetch con headers JWT)
- [ ] 5.8 Crear wwwroot/js/auth.js (token storage, isAuthenticated, logout)
- [ ] 5.9 Crear wwwroot/js/main.js (inicialización, routing)
- [ ] 5.10 Crear wwwroot/js/productos.js (fetch, render, CRUD)
- [ ] 5.11 Crear wwwroot/js/categorias.js (fetch, render, CRUD)
- [ ] 5.12 Integrar frontend con API (token en headers, redirecciones)

## Phase 6: Pruebas y Ajustes

- [ ] 6.1 Probar registro de usuarios (valida email único, password válido)
- [ ] 6.2 Probar login (retorna JWT, token guardado en localStorage)
- [ ] 6.3 Probar acceso con token válido vs inválido
- [ ] 6.4 Probar CRUD Productos (crear, listar, editar, eliminar)
- [ ] 6.5 Probar CRUD Categorías (crear, listar, editar, eliminar)
- [ ] 6.6 Probar soft delete (producto eliminado no aparece en lista pero existe)
- [ ] 6.7 Verificar validaciones (campos requeridos, rangos válidos)
- [ ] 6.8 Ajustar mensajes de error en frontend

## Implementation Order

1. **Phase 1** → Setup proyecto y dependencias (fundamento para todo)
2. **Phase 2** → Modelo de datos (base para servicios)
3. **Phase 3** → Servicios (lógica de negocio, sin controllers aún)
4. **Phase 4** → Controllers (conectan servicios a HTTP)
5. **Phase 5** → Frontend (consume API controllers)
6. **Phase 6** → Testing end-to-end

**Nota**: Las fases 1-4 pueden completarse en una sesión. Fase 5 (frontend) requiere tiempo adicional. Fase 6 es testing de integración.