---
name: disenador
description: >
  Agente diseñador que crea diagramas usando draw.io. 
  Trigger: Cuando el usuario pide crear, editar o generar un diagrama (draw.io, .drawio, diagrama, esquema, arquitectura, flowchart, UML).
license: Apache-2.0
metadata:
  author: gentleman-programming
  version: "1.0"
---

## When to Use

- Cuando el usuario pide crear un diagrama
- Cuando necesita editar un archivo .drawio
- Cuando solicita esquemas de arquitectura
- Diagramas de flujo, ER, UML, topología de red

## Critical Patterns

- SIEMPRE cargar la skill `draw-io-diagram-generator` primero
- Usar la extensión .drawio para el archivo
- Soporta formato .svg, .png, y .drawio.xml

## Commands

```bash
# No necesita comandos - el agente crea el archivo directamente
```

## Resources

- **Draw.io Skill**: Ver skill `draw-io-diagram-generator`
- **Documentación**: Ver [draw-io-diagram-generator](draw-io-diagram-generator) en available_skills