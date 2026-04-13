# PRD - Sprint 1: Registro de Productos y Login

## 1. Descripción del Proyecto

**Nombre del sistema**: SISIE - Sistema de Venta de Hardware por Mostrador

**Tipo de sistema**: Aplicación web para gestión comercial de local de ventas de hardware (tipo ferretería o local de computación).

**Objetivo principal**: Controlar inventario de productos, registrar ventas y gestionar usuarios del sistema.

**Duración estimada del proyecto**: 3 sprints (semanas)

---

## 2. Cómo surge el Proyecto - Planning Inicial

### El inicio: definición del problema

Cuando arrancamos con el proyecto, lo primero fue entender qué necesitábamos construir. El equipo explicó que quería un sistema de venta por mostrador para un local de hardware (similar a una ferretería o local de computación).

### Primera decisión: El Stack Tecnológico

El equipo tenía claro desde el inicio qué tecnologías quería usar:

| Capa | Tecnología Elegida | Para qué sirve |
|------------|---------------------|----------------|
| **Backend** | C# con .NET 8 (Web API) | Procesar información, comunicarse con la base de datos |
| **Frontend** | HTML, CSS, Tailwind CSS, JavaScript | Mostrar las pantallas al usuario |
| **Base de datos** | SQLite | Guardar la información del sistema |
| **Comunicación** | HTTP / JSON | Cómo el frontend habla con el backend |
| **Autenticación** | JWT (JSON Web Tokens) | Mantener al usuario logueado de forma segura |

### Segunda decisión: La Arquitectura

Definimos una arquitectura de **Monolito de 3 capas** (Cliente-Servidor):

1. **Capa de Presentación** (Frontend): Todo lo que el usuario ve - páginas HTML, estilos, JavaScript
2. **Capa de Negocio** (Servicios): La lógica del sistema - qué operaciones se pueden hacer
3. **Capa de Datos**: Cómo se guarda y recupera la información - Entity Framework Core

**¿Por qué monolito?**: Es más simple de desarrollar y mantener para un proyecto de este tamaño. No requiere infraestructura compleja.

### Tercera decisión: El Modelo de Datos

Planificamos las entidades principales del sistema:

| Entidad | Para qué |
|---------|----------|
| **Usuario** | Personas que usan el sistema (vendedores, administradores) |
| **Categoria** | Tipos de productos (ej: "Herramientas", "Electricidad") |
| **Producto** | Los artículos que se venden (ej: "Martillo", "Cable") |
| **Venta** | Cada transacción de venta |
| **DetalleVenta** | Los productos vendidos en cada transacción |

### Cuarta decisión: Alcance y Sprints

Definimos qué iba en cada entrega:

**Entrega 1 (Sprint 1)**:
- Autenticación (login y registro)
- CRUD de productos
- CRUD de categorías

**Entrega 2 (Sprint 2)**:
- Registro de ventas
- Anulación de ventas

**Entrega 3 (Sprint 3)**:
- Devoluciones
- Historial de ventas
- Pruebas

### Resumen de decisiones tomadas al inicio

| Decisión | Valor elegido |
|----------|---------------|
| Lenguaje backend | C# con .NET 8 |
| Framework frontend | HTML + Tailwind CSS |
| Base de datos | SQLite |
| Patrón arquitectónico | Monolito de 3 capas |
| Sistema de autenticación | JWT con EF Core Identity |
| Metodología de trabajo | SDD con agentes de IA |
| Cantidad de desarrolladores | 2 personas |

---

## 3. Metodología de Desarrollo: SDD y Sistema Multi-Agente

### ¿Qué es SDD?

**SDD = Spec-Driven Development** (Desarrollo Guiado por Especificaciones). Es una metodología de trabajo que organiza el desarrollo en etapas claras. La usamos para mantener el proyecto ordenado y no perdernos en el camino.

Las etapas que seguimos fueron:

1. **Exploración**: Analizar qué existe y qué necesita el sistema
   - *Qué hicimos*: Revisamos la estructura del proyecto, entendemos qué había hecho cada miembro del equipo

