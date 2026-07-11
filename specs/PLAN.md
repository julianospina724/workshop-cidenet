# PLAN — Módulo de Usuarios, Roles y Permisos (CIDENET)

> Generado por `/plan` a partir de `specs/SPEC.md` y `features/*.feature`. Backend completo primero (modelo → reglas de negocio → endpoints), frontend después. Cada iteración termina en **un commit** con sus tests en verde (TDD puro + GitFlow puro).

**Nota de tiempo:** el Día 2 del taller son ~4 horas. Este plan tiene 17 iteraciones — más de las que probablemente completes en la sesión. Eso está bien: el objetivo es practicar el loop `/test` → `/iterate` con calidad, no llegar a la última. Prioriza de arriba hacia abajo; si el tiempo se acaba, para en cualquier iteración con sus tests en verde y commiteada — eso ya es un cierre válido. Marco con 🔥/⚡/💡 qué tan crítica es cada una si necesitas recortar.

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
**Verificado:** 5/5 tests en verde (`tests/Api.Tests/DomainPersistenceTests.cs`, SQLite en memoria — incluye el rechazo de email duplicado). Migración `20260711154649_InitialUsersPermissionsAudit` generada y aplicada contra el Postgres de `docker compose` (tablas `Users`, `PermissionMatrixEntries`, `AuditLogs`, índice único `IX_Users_Email` confirmados con `psql`). Seed de la matriz de permisos por defecto se deja para la Iteración 10 (US-006), cuando se implemente su lectura/escritura.

## Iteración 3 — US-001: Crear cuenta (backend) 🔥 ✅

**Entregable:** `POST /api/users` con todas las reglas de `features/US-001.feature`: validación de campos, complejidad de contraseña, hash (nunca se retorna), unicidad de email (normalizado), normalización de nombre/apellido, estado inicial activo.
**Done-when:** los escenarios de `US-001.feature` (incluidos los negativos: contraseñas no coincidentes, campos inválidos, email duplicado, condición de carrera de email) pasan como tests de backend.
**Verificado:** 15/15 tests en verde (9 nuevos en `CreateUserEndpointTests.cs`, vía `WebApplicationFactory` + SQLite en memoria). Probado también end-to-end contra el Postgres real de `docker compose`: `POST /api/users` → 201, email normalizado a minúsculas, contraseña guardada como hash PBKDF2 (nunca en la respuesta), email duplicado → 400 con mensaje general. **Corrección de la Iteración 2:** `User.Apellido` se elimina del modelo (migración `RemoveApellidoFromUser`) — el caso y el Gherkin siempre usaron un solo campo `nombre`, no nombre+apellido separados.

## Iteración 4 — US-007: Autenticación y bloqueo (backend) 🔥 ✅

**Entregable:** `POST /api/auth/login` — valida credenciales contra el hash, bloquea usuarios `inactivo`/`eliminado`, mensaje único anti-enumeración, bloqueo temporal de 15 min tras 3 intentos fallidos con reinicio del contador.
**Done-when:** los escenarios de `US-007.feature` pasan, incluidos los de bloqueo y liberación automática.
**Verificado:** 26/26 tests en verde (11 nuevos en `AuthenticateEndpointTests.cs`; se introdujo `IClock`/`FakeClock` para probar la liberación de bloqueo tras 15 min sin esperar de verdad). Probado también end-to-end contra Docker: login válido, 3 fallos consecutivos → bloqueo 423 incluso con contraseña correcta, mensaje anti-enumeración idéntico para credenciales inválidas/usuario inactivo.

## Iteración 5 — Emisión y validación de JWT 🔥 ✅

