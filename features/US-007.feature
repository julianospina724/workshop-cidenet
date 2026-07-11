@epic_id:EPIC-001
Feature: Autenticación y bloqueo de usuarios inactivos
  Como usuario del sistema
  Quiero autenticarme con mis credenciales
  Para acceder a la plataforma; y el sistema debe impedir el acceso a usuarios inactivos

  @story_id:US-007 @origin:discovery_inicial @priority:1 @complexity:medium @smoke
  Scenario: Autenticarse con credenciales correctas
    Given existe el usuario "ana.perez@mail.com" con estado "activo" y contraseña "Clave123$"
    When inicio sesión con email "ana.perez@mail.com" y contraseña "Clave123$"
    Then el sistema me autentica y accede a la plataforma

  @story_id:US-007 @origin:discovery_inicial @priority:1 @complexity:low @error_handling
  Scenario: Credenciales incorrectas
    Given existe el usuario "ana.perez@mail.com" con contraseña "Clave123$"
    When inicio sesión con email "ana.perez@mail.com" y contraseña "otraClave"
    Then el sistema muestra el mensaje "email o contraseña incorrectos"
    And no accedo a la plataforma

  @story_id:US-007 @origin:discovery_inicial @priority:1 @complexity:low @error_handling
  Scenario: Usuario inactivo con credenciales correctas
    Given existe el usuario "ana.perez@mail.com" con estado "inactivo" y contraseña "Clave123$"
    When inicio sesión con email "ana.perez@mail.com" y contraseña "Clave123$"
    Then el sistema muestra el mismo mensaje "email o contraseña incorrectos"
    And no accedo a la plataforma

  @story_id:US-007 @origin:analisis_completitud @area:auditoria @priority:1 @complexity:medium
  Scenario: Usuario eliminado no puede autenticarse
    Given existe el usuario "ana.perez@mail.com" en estado "eliminado" con contraseña "Clave123$"
    When inicio sesión con email "ana.perez@mail.com" y contraseña "Clave123$"
    Then el sistema rechaza el acceso

  @story_id:US-007 @origin:discovery_inicial @priority:2 @complexity:low @data_driven
  Scenario Outline: Login normaliza el email a minúsculas
    Given existe el usuario con email "ana.perez@mail.com"
    When inicio sesión con email "<email_ingresado>" y la contraseña correcta
    Then el sistema me autentica correctamente

    Examples:
      | email_ingresado     |
      | Ana.Perez@Mail.com  |
      | ANA.PEREZ@MAIL.COM  |

  @story_id:US-007 @origin:analisis_completitud @area:monitoreo @priority:1 @complexity:high
  Scenario: Bloqueo tras 3 intentos fallidos consecutivos
    Given existe el usuario "ana.perez@mail.com" con estado "activo"
    And ya fallé 2 intentos de login consecutivos para esa cuenta
    When intento iniciar sesión con la contraseña incorrecta una vez más
    Then la cuenta queda bloqueada temporalmente
    And el sistema muestra un mensaje indicando que la cuenta está bloqueada por intentos fallidos

  @story_id:US-007 @origin:analisis_completitud @area:monitoreo @priority:1 @complexity:medium @error_handling
  Scenario: Intentar autenticarse mientras la cuenta está bloqueada
    Given la cuenta "ana.perez@mail.com" está bloqueada temporalmente por intentos fallidos
    When intento iniciar sesión con las credenciales correctas
    Then el sistema rechaza el acceso indicando que la cuenta está bloqueada temporalmente

  @story_id:US-007 @origin:analisis_completitud @area:monitoreo @priority:2 @complexity:medium
  Scenario: El bloqueo se libera automáticamente después de 15 minutos
    Given la cuenta "ana.perez@mail.com" fue bloqueada hace 15 minutos o más
    When intento iniciar sesión con las credenciales correctas
    Then el sistema me autentica correctamente

  @story_id:US-007 @origin:analisis_completitud @area:monitoreo @priority:2 @complexity:low
  Scenario: El conteo de intentos fallidos se reinicia tras un login exitoso
    Given ya fallé 2 intentos de login consecutivos para "ana.perez@mail.com"
    When inicio sesión exitosamente con las credenciales correctas
    And luego fallo un intento de login
    Then la cuenta no queda bloqueada, porque el conteo se reinició tras el login exitoso

  @story_id:US-007 @origin:discovery_inicial @priority:2 @complexity:low @error_handling
  Scenario: Fallo general del backend al autenticarse
    Given el backend no responde al intentar autenticar
    When inicio sesión con credenciales válidas
    Then el sistema muestra un mensaje de error genérico, distinto al de credenciales inválidas