2. **Propuesta**: Definir qué se va a hacer y cómo
   - *Qué hicimos*: Propusimos las funcionalidades del sprint, qué partes del sistema había que construir

3. **Especificación**: Escribir en detalle qué debe hacer cada parte
   - *Qué hicimos*: Definimos qué endpoints necesita el backend, qué campos tiene cada formulario

4. **Diseño**: Planear cómo se va a implementar técnicamente
   - *Qué hicimos*: Elegimos cómo estructurar el código, qué archivos crear

5. **Tareas**: Dividir el trabajo en pasos pequeños
   - *Qué hicimos*: Separamos qué hace cada quien (backend vs frontend)

6. **Implementación**: Escribir el código
   - *Qué hicimos*: Escribimos los controladores, servicios, y páginas HTML

7. **Verificación**: Probar que todo funcione
   - *Qué hicimos*: Probamos el login, el registro, el dashboard

8. **Archivo**: Guardar la documentación final
   - *Qué hicimos*: Generamos este PRD

**¿Por qué usamos SDD?**
- Evita que hagamos las cosas sin plan
- Permite ver el progreso en cada etapa
- Facilita encontrar errores antes de que sean grandes problemas
- Genera documentación automáticamente

### ¿Qué es un Sistema Multi-Agente?

Un **agente** es un asistente de IA especializado en una tarea específica. No es una persona, sino un programa de inteligencia artificial que tiene un rol definido.

Un **sistema multi-agente** es un equipo de varios de estos asistentes, donde cada uno tiene una especialidad.

En nuestro proyecto usamos estos agentes:

| Agente | Rol | Qué hace |
|--------|-----|----------|
| **sdd-init** | Inicializador | Prepara el proyecto para trabajar con SDD, detecta el stack tecnológico |
| **sdd-explore** | Investigador | Analiza el código existente, busca información en la memoria |
| **sdd-propose** | Proponente | Sugiere qué hacer y cómo abordar cada feature |
| **sdd-design** | Diseñador | Define la arquitectura técnica, cómo estructurar el código |
| **sdd-apply** | Implementador | Escribe el código, crea los archivos |
| **sdd-verify** | Verificador | Prueba que todo funcione, busca errores |
| **sdd-archive** | Archivador | Guarda la documentación final |

**Analogía**: Es como tener un equipo de trabajo donde cada persona tiene un rol específico (arquitecto, programador, tester), pero todos los roles los cumplen asistentes de IA. El equipo humano supervisa y toma las decisiones finales.

### ¿Cuál es nuestro rol en el equipo?

Somos un equipo de 2 personas. Así分担amos las tareas:

| Rol | Qué hacemos |
|-----|-------------|
| **Tomar decisiones finales** | Nosotros (el equipo humano) |
| **Ejecutar código** | Los agentes de IA |
| **Revisar y aprobar** | Nosotros (nosotros) |
| **Explicar conceptos** | El agente (yo) |
| **Hacer el trabajo manual** | Nosotros (lo que la IA no puede hacer) |

**En resumen**: Nosotros indicamos qué hacer, la IA lo ejecuta y nos explica cómo funciona. Nosotros validamos que esté bien. Si algo no nos gusta, se lo decimos a la IA y lo corrige.

---

## 4. Justificación del Uso de Inteligencia Artificial

### ¿Por qué utilizar IA en el desarrollo?

Como equipo de 2 personas con poco tiempo, usamos IA como herramienta de asistencia durante todo el desarrollo. Esto nos permitió:

1. **Acelerar el desarrollo**: La IA escribe código funcional en minutos/horas en lugar de días. Nos permite avanzar más rápido sin sacrificar calidad.

2. **Aplicar buenas prácticas**: La IA conoce patrones de diseño que un desarrollador novato podría no conocer. Nos ayuda a no cometer errores comunes.

