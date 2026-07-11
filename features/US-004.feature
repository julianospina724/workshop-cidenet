@epic_id:EPIC-001
Feature: Eliminar cuenta de usuario
  Como Admin
  Quiero eliminar una cuenta de usuario, con confirmación explícita
  Para revocar el acceso de personas que ya no deben tenerlo

  Background:
    Given estoy autenticado como Admin

  @story_id:US-004 @origin:discovery_inicial @priority:1 @complexity:medium @smoke
  Scenario: Eliminar un usuario tras confirmar explícitamente
    Given existe el usuario "Ana Pérez" con rol "Editor"
    When selecciono eliminar a "Ana Pérez" y confirmo la acción en el modal
    Then la cuenta de "Ana Pérez" queda en estado "eliminado"
    And ya no aparece en la tabla de usuarios

  @story_id:US-004 @origin:discovery_inicial @priority:1 @complexity:low
  Scenario: Cancelar la eliminación sin confirmar
    Given existe el usuario "Ana Pérez"
    When selecciono eliminar a "Ana Pérez" pero no confirmo la acción en el modal
    Then la cuenta de "Ana Pérez" sigue existiendo sin cambios

  @story_id:US-004 @origin:discovery_inicial @priority:1 @complexity:medium @error_handling
  Scenario: No se puede eliminar al único Admin activo
    Given "Carlos Gómez" es el único usuario con rol Admin y estado "activo"
    When intento eliminar a "Carlos Gómez"
    Then el sistema rechaza la eliminación explicando que debe existir al menos un Admin activo

  @story_id:US-004 @origin:discovery_inicial @priority:2 @complexity:low
  Scenario: El modal advierte y bloquea la confirmación si es el único Admin
    Given "Carlos Gómez" es el único usuario con rol Admin y estado "activo"
    When abro el modal de confirmación para eliminar a "Carlos Gómez"
    Then el modal muestra una advertencia y el botón de confirmar está deshabilitado

  @story_id:US-004 @origin:discovery_inicial @priority:2 @complexity:low @error_handling
  Scenario: Fallo general del backend al eliminar
    Given el backend no responde al intentar eliminar
    When confirmo la eliminación de "Ana Pérez"
    Then el sistema muestra un mensaje de error genérico

  @story_id:US-004 @origin:discovery_inicial @priority:2 @complexity:low @error_handling
  Scenario: Eliminar un usuario que ya fue eliminado por otra sesión
    Given "Ana Pérez" ya fue eliminada en otra sesión paralela
    When confirmo la eliminación de "Ana Pérez" desde mi sesión
    Then el sistema muestra un mensaje indicando que el usuario ya no existe

  @story_id:US-004 @origin:analisis_completitud @area:auditoria @priority:1 @complexity:high
  Scenario: La eliminación es lógica, no física
    Given existe el usuario "Ana Pérez"
    When elimino a "Ana Pérez"
    Then el registro de "Ana Pérez" sigue existiendo en la base de datos, marcado como "eliminado"

  @story_id:US-004 @origin:analisis_completitud @area:auditoria @priority:1 @complexity:medium
  Scenario: Un usuario eliminado no puede autenticarse
    Given el usuario "Ana Pérez" está en estado "eliminado"
    When "Ana Pérez" intenta autenticarse con sus credenciales
    Then el sistema rechaza el acceso

  @story_id:US-004 @origin:analisis_completitud @area:seguridad @priority:1 @complexity:medium
  Scenario: Autorización server-side para eliminar
    Given estoy autenticado con rol "Editor"
    When envío directamente al backend una solicitud para eliminar a otro usuario
    Then el backend rechaza la operación por falta de permisos

  @story_id:US-004 @origin:analisis_completitud @area:testing @priority:2 @complexity:low
  Scenario: Doble clic en confirmar no duplica la eliminación
    Given existe el usuario "Ana Pérez"
    When hago doble clic muy rápido en el botón de confirmar eliminación
    Then solo se procesa una eliminación y no se genera ningún error por duplicidad
