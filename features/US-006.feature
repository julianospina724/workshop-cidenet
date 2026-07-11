@epic_id:EPIC-001
Feature: Configurar la matriz de permisos por rol
  Como Admin
  Quiero activar/desactivar permisos por rol en una matriz (recurso × acción CRUD)
  Para ajustar qué puede hacer cada rol sobre cada recurso

  Background:
    Given estoy autenticado como Admin
    And la matriz de permisos tiene el estado por defecto del caso

  @story_id:US-006 @origin:discovery_inicial @priority:1 @complexity:medium @smoke
  Scenario: Activar/desactivar un permiso y guardar
    When desactivo el permiso de "crear" sobre "reports" para el rol "Editor"
    And guardo los cambios
    Then la matriz refleja que "Editor" ya no puede crear "reports"

  @story_id:US-006 @origin:discovery_inicial @priority:1 @complexity:low
  Scenario: Los cambios no se persisten hasta guardar explícitamente
    When desactivo el permiso de "eliminar" sobre "users" para el rol "Editor"
    Then el cambio no queda persistido hasta que presione "Guardar cambios"

  @story_id:US-006 @origin:discovery_inicial @priority:1 @complexity:medium @error_handling
  Scenario: No puedo modificar los permisos de mi propio rol
    Given mi rol es "Admin"
    When abro la fila de la matriz correspondiente al rol "Admin"
    Then los toggles de esa fila aparecen deshabilitados

  @story_id:US-006 @origin:discovery_inicial @priority:1 @complexity:medium @error_handling
  Scenario: Intento de modificar permisos del propio rol vía manipulación directa
    Given mi rol es "Admin"
    When envío directamente al backend un cambio de permisos para el rol "Admin"
    Then el backend rechaza la operación explicando que no puedo asignarme permisos a mí mismo

  @story_id:US-006 @origin:discovery_inicial @priority:2 @complexity:low @error_handling
  Scenario: Fallo general al guardar la matriz
    Given el backend no responde al guardar
    When modifico un permiso y guardo los cambios
    Then el sistema muestra un mensaje de error genérico
    And los toggles vuelven a su estado anterior

  @story_id:US-006 @origin:analisis_completitud @area:seguridad @priority:1 @complexity:medium
  Scenario: Autorización server-side para modificar la matriz
    Given estoy autenticado con rol "Editor"
    When envío directamente al backend una solicitud para modificar la matriz de permisos
    Then el backend rechaza la operación por falta de permisos

  @story_id:US-006 @origin:discovery_inicial @priority:2 @complexity:medium
  Scenario: No se pueden crear ni eliminar roles o recursos
    When intento agregar un rol o un recurso nuevo a la matriz
    Then el sistema no ofrece ninguna opción para hacerlo — solo existen los roles y recursos fijos del caso