3. **Aprender el "por qué"**: La IA nos explica las decisiones técnicas. No solo nos da el código, sino que nos dice por qué lo hizo así. Esto es invaluable para el aprendizaje.

4. **Documentación automática**: La IA genera registros automáticos del progreso. Nos ayuda a documentar lo que hacemos sin tener que parar de programar.

5. **Depuración más rápida**: Cuando algo no funciona, la IA diagnostica el problema mucho más rápido que nosotros buscando en Google.

### ¿Qué tareas automatizó la IA?

| Tarea | Qué hizo la IA |
|-------|----------------|
| Estructura del proyecto | Creó las carpetas, configuró los archivos iniciales |
| Código de autenticación | Escribió el login, registro, y generación de tokens JWT |
| Diseño de interfaces | Creó las páginas HTML con Tailwind, animaciones, validaciones |
| Configuración de base de datos | Configuró Entity Framework, creó las tablas |
| Depuración de errores | Encontró y soluciones los problemas de CORS y JWT |

### ¿Qué decidió el equipo humano?

| Decisión | Quién la tomó |
|----------|---------------|
| Tecnologías a usar | Nosotros (al inicio del proyecto) |
| Diseño de las pantallas | Nosotros (aprobamos o rechazamos las propuestas) |
| Funcionalidades del sistema | Nosotros (definimos el alcance) |
| Código final | Nosotros (revisamos y validamos) |

### Beneficios observables

- **Tiempo**: Tareas que habrían tomado horas las completamos en minutos
- **Calidad**: El código sigue patrones correctos sin que tengamos que buscarlos
- **Aprendizaje**: Entendimos cómo funcionan JWT, CORS, Entity Framework mientras trabajábamos
- **Documentación**: Tenemos este PRD generado casi sin esfuerzo extra

---

## 5. Ramas de Trabajo

El proyecto se desarrolló en dos ramas paralelas en Git:

| Rama | Responsable | Contenido |
|------|-------------|------------|
| **Axel** | Backend | Lógica del servidor, base de datos, API |
| **Nico** | Frontend | Pantallas, diseño, interacción con usuario |

Esto permite que el backend y frontend se desarrollen independientemente y luego se integren en main.

---

## 6. Funcionalidades Desarrolladas - Sprint 1

### 6.1 Autenticación (HU1 - Registro de usuarios, HU2 - Login)

**Lo que el usuario puede hacer:**
- Crear una cuenta nueva con nombre, email y contraseña
- Iniciar sesión con email y contraseña
- Ver si la contraseña está bien escrita antes de enviar
- Elegir si mostrar u ocultar la contraseña mientras escribe
- Ver mensajes de error o éxito durante el proceso
- Ver un indicador de "cargando" mientras el servidor responde
- Cerrar sesión desde cualquier pantalla

**Lo que hicimos por dentro:**
- El servidor verifica que el email no esté ya usado
- La contraseña se guarda de forma segura (encriptada con bcrypt)
- Se genera un "token" (una clave temporal) para mantener la sesión activa

**Archivos creados/modificados:**
- `Controllers/AuthController.cs` - Endpoints de autenticación
- `Services/Implementations/AuthService.cs` - Lógica de auth
- `Models/DTOs/RegisterRequest.cs`, `LoginRequest.cs`, `AuthResult.cs` - Objetos de transferencia
- `wwwroot/login.html` - Página de login
- `wwwroot/register.html` - Página de registro

### 6.2 Dashboard - Pantalla Principal (HU4)

**Lo que el usuario puede ver:**
- Su nombre de bienvenida
- Un menú con tarjetas para cada sección del sistema
- Estadísticas rápidas (cuántos productos hay, cuáles tienen poco stock)
- Botón para cerrar sesión

**Las secciones disponibles:**
- Productos (gestionar inventario)
- Categorías (organizar productos)
- Nueva Venta (registrar ventas)
- Stock (ver niveles de inventario)
- Usuarios (administrar cuentas)
- Ajustes (configuración)

