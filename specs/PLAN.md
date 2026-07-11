# PLAN — Módulo de Usuarios, Roles y Permisos (CIDENET)

> Generado por `/plan` a partir de `specs/SPEC.md` y `features/*.feature`. Backend completo primero (modelo → reglas de negocio → endpoints), frontend después. Cada iteración termina en **un commit** con sus tests en verde (TDD puro + GitFlow puro).

**Nota de tiempo:** el Día 2 del taller son ~4 horas. Este plan tiene 16 iteraciones — más de las que probablemente completes en la sesión. Eso está bien: el objetivo es practicar el loop `/test` → `/iterate` con calidad, no llegar a la iteración 16. Prioriza de arriba hacia abajo; si el tiempo se acaba, para en cualquier iteración con sus tests en verde y commiteada — eso ya es un cierre válido. Marco con 🔥/⚡/💡 qué tan crítica es cada una si necesitas recortar.

---

## Iteración 1 — Infraestructura 🔥 ✅

**Entregable:** levantar el stack completo (Postgres + API + frontend) con Docker.
**Done-when:** `docker compose up --build` levanta `db`, `api` y `frontend`; `GET http://localhost:5000/health` responde `{"status":"ok"}`; `http://localhost:5173` carga el frontend mínimo.
**Verificado:** `docker compose ps` — `db` healthy, `api` y `frontend` up; `/health` → `{"status":"ok"}` (200); `http://localhost:5173` → 200.

## Iteración 2 — Modelo de dominio y migración inicial 🔥 ✅

**Entregable:** entidades de dominio y su mapeo EF, sin endpoints todavía:
- `User` (nombre, apellido, email, passwordHash, rol, estado: `activo`/`inactivo`/`eliminado`, contador de intentos fallidos, timestamp de bloqueo).
- `PermissionMatrix` (rol × recurso × acción CRUD → booleano), con seed del estado por defecto del caso.
- `AuditLog` (usuarioId, entidad, accion, fecha, detalle del cambio) — soporta US-008-AUD.
- Índice único sobre `User.Email` (normalizado a minúsculas).
- Migración de EF Core aplicada contra Postgres.

**Done-when:** tests de integración mínimos verifican que el `AppDbContext` persiste y recupera un `User`, un `PermissionMatrix` y un `AuditLog`; la migración corre limpia sobre una base vacía.
**Verificado:** 5/5 tests en verde (`tests/Api.Tests/DomainPersistenceTests.cs`, SQLite en memoria — incluye el rechazo de email duplicado). Migración `20260711154649_InitialUsersPermissionsAudit` generada y aplicada contra el Postgres de `docker compose` (tablas `Users`, `PermissionMatrixEntries`, `AuditLogs`, índice único `IX_Users_Email` confirmados con `psql`). Seed de la matriz de permisos por defecto se deja para la Iteración 9 (US-006), cuando se implemente su lectura/escritura.

## Iteración 3 — US-001: Crear cuenta (backend) 🔥 ✅

**Entregable:** `POST /api/users` con todas las reglas de `features/US-001.feature`: validación de campos, complejidad de contraseña, hash (nunca se retorna), unicidad de email (normalizado), normalización de nombre/apellido, estado inicial activo.
**Done-when:** los escenarios de `US-001.feature` (incluidos los negativos: contraseñas no coincidentes, campos inválidos, email duplicado, condición de carrera de email) pasan como tests de backend.
**Verificado:** 15/15 tests en verde (9 nuevos en `CreateUserEndpointTests.cs`, vía `WebApplicationFactory` + SQLite en memoria). Probado también end-to-end contra el Postgres real de `docker compose`: `POST /api/users` → 201, email normalizado a minúsculas, contraseña guardada como hash PBKDF2 (nunca en la respuesta), email duplicado → 400 con mensaje general. **Corrección de la Iteración 2:** `User.Apellido` se elimina del modelo (migración `RemoveApellidoFromUser`) — el caso y el Gherkin siempre usaron un solo campo `nombre`, no nombre+apellido separados.

## Iteración 4 — US-007: Autenticación y bloqueo (backend) 🔥

**Entregable:** `POST /api/auth/login` — valida credenciales contra el hash, bloquea usuarios `inactivo`/`eliminado`, mensaje único anti-enumeración, bloqueo temporal de 15 min tras 3 intentos fallidos con reinicio del contador.
**Done-when:** los escenarios de `US-007.feature` pasan, incluidos los de bloqueo y liberación automática.

