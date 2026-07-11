@epic_id:EPIC-001
Feature: Historial de auditoría de cambios sobre usuarios
  Como sistema
  Quiero registrar en base de datos cada cambio aplicado a un registro de usuario
  Para poder reconstruir después quién hizo qué y cuándo

  @story_id:US-008-AUD @origin:analisis_completitud @area:auditoria @priority:1 @complexity:high @smoke
  Scenario: Crear un usuario genera su registro de auditoría
    Given estoy autenticado como Admin
    When creo una cuenta de usuario
    Then queda un registro de auditoría con fecha, hora, quién lo hizo y qué se creó

  @story_id:US-008-AUD @origin:analisis_completitud @area:auditoria @priority:1 @complexity:medium @data_driven
  Scenario Outline: Cada operación de escritura genera su propio registro de auditoría
    Given estoy autenticado como Admin
    When realizo la operación "<operacion>" sobre un usuario existente
    Then queda un registro de auditoría con fecha, hora, quién lo hizo y qué cambió

    Examples:
      | operacion                    |
      | editar datos                 |
      | eliminar (soft delete)       |
      | cambiar rol                  |
      | cambiar estado               |

  @story_id:US-008-AUD @origin:analisis_completitud @area:auditoria @priority:1 @complexity:high @error_handling
  Scenario: Si falla el registro de auditoría, la operación también falla
    Given el mecanismo de registro de auditoría no puede escribir
    When intento editar un usuario
    Then la edición no se persiste
    And el sistema muestra un mensaje de error genérico

  @story_id:US-008-AUD @origin:analisis_completitud @area:auditoria @priority:2 @complexity:low
  Scenario: Los registros de auditoría no pueden editarse ni eliminarse
    Given existe un registro de auditoría de un cambio anterior
    When intento modificar o eliminar ese registro directamente
    Then la operación es rechazada — el registro es de solo lectura una vez creado

  @story_id:US-008-AUD @origin:analisis_completitud @area:mantenimiento @priority:3 @complexity:medium
  Scenario: Los registros de auditoría se purgan después de 6 meses
    Given existe un registro de auditoría con más de 6 meses de antigüedad
    When se ejecuta la rutina de retención de datos
    Then ese registro es purgado de la base de datos

  @story_id:US-008-AUD @origin:analisis_completitud @area:mantenimiento @priority:3 @complexity:low
  Scenario: Los registros de auditoría de menos de 6 meses se conservan
    Given existe un registro de auditoría con menos de 6 meses de antigüedad
    When se ejecuta la rutina de retención de datos
    Then ese registro permanece en la base de datos
