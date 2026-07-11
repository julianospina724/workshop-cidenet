# US-008-AUD · Historial de auditoría de cambios sobre usuarios

**Como** sistema (con fines de gobernanza y trazabilidad)
**quiero** registrar en base de datos cada cambio aplicado a un registro de usuario
**para** poder reconstruir después quién hizo qué y cuándo.

**Origen:** análisis de completitud, área 🗃️ AUD — no estaba en el brief original ni en las historias iniciales.

## Alcance

- Por cada cambio sobre un registro de usuario (creación, edición, eliminación lógica, cambios de rol/estado), se registra en base de datos: fecha, hora, usuario que lo realizó y qué cambió (valores anterior/nuevo, al menos de forma resumida).
- **Sin UI en este MVP** — es solo un registro a nivel de base de datos; la pantalla de consulta queda fuera de alcance (candidata a una iteración futura).

## Reglas explícitas relacionadas

- Ninguna del brief original — es un gap descubierto en la fase de Completitud.

## Nota INVEST

Independiente (es un mecanismo transversal de registro, no una pantalla). Negociable (granularidad del "qué cambió" se puede afinar). Valiosa (trazabilidad y gobernanza de un módulo de seguridad). Estimable y Small (tabla/mecanismo de auditoría + hooks en las operaciones de escritura existentes). Testable (se puede verificar que cada operación de escritura deja su registro).
