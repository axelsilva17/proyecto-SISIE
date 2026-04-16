# Skill Registry — proyecto-SISIE

**Para agentes que delegan trabajo.** Los sub-agentes reciben las compact rules ya resueltas en su prompt de lanzamiento.

## User Skills Disponibles

| Trigger | Skill | Path |
|---------|-------|------|
| Pull request, abrir PR | branch-pr | ~/.config/opencode/skills/branch-pr/SKILL.md |
| Go tests, Bubbletea | go-testing | ~/.config/opencode/skills/go-testing/SKILL.md |
| Crear issue, bug, feature | issue-creation | ~/.config/opencode/skills/issue-creation/SKILL.md |
| "judgment day", dual review | judgment-day | ~/.config/opencode/skills/judgment-day/SKILL.md |
| SDD: apply, implementar tareas | sdd-apply | ~/.config/opencode/skills/sdd-apply/SKILL.md |
| SDD: archive, cerrar cambio | sdd-archive | ~/.config/opencode/skills/sdd-archive/SKILL.md |
| SDD: design, diseño técnico | sdd-design | ~/.config/opencode/skills/sdd-design/SKILL.md |
| SDD: explore, investigar | sdd-explore | ~/.config/opencode/skills/sdd-explore/SKILL.md |
| SDD: init, inicializar | sdd-init | ~/.config/opencode/skills/sdd-init/SKILL.md |
| SDD: onboard, walkthrough | sdd-onboard | ~/.config/opencode/skills/sdd-onboard/SKILL.md |
| SDD: propose, propuesta | sdd-propose | ~/.config/opencode/skills/sdd-propose/SKILL.md |
| SDD: spec, especificaciones | sdd-spec | ~/.config/opencode/skills/sdd-spec/SKILL.md |
| SDD: tasks, breakdown | sdd-tasks | ~/.config/opencode/skills/sdd-tasks/SKILL.md |
| SDD: verify, validar | sdd-verify | ~/.config/opencode/skills/sdd-verify/SKILL.md |
| Crear nuevo skill | skill-creator | ~/.config/opencode/skills/skill-creator/SKILL.md |
| "actualizar skills", "skill registry" | skill-registry | ~/.config/opencode/skills/skill-registry/SKILL.md |
| Frontend interfaces, web components | **frontend-design** | ~/.agents/skills/frontend-design/SKILL.md |
| Canvas, gráficos interactivos | **canvas-design** | ~/.agents/skills/canvas-design/SKILL.md |
| Theming, temas CSS | **theme-factory** | ~/.agents/skills/theme-factory/SKILL.md |
| Brand, identidad visual | **brand-guidelines** | ~/.agents/skills/brand-guidelines/SKILL.md |
| Buscar skills, encontrar | **find-skills** | ~/.agents/skills/find-skills/SKILL.md |
| Word documentos, .docx | **docx** | ~/.agents/skills/docx/SKILL.md |
| PDF, archivos PDF | **pdf** | ~/.agents/skills/pdf/SKILL.md |
| PowerPoint, .pptx | **pptx** | ~/.agents/skills/pptx/SKILL.md |
| Excel, .xlsx | **xlsx** | ~/.agents/skills/xlsx/SKILL.md |
| Testing webapp | **webapp-testing** | ~/.agents/skills/webapp-testing/SKILL.md |
| "git sync", "git push", sync | git-sync | Regla integrada |

## Compact Rules

### sdd-apply
- Lee specs + design + tasks ANTES de escribir código
- Usa modo artifact_store: engram | openspec | hybrid
- Marca tareas completas en mem_update
- No reinventes: seguí patrones del codebase existente

### sdd-init
- Detecta stack automáticamente (paquetes NuGet, config)
- Detecta testing capabilities (test runner, frameworks)
- strict_tdd: true si hay test runner, false si no
- Guarda contexto en sdd-init/{project}

### sdd-spec
- Formato Given/When/Then para escenarios
- Keywords RFC 2119: MUST, SHALL, SHOULD, MAY
- Documenta happy path + edge cases + error states

