```yaml
metadata: { version: "1.0", alumno: "Julián", last_updated: "2026-07-11", session_status: "completed" }
project_state:
  current_phase: "completed"   # epicas | historias | criterios | completitud | gherkin | completed
  current_skill: null
  resume_hint: "Discovery completo. 8 historias, 8 criterios YAML, 8 features Gherkin, specs/SPEC.md consolidada, dqs-lite.md escrito. Siguiente paso: /plan."
epicas_phase:
  status: "completed"
  epicas:
    - id: "EPIC-001"
      nombre: "Administración de usuarios y accesos"
      descripcion: "Gestión de cuentas de usuario, asignación de roles y configuración de la matriz de permisos como un mismo bloque."
      vuifed: { valor: 10, usuarios: 10, impacto: 7, factibilidad: 7, esfuerzo: 7, dependencias: 10 }  # total 51 / prom 8.5
      mvp_included: true
  ready_for_handoff: true
historias_phase:
  status: "completed"
  answers:
    roles_por_usuario: "exactamente 1 (single-role, obligatorio)"
    alcance_mvp: "A,B,C,D,E incluidas; F (reports) excluida como funcionalidad, queda solo como permiso"
    rol_se_asigna_en: "crear (US-001) y editar (US-003), no historia aparte"
    us_007_alcance: "mínimo: login + bloqueo de inactivos (sin recuperación de contraseña ni MFA)"
  story_ids: ["US-001", "US-002", "US-003", "US-004", "US-005", "US-006", "US-007"]
  ready_for_handoff: true
criterios_phase:
  status: "completed"
  generation_mode: "US-001/US-002 por entrevista; US-003 a US-007 generados automáticamente a petición explícita de Julián (decisión suya vía AskUserQuestion, aceptando que las reglas quedan definidas por Claude en vez de descubiertas por él)"
  completed_stories: ["US-001", "US-002", "US-003", "US-004", "US-005", "US-006", "US-007"]
  answers:
    US-002:
      status: "completed"
      escenarios_exito: "filtros rol/nombres/apellidos/email/fechas; orden por columnas; paginación seleccionable; orden default nombres+apellidos asc"
      validaciones_reglas: "filtros combinados AND; fechas en rango; búsqueda parcial insensible a mayúsculas"
      manejo_errores: "error técnico -> mensaje general; sin resultados -> mensaje 'no se encontraron registros'"
      casos_edge: "datepicker UI restringe hasta<desde; backend valida también"
      requisitos_ux: "spinner de carga"
    US-001:
      status: "completed"
      escenarios_exito: "activa de inmediato + campo confirmar-contraseña (deben coincidir)"
      validaciones_reglas: "nombre/apellido obligatorio no vacío; email obligatorio formato estándar; password min 8 con mayus+minus+numero+simbolo($&*#@)"
      manejo_errores: "email duplicado -> mensaje general al guardar; campo obligatorio vacío -> marca inline por campo; fallo general -> mensaje genérico"
      casos_edge: "email normalizado a minúsculas; índice único en BD para email"
      requisitos_ux: "loading al procesar; mensaje general de éxito/fracaso al responder backend"
    US-003: { status: "completed", modo: "auto-generado" }
    US-004: { status: "completed", modo: "auto-generado" }
    US-005: { status: "completed", modo: "auto-generado" }
    US-006: { status: "completed", modo: "auto-generado" }
    US-007: { status: "completed", modo: "auto-generado" }
  ready_for_handoff: true
completitud_phase:
  status: "completed"
  current_area: "TST"        # CFG | USR | SEC | AUD | MON | INT | MNT | RPT | BCK | TST
  cobertura_areas:
    CFG: { gaps: 0, nota: "sin configuración dinámica; todo fijo en código, ajustes requieren despliegue" }
    USR: { gaps: 1, clasificacion: "🔥 crítico" }
    SEC: { gaps: 3, clasificacion: "2x 🔥 crítico + 1x ⚡ importante" }
    AUD: { gaps: 3, clasificacion: "2x ⚡ importante + 1x 🔥 crítico" }
    MON: { gaps: 1, clasificacion: "🔥 crítico" }
    INT: { gaps: 0, nota: "sin integraciones externas en este alcance" }
    MNT: { gaps: 1, clasificacion: "💡 mejora" }
    RPT: { gaps: 0, nota: "sin reportes en este MVP, consistente con la exclusión ya decidida en Historias" }
    BCK: { gaps: 0, nota: "cuentas/roles/privilegios son críticos y deberían respaldarse, pero backup automático queda fuera de este MVP (decisión explícita, no gap accionable en código)" }
    TST: { gaps: 0, nota: "participante confirma que todas las reglas críticas ya tienen escenario negativo definido; cierre de las 10 áreas" }
  ready_for_handoff: true
  gaps_descubiertos:
    - "⚡ Normalización de cadenas de texto (nombre/apellido): trim + colapso de espacios múltiples antes de persistir. Aplicado en US-001, US-003, US-005."
    - "🔥 Eliminación lógica (soft delete): el registro permanece en BD por auditoría, marcado 'eliminado' (distinto de 'inactivo'); bloquea autenticación y cambios; se excluye por completo de la tabla US-002. Aplicado en US-002, US-004, US-007."
    - "⚡ Historial de auditoría a nivel de BD (quién/cuándo/qué cambió) por cada escritura sobre un usuario — sin UI en este MVP. Nueva historia: US-008-AUD."
    - "🔥 Bloqueo temporal de cuenta tras 3 intentos fallidos de login consecutivos, liberado automáticamente a los 15 minutos. Aplicado en US-007 (reemplaza la nota anterior de 'sin bloqueo por intentos')."
    - "💡 Retención de auditoría: los registros de US-008-AUD se conservan solo 6 meses, luego se purgan."
    - "Rol obligatorio en la transacción de creación/edición — nunca queda un usuario sin rol (confirma R5, sin gap nuevo)."
    - "🔥 No se puede eliminar/inactivar un Admin si es el único Admin ACTIVO — un Admin inactivo no protege la regla (refina R1). Aplicado en US-003 y US-004."
    - "🔥 La contraseña (ni su hash) nunca se retorna en ninguna respuesta del sistema — campo de solo escritura. Aplicado en US-001, US-002, US-003."
    - "🔥 Autorización server-side obligatoria en toda acción de escritura: Admin puede modificar cualquier cuenta; Editor/Viewer solo la propia (nunca su rol); solo Admin puede tocar la matriz de permisos. El backend valida en cada request, nunca confía solo en la UI. Aplicado en US-003, US-004, US-005, US-006."
    - "⚡ El estado (activo/inactivo) no es editable desde el perfil propio (US-005) — nadie puede autodesactivarse/autoactivarse; solo Admin lo cambia vía US-003."
gherkin_phase:
  status: "completed"
  features_generated: ["US-001", "US-002", "US-003", "US-004", "US-005", "US-006", "US-007", "US-008-AUD"]
  ready_for_handoff: true
handoff_history:
  - { from: "epicas", to: "historias", at: "2026-07-10" }
  - { from: "historias", to: "criterios", at: "2026-07-10" }
  - { from: "criterios", to: "completitud", at: "2026-07-11" }
  - { from: "completitud", to: "gherkin", at: "2026-07-11" }
  - { from: "gherkin", to: "completed", at: "2026-07-11" }
```
