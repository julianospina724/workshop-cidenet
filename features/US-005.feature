@epic_id:EPIC-001
Feature: Editar el propio perfil
  Como usuario de cualquier rol
  Quiero editar mi propio perfil
  Para mantener mis datos personales actualizados, sin poder cambiar mi propio rol ni mis permisos

  Background:
    Given estoy autenticado como un usuario con rol "Editor"

  @story_id:US-005 @origin:discovery_inicial @priority:1 @complexity:medium @smoke
  Scenario: Editar mi propio nombre y email
    When edito mi perfil cambiando mi nombre y mi email a datos válidos
    Then los cambios quedan guardados en mi propia cuenta

  @story_id:US-005 @origin:discovery_inicial @priority:1 @complexity:low
  Scenario: El formulario de perfil propio no incluye rol ni permisos
    When abro el formulario de edición de mi propio perfil
    Then no veo ningún campo para cambiar mi rol ni mis permisos

  @story_id:US-005 @origin:discovery_inicial @priority:1 @complexity:low @error_handling
  Scenario: Email duplicado al editar el perfil propio
    Given existe otro usuario con email "otro@mail.com"
    When intento cambiar mi email a "otro@mail.com"
    Then el sistema muestra un mensaje general indicando que el correo ya está registrado

  @story_id:US-005 @origin:discovery_inicial @priority:2 @complexity:low @error_handling
  Scenario: Fallo general al guardar el perfil propio
    Given el backend no responde al intentar guardar
    When edito mi perfil con datos válidos
    Then el sistema muestra un mensaje de error genérico

  @story_id:US-005 @origin:analisis_completitud @area:seguridad @priority:1 @complexity:medium
  Scenario: No puedo cambiar mi propio rol vía manipulación directa
    When envío directamente al backend una solicitud para cambiar mi propio rol
    Then el backend rechaza la operación explicando la restricción

  @story_id:US-005 @origin:analisis_completitud @area:seguridad @priority:1 @complexity:medium
  Scenario: No puedo cambiar mi propio estado a inactivo
    When envío directamente al backend una solicitud para desactivar mi propia cuenta
    Then el backend rechaza la operación

  @story_id:US-005 @origin:analisis_completitud @area:seguridad @priority:1 @complexity:high
  Scenario: No puedo editar la cuenta de otro usuario desde este flujo
    Given existe otro usuario "Ana Pérez"
    When envío directamente al backend una solicitud para editar el perfil de "Ana Pérez" usando mi propia sesión
    Then el backend rechaza la operación por no ser mi propia cuenta

  @story_id:US-005 @origin:analisis_completitud @area:usuarios @priority:2 @complexity:medium
  Scenario: Conflicto con desactivación en paralelo
    Given un Admin desactiva mi cuenta justo antes de que yo guarde mi perfil
    When intento guardar mis cambios
    Then el sistema rechaza el guardado indicando que la cuenta ya no está activa