**Entregable:** el login (US-007) emite un JWT firmado con `sub` (id de usuario) y `role` (rol) como claims. Middleware de autenticación JWT registrado en `Program.cs`; los endpoints protegidos (a partir de la Iteración 6) leen la identidad del llamador desde `HttpContext.User` en vez de confiar en lo que el cliente diga.
**Por qué esta iteración existe:** no estaba en el plan original — surgió al llegar a US-002, que ya requiere saber si quien llama es Admin/Editor/Viewer. Sin esto, ninguna de las reglas de autorización server-side (🔥 gap-SEC del discovery) es verificable.
**Done-when:** un test de integración hace login, recibe un token, lo usa en el header `Authorization: Bearer` de una petición a un endpoint protegido con `[Authorize]` de prueba, y confirma que `HttpContext.User` expone el id/rol correctos; una petición sin token (o con uno inválido/expirado) es rechazada con 401.
**Verificado:** 31/31 tests en verde (5 nuevos en `JwtAuthenticationTests.cs`: token no vacío, token válido expone id/rol vía `GET /api/auth/whoami`, sin token → 401, token manipulado → 401, token expirado → 401 — este último craftea un JWT firmado con la misma clave pero `exp` en el pasado). Probado también end-to-end contra Docker. `JWT_SIGNING_KEY` configurable por variable de entorno (con default de desarrollo); agregada a `docker-compose.yml` con nota de que en un entorno real debe venir de un secreto gestionado.

## Iteración 6 — US-002: Consultar usuarios (backend) ⚡ ✅

**Entregable:** `GET /api/users` con filtros combinables (rol, nombre, email, estado, rango de fechas), búsqueda parcial insensible a mayúsculas, orden por columnas (default nombre ascendente), paginación con tamaño elegible, exclusión de usuarios `eliminado`, sin exponer la contraseña. Requiere JWT válido con rol Admin o Editor (Viewer no tiene permiso sobre `users`).
**Done-when:** los escenarios de `US-002.feature` pasan (incluida la validación de rango de fechas invertido, el caso sin resultados, y el rechazo a Viewer).
**Verificado:** 44/44 tests en verde (13 nuevos en `GetUsersEndpointTests.cs`). Probado también end-to-end contra Docker: Admin ve la tabla paginada, Viewer recibe 403. **Corrección menor:** `US-002.yaml`/`.feature` mencionaban "nombres, apellidos" como si fueran dos campos y no listaban explícitamente el filtro de estado (que sí estaba en la historia original) — se alinearon con el modelo real (un solo campo `Nombre`) y se agregó el filtro de estado.

## Iteración 7 — US-003: Editar cuenta (backend) 🔥 ✅

**Entregable:** `PUT /api/users/{id}` — solo Admin (vía JWT; Editor/Viewer reciben 403), ningún usuario puede cambiar su propio rol (ni siquiera un Admin editándose a sí mismo), bloqueo de la regla "último Admin activo" al cambiar rol/estado, unicidad de email al editar, normalización de texto, respuesta sin exponer la contraseña.
**Done-when:** los escenarios de `US-003.feature` pasan, incluidos los de autorización y los del último Admin activo.
**Verificado:** 53/53 tests en verde (9 nuevos en `EditUserEndpointTests.cs`). Probado también end-to-end contra Docker. **Nota de diseño:** la edición del propio perfil por Editor/Viewer (US-005, Iteración 9) queda en un endpoint separado — este endpoint es exclusivamente de Admin, lo que simplifica la autorización a un `RequireRole("Admin")` en vez de lógica condicional por caller.

## Iteración 8 — US-004: Eliminar cuenta (backend) 🔥 ✅

**Entregable:** `DELETE /api/users/{id}` como eliminación lógica (estado `eliminado`, distinto de `inactivo`), bloqueo si es el único Admin activo, autorización solo Admin (vía JWT), idempotencia ante doble solicitud.
**Done-when:** los escenarios de `US-004.feature` pasan, incluido el de eliminación ya realizada por otra sesión.
**Verificado:** 60/60 tests en verde (7 nuevos en `DeleteUserEndpointTests.cs`, todos en verde en el primer intento). Probado también end-to-end contra Docker: 204 al eliminar, el usuario queda excluido de `GET /api/users`, y 404 al intentar eliminarlo de nuevo (idempotencia natural, sin código especial).

## Iteración 9 — US-005: Editar perfil propio (backend) ⚡ ✅