### sdd-design
- Incluye diagramas para flujos complejos
- Documenta decisiones con rationale (el "porqué")
- Arquitectura con capas claramente definidas

### sdd-tasks
- Numeración jerárquica: 1.0, 1.1, 1.2, etc.
- Agrupar por fase: infraestructura, implementación, testing
- Tareas pequeñas (completables en una sesión)

### sdd-verify
- Comparar implementación contra CADA escenario de specs
- Probar happy path + edge cases
- Reportar CRITICAL / WARNING / SUGGESTION

### branch-pr
- Issue-first: siempre crear issue antes de PR
- Rama feature desde main
- Commits convencionales: feat, fix, chore, docs
- PR con description clara

### judgment-day
- Lanzar 2 sub-agentes independientes a revisar mismo target
- Sintetizar hallazgos de ambos
- Aplicar fixes y re-juzgar hasta que pasen
- Escalar después de 2 iteraciones si no pasan

### sdd-explore
- Investigar codebase antes de comprometer
- Comparar approaches técnicos
- Documentar tradeoffs
- No crea archivos, solo investigación

### sdd-propose
- Crear proposal formal con intent, scope, approach
- Incluir rollback plan para cambios riesgosos
- Identificar módulos afectados
- Guardar en sdd/{change}/proposal

### go-testing
- Tests en Go con bubbletea TUI pattern
- Usar teatest para testing helpers
- Unit tests + integration tests
- Coverage con go test -cover

### frontend-design (NUEVA)
- Elige dirección estética clara ANTES de codear (minimalismo, maximalismo, retro-futurista, etc.)
- Tipografía distintiva y única — evitar Arial/Inter genéricos
- Atención extrema al detalle visual
- Código functional y production-grade

### canvas-design (NUEVA)
- Gráficos interactivos con Canvas API
- Animaciones fluidas y performantes
- Eventos de mouse/touch optimizados
- requestAnimationFrame para loops de animación

### theme-factory (NUEVA)
- CSS custom properties para theming
- Variables de color, spacing, tipografía
- dark mode support
- Diseño atómico de tokens

### brand-guidelines (NUEVA)
- Paleta de colores de marca
- Tipografía de marca
- Espaciados y proporciones consistentes
- Iconografía y estilo visual unificado

### find-skills (NUEVA)
- Buscar en ~/.config/opencode/skills/ y ~/.agents/skills/
- glob */SKILL.md para encontrar skills disponibles
- Mostrar triggers y paths

### docx
- Archivos .docx son ZIP con XML adentro
- Usar pandoc para leer, docx-js para crear
- Editing: unpack → edit XML → repack
- Soporta tablas, headers, page numbers, imágenes

### pdf
- Generación de PDFs con librerías especializadas
- Soporta imágenes, tablas, fuentes personalizadas
- Conversión desde HTML/Markdown

### pptx
- PowerPoint como ZIP de XMLs
- Slides, transiciones, animaciones
- Tablas, gráficos, imágenes embebidas

### xlsx
- Excel como ZIP de XMLs
- Fórmulas, gráficos, formatos
- Múltiples sheets

### webapp-testing
- Testing de aplicaciones web
- Selenium, Playwright, puppeteer
- E2E tests, integration tests

### git-sync
- Después de hacer git push, INDICAR al usuario qué debe hacer su compañero
- Ejemplos: "tu compañero debe hacer git pull origin main", "hacé git pull para descargar los cambios", etc.
- Esto aplica para: git push, git commit, merge de PR, o cualquier cambio que requiera acción del compañero

## Project Conventions

| Archivo | Path | Notas |
|---------|------|-------|
| AGENTS.md | .atl/AGENTS.md | Agentes Axel (Backend) y Nico (Frontend) |

---

## Uso

El orquestador lee este archivo para resolver skills antes de lanzar sub-agentes.
Para actualizar tras agregar skills nuevos, ejecutar skill-registry skill.

**Última actualización:** 2026-04-16