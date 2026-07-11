# SPEC — Módulo de Usuarios, Roles y Permisos (CIDENET)

> Generado por `/discovery` a partir del caso aportado por Julián. Especificación técnica completa: historias, criterios de aceptación y reglas de negocio, validados durante la entrevista (Épicas → Historias → Criterios → Completitud → Gherkin).

## Historias de usuario

- **US-001** — Como Admin quiero crear una cuenta de usuario (nombre, email, contraseña, rol, estado) para dar acceso a nuevas personas.
- **US-002** — Como Admin quiero ver una tabla paginada de usuarios con búsqueda y filtros para encontrar y gestionar cuentas rápidamente.
- **US-003** — Como Admin quiero editar los datos, el rol y el estado de un usuario para mantener la información y los accesos al día.
- **US-004** — Como Admin quiero eliminar una cuenta de usuario, con confirmación explícita, para revocar accesos.
- **US-005** — Como usuario de cualquier rol quiero editar mi propio perfil, sin poder cambiar mi propio rol ni mis permisos.
- **US-006** — Como Admin quiero activar/desactivar permisos por rol en una matriz para ajustar qué puede hacer cada rol.
- **US-007** — Como usuario del sistema quiero autenticarme con mis credenciales; el sistema debe impedir el acceso a usuarios inactivos.
- **US-008-AUD** — *(historia de gap, fase Completitud)* Como sistema quiero registrar en base de datos cada cambio sobre un usuario, para poder auditar quién hizo qué y cuándo.

## Reglas de negocio

Explícitas del caso original:
- El email debe ser único en todo el sistema.
- Todo usuario debe tener exactamente un rol asignado (obligatorio).
- Un usuario no puede modificar su propio rol.
- Un usuario no puede asignarse permisos a sí mismo.
- Un usuario inactivo no puede autenticarse.
- No se puede eliminar el último usuario con rol Admin.

Descubiertas durante Criterios y Completitud:
- La contraseña requiere mínimo 8 caracteres, con mayúscula, minúscula, número y símbolo (`$ & * # @`); se confirma con un campo duplicado.
- El email se normaliza siempre a minúsculas antes de guardar o comparar; existe una restricción de índice único a nivel de base de datos.
- Las cadenas de texto (nombre, apellido) se normalizan antes de persistir: trim y colapso de espacios múltiples.
- La regla del "último Admin" en realidad protege al último Admin **activo** — un Admin inactivo no cuenta como protección; aplica tanto a eliminar como a inactivar/cambiar de rol.
- La contraseña (o su hash) nunca se retorna en ninguna respuesta del sistema — es un campo de solo escritura.
- Toda acción de escritura valida la autorización en el **backend**, sin depender de que la interfaz oculte controles: Admin gestiona cualquier cuenta; Editor/Viewer solo la propia (nunca su rol ni su estado); solo Admin toca la matriz de permisos.
- La eliminación de un usuario es **lógica** (soft delete): el registro permanece en base de datos marcado en un estado "eliminado" — distinto de "inactivo" — que bloquea autenticación y cambios posteriores, y se excluye siempre de la tabla de usuarios.
- Tras 3 intentos fallidos de login consecutivos, la cuenta se bloquea temporalmente por 15 minutos, liberándose automáticamente; el conteo se reinicia tras un login exitoso.
- Cada operación de escritura sobre un usuario genera un registro de auditoría (fecha, hora, quién, qué cambió) a nivel de base de datos, sin UI en este MVP; los registros son append-only y se purgan a los 6 meses.
- No hay integraciones con sistemas externos, ni reportes, ni backup automatizado en el alcance de este MVP (decisiones explícitas, no huecos).

## Endpoints

| Método | Ruta | Descripción | Quién puede ejecutarlo |
|---|---|---|---|
| POST | /api/users | Crear usuario (US-001) | Admin |
| GET | /api/users | Listar/consultar usuarios con filtros y paginación (US-002) | Admin, Editor (lectura) |
| PUT/PATCH | /api/users/{id} | Editar usuario (US-003) | Admin (cualquiera); el propio usuario (solo su registro, sin rol/estado, vía US-005) |
| DELETE | /api/users/{id} | Eliminar lógicamente un usuario (US-004) | Admin |
| GET/PUT | /api/permissions | Consultar/editar la matriz de permisos (US-006) | Admin |
| POST | /api/auth/login | Autenticarse (US-007) | Cualquier usuario activo, no bloqueado |

## Pantallas

### Tabla de usuarios (US-002)
- Columnas: nombre, apellido, email, rol, estado, fecha de alta/actualización.
- Filtros: rol, nombres, apellidos, email, rango de fechas; búsqueda parcial insensible a mayúsculas; filtros combinados con AND.
- Orden por columnas; por defecto, nombres+apellidos ascendente. Paginación con tamaño elegible por el usuario.
- Estados: spinner de carga; mensaje si no hay resultados; mensaje general de error técnico.

### Formulario de creación/edición (US-001, US-003)
- Campos: nombre, apellido, email, contraseña + confirmación (solo al crear), rol, estado.
- Validaciones visibles: campos obligatorios marcados inline; contraseña con reglas de complejidad; selector de rol deshabilitado al autoeditarse.
- Estados: loading al guardar; mensaje general de éxito/fracaso.

### Perfil propio (US-005)
- Campos: nombre, apellido, email — sin rol, sin estado, sin permisos.
- Mismas validaciones de formato que el formulario de Admin.

### Matriz de permisos (US-006)
- Filas por rol (Admin/Editor/Viewer), columnas por recurso × acción CRUD, con toggles.
- Guardado explícito (botón "Guardar cambios"); fila del propio rol deshabilitada.

### Confirmación de eliminación (US-004)
- Modal con datos del usuario y advertencias; botón de confirmar deshabilitado si es el único Admin activo.

### Login (US-007)
- Campos: email, contraseña. Mensaje único para credenciales inválidas / usuario inactivo o eliminado. Aviso de bloqueo temporal tras 3 intentos fallidos.

## Criterios de aceptación (YAML)

Ver `specs/criterios/` — un archivo por historia (`US-001.yaml` … `US-008-AUD.yaml`), con las categorías `escenarios_exito`, `validaciones_reglas`, `manejo_errores`, `requisitos_ux`, `casos_edge` y `validacion_smart`.

## Cobertura (DQS-lite)

Ver `sessions/julian/dqs-lite.md` para el reporte completo de cobertura por las 10 áreas del ciclo de vida y el balance camino-feliz / camino-negativo.