**Entregable:** endpoint (o variante de US-003) para que cualquier usuario edite solo su propio registro (nombre, email), identificado por el `sub` del JWT — rol, estado y permisos bloqueados incluso vía manipulación directa.
**Done-when:** los escenarios de `US-005.feature` pasan, incluidos los de intento de escalar privilegios y el conflicto con desactivación en paralelo.
**Verificado:** 66/66 tests en verde (6 nuevos en `EditProfileEndpointTests.cs`). Endpoint dedicado `PUT /api/users/me` (en vez de reutilizar `PUT /api/users/{id}`): el id sale del JWT, nunca de la URL, y el contrato `EditProfileRequest` **no tiene campos Rol/Estado** — no hay superficie de ataque que rechazar, System.Text.Json simplemente ignora esos campos si alguien los manda. Verificado también end-to-end contra Docker (`"rol":"Admin"` enviado directamente al backend no tiene ningún efecto).

## Iteración 10 — US-006: Matriz de permisos (backend) ⚡ ✅

**Entregable:** `GET/PUT /api/permissions` — solo Admin (vía JWT), bloqueo de autoasignación (fila del propio rol), sin crear/eliminar roles o recursos.
**Done-when:** los escenarios de `US-006.feature` pasan.
**Verificado:** 72/72 tests en verde (7 nuevos en `PermissionsEndpointTests.cs`). Matriz por defecto (48 entradas: 3 roles × 4 recursos × 4 acciones) sembrada vía EF Core `HasData` con la matriz exacta del caso, aplicada en migración `SeedDefaultPermissionMatrix` contra Postgres y verificada con `psql`. "No crear/eliminar roles o recursos" queda garantizado por diseño (son enums fijos — un valor inválido ni siquiera deserializa). Probado también end-to-end contra Docker.

## Iteración 11 — US-008-AUD: Auditoría transversal (backend) 💡

**Entregable:** instrumentar las escrituras de las iteraciones 3, 7, 8 y 10 para que cada una genere su `AuditLog` de forma atómica (falla la operación si falla el registro); registros append-only; rutina de purga a 6 meses (puede quedar como job/función, sin necesidad de scheduler real en el MVP).
**Done-when:** los escenarios de `US-008-AUD.feature` pasan.

---

## Iteración 12 — Frontend: Login 🔥

**Entregable:** pantalla de login (US-007) — formulario, loading, mensaje de error único, aviso de bloqueo temporal, redirección tras éxito, guarda el JWT recibido (ej. en memoria/contexto de React) para las siguientes peticiones.
**Done-when:** tests de componente de `US-007.feature` (los aplicables a UI) pasan.

## Iteración 13 — Frontend: Tabla de usuarios ⚡

**Entregable:** pantalla de US-002 — filtros, búsqueda, orden, paginación, spinner, mensaje sin resultados, acciones por fila (enlazan a editar/eliminar).
**Done-when:** tests de componente de `US-002.feature` pasan.

## Iteración 14 — Frontend: Formulario crear/editar usuario 🔥

**Entregable:** pantalla de US-001 + US-003 (comparten formulario) — validaciones visibles, confirmar-contraseña al crear, selector de rol deshabilitado en autoedición, mensajes de error inline y generales.
**Done-when:** tests de componente de `US-001.feature` y `US-003.feature` (los aplicables a UI) pasan.

## Iteración 15 — Frontend: Modal de eliminación ⚡

**Entregable:** modal de confirmación de US-004 — info del usuario, advertencia y bloqueo si es el único Admin activo.
**Done-when:** tests de componente de `US-004.feature` pasan.

## Iteración 16 — Frontend: Perfil propio 💡

**Entregable:** pantalla de US-005 — solo nombre/email editables.
**Done-when:** tests de componente de `US-005.feature` pasan.

## Iteración 17 — Frontend: Matriz de permisos 💡

**Entregable:** pantalla de US-006 — toggles por rol/recurso/acción, guardado explícito, fila del propio rol deshabilitada.
**Done-when:** tests de componente de `US-006.feature` pasan.
