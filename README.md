# proyecto-SISIE

Sistema Integral de Gestión para una Ferretería / Local comercial. Backend REST API + Frontend vanilla.

## Descripción

SISIE es un sistema de gestión integral para ferreterías y locales comerciales que permetellevar el control de inventario, ventas, clientes y categorías de productos. Construido con .NET 8 Web API y frontend vanilla HTML/Tailwind CSS.

## Características

- 🔐 **Autenticación**: Registro y login con JWT
- 📦 **Gestión de Productos**: CRUD completo con paginación
- 🏷️ **Categorías**: Clasificación de productos
- 🛒 **Ventas**: Módulo de ventas con carrito de compras
- 📊 **Dashboard**: Estadísticas rápidas del negocio
- 📄 **Historial**: Registro de ventas realizadas
- 🔄 **Estados**:Control de productos activos/inactivos

## Tech Stack

| Capa | Tecnología |
|------|------------|
| Backend | .NET 8 Web API |
| ORM | Entity Framework Core |
| Base de datos | SQLite |
| Auth | JWT Bearer |
| Frontend | HTML5 + Tailwind CSS |
| JS | Vanilla JavaScript |

## Requisitos Previos

- [.NET 8 SDK](https://dotnet.microsoft.com/download)
- Navegador moderno (Chrome, Firefox, Edge)

## Instalación

```bash
# 1. Clonar el repositorio
git clone https://github.com/axelsilva17/proyecto-SISIE.git
cd proyecto-SISIE

# 2. Restaurar paquetes
dotnet restore

# 3. Compilar
dotnet build
```

## Usage

```bash
# Ejecutar el servidor
dotnet run
```

El servidor estará disponible en: **http://localhost:5000**

### Páginas Disponibles

| Página | Descripción |
|--------|-------------|
| `/` | Dashboard principal |
| `/login.html` | Login de usuario |
| `/register.html` | Registro de nuevos usuarios |
| `/productos.html` | Gestión de productos |
| `/categorias.html` | Gestión de categorías |
| `/ventas.html` | Módulo de ventas |

### API Endpoints

#### Auth
- `POST /api/auth/register` - Registrar usuario
- `POST /api/auth/login` - Iniciar sesión
- `GET /api/auth/me` - Obtener datos del usuario actual

#### Productos
- `GET /api/productos` - Listar productos (paginado)
- `GET /api/productos/{id}` - Obtener producto por ID
- `POST /api/productos` - Crear producto
- `PUT /api/productos/{id}` - Actualizar producto
- `DELETE /api/productos/{id}` - Desactivar producto
- `PATCH /api/productos/{id}/toggle` - Activar/desactivar producto

#### Categorías
- `GET /api/categorias` - Listar categorías
- `GET /api/categorias/{id}` - Obtener categoría por ID
- `POST /api/categorias` - Crear categoría
- `PUT /api/categorias/{id}` - Actualizar categoría
- `DELETE /api/categorias/{id}` - Eliminar categoría

## Ramas de Desarrollo

| Rama | Descripción |
|------|-------------|
| `main` | Rama principal/producción |
| `Principal` | Rama de integración |
| `Nico` | Desarrollo frontend |
| `Axel` | Desarrollo backend |

## Contribuidores

- [axelsilva17](https://github.com/axelsilva17)
- [NicolasPucheta](https://github.com/NicolasPucheta)

