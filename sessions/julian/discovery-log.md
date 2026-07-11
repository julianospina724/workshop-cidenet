# Bitácora de discovery — Julián

## 🎯 Fase 1 — Épicas (completada)

- **EPIC-001 · Administración de usuarios y accesos** — épica única que engloba cuentas, roles y matriz de permisos (Julián optó por no partirla).
- VUIFED: Valor 10 · Usuarios 10 · Impacto 7 · Factibilidad 7 · Esfuerzo 7 · Dependencias 10 → **total 51 / promedio 8.5**.
- `mvp_included: true` — es el corazón del módulo.
- Artefacto: `specs/epicas/EPIC-001.md`.

## 📝 Fase 2 — Historias (completada)

- **Decisión de diseño clave:** 1 rol por usuario (single-role, obligatorio). R5 = exactamente un rol.
- **Alcance MVP:** entran gestión de cuentas, roles, matriz de permisos, perfil propio y autenticación; `reports` queda fuera como funcionalidad (solo permiso).
- 7 historias INVEST acordadas:
  - US-001 Crear cuenta · US-002 Consultar (tabla) · US-003 Editar · US-004 Eliminar
  - US-005 Editar propio perfil · US-006 Configurar matriz de permisos · US-007 Autenticación (mínima: login + bloqueo de inactivos)
- Rol se asigna dentro de crear/editar (no historia aparte).
- Artefactos: `specs/historias/US-001.md` … `US-007.md`.

## ⚡ Fase 3 — Criterios (en progreso)

- **US-001 (Crear cuenta) — completada.** Cuenta nace activa de inmediato; formulario con confirmar-contraseña. Password: min 8, mayúscula+minúscula+número+símbolo (`$&*#@`). Nombre/apellido y email obligatorios. Email duplicado → mensaje general al guardar; campos faltantes → marca inline; fallo general → mensaje genérico. Email se normaliza a minúsculas + índice único en BD. UX: loading + mensaje general de resultado. SMART validado.
- Artefacto: `specs/criterios/US-001.yaml`.
- **US-002 (Consultar/tabla de usuarios) — completada.** Filtros por rol/nombres/apellidos/email/fechas (rango), combinados con AND; orden por columnas, default nombres+apellidos ascendente; paginación con tamaño elegible por el usuario. Búsqueda parcial insensible a mayúsculas. Error técnico → mensaje general; sin resultados → mensaje específico. Datepicker restringe hasta<desde en UI + validación backend. UX: spinner de carga. SMART validado.
- Artefacto: `specs/criterios/US-002.yaml`.
- **US-003 a US-007 — generados automáticamente**, a petición explícita de Julián (confirmada vía pregunta directa: prefirió velocidad sobre descubrir cada regla él mismo). Cubren: edición (US-003, con la restricción de no poder auto-cambiarse el rol y protección del último Admin activo), eliminación con R1 (US-004), edición de perfil propio con R2/R3 (US-005), matriz de permisos con R3 (US-006), y autenticación con R6 y mensajes anti-enumeración (US-007). **Nota:** a diferencia de US-001/US-002, estos criterios no fueron descubiertos por Julián pregunta a pregunta — quedan sujetos a su revisión antes de darlos por definitivos.
- Artefactos: `specs/criterios/US-003.yaml` … `US-007.yaml`.
- ✅ Fase Criterios completa (`ready_for_handoff: true`).
- Siguiente: 🔍 Fase 4 — Completitud (autocrítica por las 10 áreas del ciclo de vida).

## 🔍 Fase 4 — Completitud (en progreso)

