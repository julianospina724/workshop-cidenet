# Reporte DQS-lite — Discovery de Julián

> Cierre del discovery del módulo de Usuarios, Roles y Permisos (CIDENET). Cobertura descriptiva por área — sin comparar contra un número objetivo, eso lo calibra el facilitador.

## Cobertura por área de completitud

- **⚙️ Configuración** — explorada. Decisión explícita y consciente: nada es configurable dinámicamente (política de contraseña, tamaño de página); cualquier ajuste requiere código y despliegue. No es un hueco, es una decisión de alcance.
- **👥 Usuarios / acceso** — explorada con un hallazgo importante: la protección del "último Admin" original del caso solo hablaba de rol; la entrevista llevó a precisar que en realidad protege al último Admin *activo* (un Admin inactivo no cuenta). Quedó aplicado en edición y eliminación.
- **🔒 Seguridad / autorización** — la más productiva de las 10 áreas. Salieron tres reglas que el caso no mencionaba en absoluto: la contraseña como campo de solo escritura (nunca se retorna), la autorización server-side obligatoria en toda escritura (no basta con ocultar botones en la UI), y que el estado propio tampoco es editable desde el perfil. Bien cubierta.
- **🗃️ Auditoría / integridad** — muy productiva. Definiste normalización de texto (trim + espacios), y sobre todo el diseño de la eliminación como lógica (no física) con un estado "eliminado" distinto de "inactivo" — una decisión de modelo de datos que no estaba en el brief. También salió de aquí una historia completamente nueva: el historial de auditoría (US-008-AUD).
- **📈 Monitoreo** — explorada, con un gap crítico: bloqueo temporal de cuenta tras 3 intentos fallidos (15 min, liberación automática). Esto reemplazó una suposición mía anterior ("sin bloqueo por intentos") — vale la pena que sepas que esa primera versión de US-007 fue una asunción que la completitud corrigió.
- **🔗 Integraciones** — explorada, sin gaps: decisión explícita de no interactuar con sistemas externos en este alcance.
- **🔧 Mantenimiento** — explorada: retención de auditoría a 6 meses, luego purga. Un gap de prioridad baja pero bien definido.
- **📊 Reportes** — explorada, sin gaps: consistente con la exclusión de "reports" como funcionalidad ya decidida en Historias.
- **💾 Backup** — explorada: reconociste que cuentas/roles/permisos son datos críticos que deberían respaldarse, pero el respaldo automático queda fuera de este MVP (responsabilidad de infraestructura, no de este módulo).
- **🧪 Testing (cierre)** — confirmaste que las reglas críticas descubiertas ya tienen su escenario negativo definido. No quedó ninguna regla 🔥 sin un caso que la viole a propósito.

**Ninguna área quedó floja.** Las 10 se recorrieron con al menos una pregunta sustantiva, y la mayoría produjo al menos un gap real frente al caso original.

## Balance camino-feliz / camino-negativo

Cada uno de los 8 `.feature` generados tiene al menos un escenario de camino feliz y al menos un escenario negativo (`@error_handling`) por cada regla crítica descubierta. Las reglas con más superficie de fallo (autenticación, eliminación, edición del último Admin) tienen varios escenarios negativos, incluyendo `Scenario Outline` para las validaciones de campo con múltiples valores de entrada. Los escenarios etiquetados `@origin:analisis_completitud` (12 de los ~50 escenarios totales) son, en su mayoría, los que prueban una regla que el caso no mencionaba — vale la pena repasarlos con cuidado en el `/test` de mañana, porque son los que más fácil se quedan sin cubrir si se programa solo desde el brief original.

## Nota sobre el modo de esta sesión

A diferencia de la fase de Épicas e Historias (y de US-001/US-002 en Criterios), que siguieron la entrevista pregunta a pregunta, **US-003 a US-007 de Criterios se generaron automáticamente** a tu pedido explícito. Eso significa que varias decisiones de esas 5 historias (ej. el mensaje anti-enumeración de US-007 original, el guardado explícito de la matriz en US-006) fueron mías, no tuyas — te las señalé en su momento para que las revisaras. La fase de Completitud sí la hiciste completa, pregunta a pregunta, y ahí es donde salió la mayoría de las correcciones y gaps reales.

## Huecos evidentes / invitación a volver

No detecto un área completa sin explorar. Si quieres blindar más el discovery antes de pasar a construcción, los dos puntos con más superficie para una segunda pasada de `/discovery resume` → completitud serían:
- Revisar a fondo los criterios de US-003 a US-007 (generados automáticamente) contra tu propio criterio de negocio, no solo el mío.
- El mensaje exacto al usuario cuando su cuenta está bloqueada por intentos fallidos (US-007) quedó como "puede reintentar más tarde" — si quieres decidir si se muestra el tiempo restante exacto, es una decisión de UX que no se profundizó.

Fuera de eso, el discovery se siente sólido para pasar a `/plan`.