## Iteración 5 — US-002: Consultar usuarios (backend) ⚡

**Entregable:** `GET /api/users` con filtros combinables (rol, nombre, apellido, email, rango de fechas), búsqueda parcial insensible a mayúsculas, orden por columnas (default nombre+apellido ascendente), paginación con tamaño elegible, exclusión de usuarios `eliminado`, sin exponer la contraseña.
**Done-when:** los escenarios de `US-002.feature` pasan (incluida la validación de rango de fechas invertido y el caso sin resultados).

## Iteración 6 — US-003: Editar cuenta (backend) 🔥

**Entregable:** `PUT/PATCH /api/users/{id}` — autorización server-side (Admin cualquiera, Editor/Viewer solo su propia cuenta y nunca su rol), bloqueo de la regla "último Admin activo" al cambiar rol/estado, unicidad de email al editar, normalización de texto.
**Done-when:** los escenarios de `US-003.feature` pasan, incluidos los de autorización y los del último Admin activo.

## Iteración 7 — US-004: Eliminar cuenta (backend) 🔥

**Entregable:** `DELETE /api/users/{id}` como eliminación lógica (estado `eliminado`, distinto de `inactivo`), bloqueo si es el único Admin activo, autorización solo Admin, idempotencia ante doble solicitud.
**Done-when:** los escenarios de `US-004.feature` pasan, incluido el de eliminación ya realizada por otra sesión.

## Iteración 8 — US-005: Editar perfil propio (backend) ⚡

**Entregable:** endpoint (o variante de US-003) para que cualquier usuario edite solo su propio registro (nombre, email) — rol, estado y permisos bloqueados incluso vía manipulación directa.
**Done-when:** los escenarios de `US-005.feature` pasan, incluidos los de intento de escalar privilegios y el conflicto con desactivación en paralelo.

## Iteración 9 — US-006: Matriz de permisos (backend) ⚡

**Entregable:** `GET/PUT /api/permissions` — solo Admin, bloqueo de autoasignación (fila del propio rol), sin crear/eliminar roles o recursos.
**Done-when:** los escenarios de `US-006.feature` pasan.

## Iteración 10 — US-008-AUD: Auditoría transversal (backend) 💡

**Entregable:** instrumentar las escrituras de las iteraciones 3, 6, 7 y 9 para que cada una genere su `AuditLog` de forma atómica (falla la operación si falla el registro); registros append-only; rutina de purga a 6 meses (puede quedar como job/función, sin necesidad de scheduler real en el MVP).
**Done-when:** los escenarios de `US-008-AUD.feature` pasan.

---

## Iteración 11 — Frontend: Login 🔥

**Entregable:** pantalla de login (US-007) — formulario, loading, mensaje de error único, aviso de bloqueo temporal, redirección tras éxito.
**Done-when:** tests de componente de `US-007.feature` (los aplicables a UI) pasan.

## Iteración 12 — Frontend: Tabla de usuarios ⚡

**Entregable:** pantalla de US-002 — filtros, búsqueda, orden, paginación, spinner, mensaje sin resultados, acciones por fila (enlazan a editar/eliminar).
**Done-when:** tests de componente de `US-002.feature` pasan.

## Iteración 13 — Frontend: Formulario crear/editar usuario 🔥

**Entregable:** pantalla de US-001 + US-003 (comparten formulario) — validaciones visibles, confirmar-contraseña al crear, selector de rol deshabilitado en autoedición, mensajes de error inline y generales.
**Done-when:** tests de componente de `US-001.feature` y `US-003.feature` (los aplicables a UI) pasan.

## Iteración 14 — Frontend: Modal de eliminación ⚡

**Entregable:** modal de confirmación de US-004 — info del usuario, advertencia y bloqueo si es el único Admin activo.
**Done-when:** tests de componente de `US-004.feature` pasan.

## Iteración 15 — Frontend: Perfil propio 💡

**Entregable:** pantalla de US-005 — solo nombre/email editables.
**Done-when:** tests de componente de `US-005.feature` pasan.

## Iteración 16 — Frontend: Matriz de permisos 💡

**Entregable:** pantalla de US-006 — toggles por rol/recurso/acción, guardado explícito, fila del propio rol deshabilitada.
**Done-when:** tests de componente de `US-006.feature` pasan.
