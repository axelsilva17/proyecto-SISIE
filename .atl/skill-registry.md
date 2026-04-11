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
- Numeração jerárquica: 1.0, 1.1, 1.2, etc.
-agrupar por fase: infraestructura, implementación, testing
- Tareas pequeñas (completables en una sesión)

### sdd-verify
- Comparar implementación contra CADA escenario de specs
- Probar happy path + edge cases
- Reportar CRITICAL / WARNING / SUGGESTION

## Project Conventions

Aún no hay conventions específicas del proyecto.

---

## Uso

El orquestador lee este archivo para resolver skills antes de lanzar sub-agentes.
Para actualizar tras agregar skills nuevos, ejecutar skill-registry skill.