@epic_id:EPIC-001
Feature: Editar cuenta de usuario
  Como Admin
  Quiero editar los datos, el rol y el estado de un usuario
  Para mantener la información y los accesos al día

  Background:
    Given existe un usuario "Ana Pérez" con rol "Editor" y estado "activo"

  @story_id:US-003 @origin:discovery_inicial @priority:1 @complexity:medium @smoke
  Scenario: Editar los datos de otro usuario
    Given estoy autenticado como Admin
    When edito a "Ana Pérez" cambiando su rol a "Viewer"
    Then los cambios quedan guardados
    And "Ana Pérez" ahora tiene rol "Viewer"

  @story_id:US-003 @origin:discovery_inicial @priority:1 @complexity:low
  Scenario: El selector de rol se deshabilita al editar la propia cuenta
    Given estoy autenticado como Admin y edito mi propia cuenta
    When abro el formulario de edición
    Then el selector de rol aparece deshabilitado con una explicación de por qué

  @story_id:US-003 @origin:discovery_inicial @priority:1 @complexity:low @error_handling
  Scenario: Intentar cambiar el propio rol vía manipulación directa
    Given estoy autenticado como Admin
    When envío directamente al backend una solicitud para cambiar mi propio rol
    Then el backend rechaza la operación explicando que no puedo modificar mi propio rol

  @story_id:US-003 @origin:discovery_inicial @priority:1 @complexity:low @error_handling
  Scenario: Email duplicado al editar
    Given existe otro usuario con email "otro@mail.com"
    When edito a "Ana Pérez" y le asigno el email "otro@mail.com"
    Then el sistema muestra un mensaje general indicando que el correo ya está registrado
    And no se guardan los cambios

  @story_id:US-003 @origin:discovery_inicial @priority:2 @complexity:low @error_handling
  Scenario: Fallo general del backend al editar
    Given el backend no responde al intentar guardar
    When edito a "Ana Pérez" con datos válidos
    Then el sistema muestra un mensaje de error genérico

  @story_id:US-003 @origin:analisis_completitud @area:usuarios @priority:1 @complexity:high
  Scenario: No se puede inactivar al único Admin activo
    Given "Carlos Gómez" es el único usuario con rol Admin y estado "activo"
    When intento cambiar el estado de "Carlos Gómez" a "inactivo"
    Then el sistema rechaza el cambio explicando que debe existir al menos un Admin activo

  @story_id:US-003 @origin:analisis_completitud @area:usuarios @priority:1 @complexity:high
  Scenario: No se puede cambiar el rol del único Admin activo
    Given "Carlos Gómez" es el único usuario con rol Admin y estado "activo"
    When intento cambiar el rol de "Carlos Gómez" a "Editor"
    Then el sistema rechaza el cambio explicando que debe existir al menos un Admin activo

  @story_id:US-003 @origin:analisis_completitud @area:seguridad @priority:1 @complexity:medium
  Scenario: Autorización server-side para editar otras cuentas
    Given estoy autenticado con rol "Editor"
    When envío directamente al backend una solicitud para editar la cuenta de otro usuario
    Then el backend rechaza la operación por falta de permisos

  @story_id:US-003 @origin:analisis_completitud @area:seguridad @priority:2 @complexity:low
  Scenario: El formulario de edición nunca expone la contraseña actual
    Given estoy autenticado como Admin
    When abro el formulario de edición de "Ana Pérez"
    Then la respuesta que alimenta el formulario no incluye la contraseña ni su hash

  @story_id:US-003 @origin:analisis_completitud @area:auditoria @priority:2 @complexity:low @data_driven
  Scenario Outline: Normalización de nombre antes de persistir
    Given edito el nombre de "Ana Pérez" a "<nombre_ingresado>"
    When guardo los cambios
    Then el nombre se guarda como "<nombre_normalizado>"

    Examples:
      | nombre_ingresado    | nombre_normalizado |
      | "  Ana   Gómez  "   | "Ana Gómez"         |