- **⚙️ CFG** — sin gaps. Decisión explícita: nada es configurable dinámicamente (política de contraseña, tamaño de página, etc.), todo fijo en código.
- **👥 USR** — 1 gap 🔥 crítico: no se puede eliminar/inactivar un Admin si es el único Admin **activo** (un Admin inactivo no protege la regla). Refina R1. Aplicado en `US-003.yaml` y `US-004.yaml`.
- **🔒 SEC** — 3 gaps: 🔥 la contraseña nunca se retorna en ninguna respuesta (US-001/002/003); 🔥 autorización server-side obligatoria en toda escritura — Admin todo, Editor/Viewer solo su propia cuenta (nunca su rol), solo Admin toca la matriz de permisos (US-003/004/005/006); ⚡ el estado propio no es editable desde el perfil (US-005) — nadie se autodesactiva.
- **🗃️ AUD** — 3 gaps: ⚡ normalización de cadenas (trim + colapso de espacios) en nombre/apellido (US-001/003/005); 🔥 eliminación lógica (soft delete) — registro permanece en BD marcado 'eliminado' (distinto de 'inactivo'), bloquea auth y cambios, se excluye de la tabla (US-002/004/007); ⚡ **nueva historia US-008-AUD** — historial de auditoría a nivel de BD (quién/cuándo/qué) por cada escritura sobre un usuario, sin UI en el MVP.
- **📈 MON** — 1 gap 🔥 crítico: bloqueo temporal de cuenta tras 3 intentos fallidos de login, liberado automáticamente a los 15 minutos. Reemplaza la suposición anterior de "sin bloqueo por intentos" en US-007.
- **🔗 INT** — sin gaps. Decisión explícita: sin integraciones con sistemas externos en este alcance.
- **🔧 MNT** — 1 gap 💡 mejora: retención de auditoría de 6 meses para los registros de US-008-AUD, luego se purgan.
- **📊 RPT** — sin gaps. Se excluyen reportes por completo del MVP (consistente con la decisión de Historias).
- **💾 BCK** — sin gaps de código. Cuentas/roles/privilegios son críticos y deberían respaldarse en producción, pero backup automático queda fuera del MVP (decisión explícita, responsabilidad de infraestructura).
- **🧪 TST** — sin gaps nuevos. Julián confirma que todas las reglas 🔥 críticas ya tienen su escenario negativo definido.

## ✅ Fase 4 — Completitud cerrada (10/10 áreas)

Resumen de gaps descubiertos (10 en total + 1 historia nueva):
- 🔥 Crítico (5): último Admin activo protege eliminación/inactivación (US-003/004) · contraseña nunca se retorna (US-001/002/003) · autorización server-side en toda escritura (US-003/004/005/006) · eliminación lógica con estado 'eliminado' distinto de 'inactivo' (US-002/004/007) · bloqueo de cuenta tras 3 intentos fallidos, 15 min (US-007).
- ⚡ Importante (3): estado propio no editable en perfil (US-005) · normalización de cadenas nombre/apellido (US-001/003/005) · historial de auditoría a nivel de BD, sin UI (nueva historia US-008-AUD).
- 💡 Mejora (2): retención de auditoría a 6 meses (US-008-AUD) · [sin configuración dinámica es una no-decisión, no un gap].
- Áreas sin gaps (decisiones explícitas, no huecos): CFG (nada configurable), INT (sin integraciones externas), RPT (sin reportes en MVP), BCK (backup fuera de alcance).
- `completitud_phase.ready_for_handoff: true`.
- Siguiente: 🔧 Fase 5 — Gherkin (genera `features/*.feature` desde `specs/criterios/*.yaml`, cierra con reporte DQS-lite).

## 🔧 Fase 5 — Gherkin + DQS-lite (completada)

- Generados 8 archivos `features/*.feature` (US-001 a US-008-AUD), con Background, escenarios de éxito, escenarios negativos por cada regla crítica, y `Scenario Outline` donde había datos variables. Tags `@story_id`, `@origin` (discovery_inicial / analisis_completitud), `@priority`, `@complexity`.
- Borrado `features/ejemplo_formato.feature` (ya no hace falta, quedan los `.feature` reales del caso).
- Reporte de cobertura: `sessions/julian/dqs-lite.md`.
- Spec consolidada: `specs/SPEC.md` (historias, reglas de negocio explícitas + descubiertas, endpoints, pantallas, referencia a criterios y DQS-lite).

## ✅ Discovery completo

- `gherkin_phase.ready_for_handoff: true`, `project_state.current_phase: "completed"`.
- Siguiente paso: `/plan`.
