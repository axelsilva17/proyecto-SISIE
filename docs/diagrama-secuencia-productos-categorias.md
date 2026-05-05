# Diagrama de Secuencia - Sistema SISIE

## Módulos: Productos y Categorías

---

### 📦 OBTENER PRODUCTOS

👤 Administrador → Sistema: Accede a productos
Sistema → Productos: Llama ObtenerTodosAsync
Productos → Productos: Aplica filtros (idCategoria, activo)
Productos → Base de Datos: SELECT con paginación
Base de Datos → Productos: Lista de productos
Productos → Sistema: Lista paginada
Sistema → Administrador: Muestra tabla de productos

---

### ➕ CREAR PRODUCTO

👤 Administrador → Sistema: Completa formulario
Sistema → Productos: Llama CrearAsync
Productos → Productos: ValidarNombreUnico (case-insensitive)
    ⚠️SInombreexiste→ Sistema: Error "Ya existe un producto con ese nombre"
    ✅SINOHayduplicado →
Productos → Productos: ValidarCategoriaExiste
    ⚠️SIcategoríanoexiste→Sistema:Error"La categoría no existe"
    ✅SIexiste →
Productos → Base de Datos: INSERT producto
Base de Datos → Productos: Producto creado
Productos → Sistema: ProductoDTO (creado)
Sistema → Administrador: "Producto creado exitosamente"

---

### ✏️ EDITAR PRODUCTO

👤 Administrador → Sistema: Edita campos y guarda
Sistema → Productos: Llama ActualizarAsync(id, datos)
Productos → Base de Datos: UPDATE producto
Base de Datos → Productos: Producto actualizado
Productos → Sistema: ProductoDTO (actualizado)
Sistema → Administrador: Producto actualizado

---

### 🚫 DESACTIVAR PRODUCTO (Soft Delete)

👤 Administrador → Sistema: Cambia estado (act/des)
Sistema → Productos: Llama ObtenerPorIdAsync
Productos → Base de Datos: Busca producto
Base de Datos → Productos: Producto encontrado
Productos → Productos: ToggleActivoAsync
Productos → Base de Datos: UPDATE Activo = !Activo
Base de Datos → Productos: Estado actualizado
Productos → Sistema: Nuevo estado
Sistema → Administrador: Refleja nuevo estado

---

### 📂 OBTENER CATEGORÍAS

👤 Administrador → Sistema: Solicita lista de categorías
Sistema → Categorías: Llama ObtenerTodosAsync
Categorías → Base de Datos: SELECT categorías ORDER BY nombre
Base de Datos → Categorías: Lista de categorías
Categorías → Sistema: List<CategoriaDTO>
Sistema → Administrador: Muestra lista

---

### ➕ CREAR CATEGORÍA

👤 Administrador → Sistema: Ingresa nombre de categoría
Sistema → Categorías: Llama CrearAsync
Categorías → Categorías: ValidarNombreUnico (case-insensitive)
    ⚠️SIexiste→ Sistema: Error "Ya existe una categoría con ese nombre"
    ✅SINOHay →
Categorías → Base de Datos: INSERT categoría
Base de Datos → Categorías: Categoría creada
Categorías → Sistema: CategoriaDTO (creada)
Sistema → Administrador: "Categoría creada exitosamente"

---

### ✏️ EDITAR CATEGORÍA

👤 Administrador → Sistema: Edita nombre y guarda
Sistema → Categorías: Llama ActualizarAsync(id, nombre)
Categorías → Base de Datos: UPDATE categoría
Base de Datos → Categorías: Categoría actualizada
Categorías → Sistema: CategoriaDTO (actualizada)
Sistema → Administrador: Categoría actualizada

---

### 🚫 ELIMINAR CATEGORÍA

👤 Administrador → Sistema: Solicita eliminar categoría
Sistema → Categorías: Llama PuedeEliminarAsync
Categorías → Base de Datos: COUNT productos activos de esta categoría
Base de Datos → Categorías: Conteo resultado
    ⚠️SITIENEproductos→Categorías:false
    Sistema → Administrador: Error "No se puede eliminar, tiene productos vinculados"
    ✅SINOTIENE →
Categorías → Categorías: Llama EliminarAsync
Categorías → Base de Datos: DELETE categoría
Base de Datos → Categorías: Categoría eliminada
Categorías → Sistema: true
Sistema → Administrador: "Categoría eliminada"

---

## 📋 RESUMEN DE MÉTODOS

### ProductoService (Productos)
| Método | Descripción |
|--------|------------|
| ObtenerTodosAsync | Lista con paginación y filtros |
| ObtenerPorIdAsync | Busca por ID |
| CrearAsync | Crea (valida nombre + categoría) |
| ActualizarAsync | Actualiza datos |
| EliminarAsync | Soft delete (marca inactivo) |
| ToggleActivoAsync | Cambia estado activo/inactivo |

### CategoriaService (Categorías)
| Método | Descripción |
|--------|------------|
| ObtenerTodosAsync | Lista todas |
| ObtenerPorIdAsync | Busca por ID |
| CrearAsync | Crea (valida nombre único) |
| ActualizarAsync | Actualiza nombre |
| EliminarAsync | Elimina física |
| PuedeEliminarAsync | Verifica sin productos activos |

---

## ✅ VALIDACIONES

| Entidad | Acción | Validaciones |
|--------|--------|-------------|
| **Producto** | Crear | Nombre único + Categoría existe |
| **Categoría** | Crear | Nombre único |
| **Categoría** | Eliminar | Solo si no tiene productos activos |
| **Todos** | Datos | DataAnnotations (required, range, etc.) |