**Características del diseño:**
- Se adapta a pantallas grandes (computadora) y pequeñas (celular)
- Animaciones suaves al pasar el mouse por las tarjetas
- Colores de marca: naranja (#F25C05) y azul (#052741)

**Archivos creados/modificados:**
- `wwwroot/index.html` - Dashboard con menú y estadísticas

### 6.3 Backend - API y Base de Datos

**Componentes que creamos:**

| Componente | Descripción |
|------------|-------------|
| **Program.cs** | Configuración principal del servidor, CORS, JWT, SQLite |
| **AuthController** | Endpoints para login, registro, logout |
| **ProductosController** | Endpoints para CRUD de productos con paginación |
| **CategoriasController** | Endpoints para CRUD de categorías |
| **AuthService** | Lógica de autenticación y generación de tokens |
| **ProductoService** | Lógica de gestión de productos |
| **CategoriaService** | Lógica de gestión de categorías |
| **ApplicationDbContext** | Configuración de la base de datos |

**Archivos de modelos que creamos:**
- Entidades: `ApplicationUser`, `Producto`, `Categoria`, `Venta`, `DetalleVenta`
- DTOs: `RegisterRequest`, `LoginRequest`, `AuthResult`, `UserDTO`, `ProductoDTO`, `CategoriaDTO`

**Base de datos:**
- Archivo: `proyectoSISIE.db` (se crea automáticamente)
- Tablas: Usuarios, Productos, Categorías
- Relaciones entre productos y categorías

---

## 7. Errores Técnicos Solucionados

Durante el desarrollo, surgieron los siguientes problemas que fueron diagnosticados y corregidos:

| # | Problema Detectado | Síntoma | Causa | Solución Aplicada |
|---|--------------------|---------|-------|-------------------|
| 1 | Error "Error de conexión" al intentar registrarse | El navegador mostraba "Error de conexión" al hacer clic en registrar | CORS no estaba configurado en el servidor | Agregamos configuración CORS en Program.cs |
| 2 | Error 500 "IDX10720" al registrar usuario | El servidor respondía con error 500 y el mensaje "IDX10720: Unable to create KeyedHashAlgorithm" | La clave JWT era muy corta (216 bits, necesitaba al menos 256) | Aumentamos la clave en appsettings.json a más de 256 bits |
| 3 | Logo muy pequeño y poco visible en login | El logo de SISIE aparecía muy pequeño en la esquina | El tamaño inicial era muy reducido (3rem) | Ajustamos el tamaño a 96px (h-24) y lo posicionamos arriba de la caja del formulario |

---

## 8. Estado Final del Sprint 1

### Lo que está funcionando:

✅ Registro de nuevos usuarios  
✅ Inicio de sesión (login)  
✅ Dashboard con menú de navegación  
✅ API REST de autenticación  
✅ API REST de productos (CRUD completo)  
✅ API REST de categorías  
✅ Base de datos SQLite configurada  
✅ Sistema de seguridad con tokens JWT  
✅ Diseño responsive (funciona en celular y computadora)  

### Lo que falta para sprints siguientes:

🔲 Interfaz de gestión de productos (pantalla para agregar, editar, eliminar productos)  
🔲 Interfaz de gestión de categorías (pantalla para agregar, editar, eliminar categorías)  
🔲 Módulo de registro de ventas  
🔲 Sistema de devoluciones  
🔲 Historial de ventas  

---

## 9. Conclusión

El **Sprint 1** estableció las bases completas del sistema SISIE. Como equipo de 2 personas, usamos IA para acelerar el desarrollo manteniendo calidad técnica.

**Lo que logramos:**
- El backend está funcionando con todas las APIs necesarias
- El frontend tiene las pantallas de autenticación y un dashboard inicial
- La base de datos está operativa
- El sistema de seguridad está configurado

La utilización de IA permitió completar este sprint en menos tiempo del que habría requerido un desarrollo tradicional, manteniendo calidad y generando documentación automática.

Este documento sirve como registro del progreso del proyecto y como justificación del uso de herramientas de IA en el desarrollo de software.