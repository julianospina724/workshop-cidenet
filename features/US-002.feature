@epic_id:EPIC-001
Feature: Consultar usuarios (tabla)
  Como Admin
  Quiero ver una tabla paginada de usuarios con búsqueda y filtros
  Para encontrar y gestionar cuentas rápidamente

  Background:
    Given existen varios usuarios con distintos roles, estados y fechas de alta

  @story_id:US-002 @origin:discovery_inicial @priority:1 @complexity:medium @smoke
  Scenario: Ver la tabla con el orden por defecto
    Given estoy autenticado como Admin
    When abro la tabla de usuarios sin aplicar ningún filtro
    Then los usuarios se muestran ordenados por nombres y apellidos, de forma ascendente

  @story_id:US-002 @origin:discovery_inicial @priority:1 @complexity:medium
  Scenario: Combinar varios filtros a la vez
    Given estoy autenticado como Admin
    When filtro por rol "Editor" y busco el texto "juan" en el nombre
    Then la tabla muestra solo usuarios con rol "Editor" cuyo nombre contiene "juan"

  @story_id:US-002 @origin:discovery_inicial @priority:1 @complexity:low @data_driven
  Scenario Outline: Búsqueda parcial e insensible a mayúsculas
    Given existe el usuario "Juan Pérez"
    When busco el texto "<texto_buscado>"
    Then el resultado <resultado>

    Examples:
      | texto_buscado | resultado                        |
      | juan          | incluye a "Juan Pérez"           |
      | JUAN          | incluye a "Juan Pérez"           |
      | juana         | no incluye a "Juan Pérez"        |

  @story_id:US-002 @origin:discovery_inicial @priority:2 @complexity:low @error_handling
  Scenario: Filtro de fechas con rango invertido rechazado por el backend
    Given envío directamente al backend un filtro de fechas con "desde" posterior a "hasta"
    When se procesa la solicitud
    Then el backend rechaza el filtro por rango de fechas inválido

  @story_id:US-002 @origin:discovery_inicial @priority:2 @complexity:low
  Scenario: El datepicker de "hasta" restringe fechas anteriores a "desde"
    Given selecciono una fecha "desde" en el filtro
    When intento abrir el selector de fecha "hasta"
    Then las fechas anteriores a "desde" no son seleccionables

  @story_id:US-002 @origin:discovery_inicial @priority:2 @complexity:low
  Scenario: Búsqueda sin resultados
    Given no existe ningún usuario que coincida con los filtros aplicados
    When aplico esos filtros
    Then la tabla muestra un mensaje indicando que no se encontraron registros

  @story_id:US-002 @origin:discovery_inicial @priority:2 @complexity:low @error_handling
  Scenario: Error técnico al consultar la tabla
    Given el backend no responde al consultar la tabla
    When abro la tabla de usuarios
    Then el sistema muestra un mensaje general de error

  @story_id:US-002 @origin:analisis_completitud @area:seguridad @priority:1 @complexity:low
  Scenario: La tabla nunca expone la contraseña
    Given existen usuarios en el sistema
    When consulto la tabla de usuarios
    Then ninguna fila incluye la contraseña ni su hash

  @story_id:US-002 @origin:analisis_completitud @area:auditoria @priority:1 @complexity:medium
  Scenario: Los usuarios eliminados nunca aparecen en la tabla
    Given el usuario "Carlos Ruiz" está en estado "eliminado"
    When consulto la tabla de usuarios, con o sin filtro de estado
    Then "Carlos Ruiz" no aparece en ningún resultado
