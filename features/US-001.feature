@epic_id:EPIC-001
Feature: Crear cuenta de usuario
  Como Admin
  Quiero crear una cuenta de usuario (nombre, email, contraseña, rol y estado inicial)
  Para dar acceso a nuevas personas dentro del sistema

  Background:
    Given estoy autenticado como Admin

  @story_id:US-001 @origin:discovery_inicial @priority:1 @complexity:medium @smoke
  Scenario: Crear una cuenta con datos válidos
    Given lleno el formulario con nombre "Ana Pérez", email "ana.perez@mail.com", contraseña "Clave123$" y confirmación "Clave123$", y selecciono el rol "Editor"
    When envío el formulario
    Then la cuenta queda creada y activa de inmediato
    And el usuario "ana.perez@mail.com" puede autenticarse con esa contraseña

  @story_id:US-001 @origin:discovery_inicial @priority:1 @complexity:low @error_handling
  Scenario: La contraseña y su confirmación no coinciden
    Given lleno el formulario con contraseña "Clave123$" y confirmación "Clave999$"
    When intento enviar el formulario
    Then el sistema no permite el envío y señala que ambos campos deben coincidir

  @story_id:US-001 @origin:discovery_inicial @priority:1 @complexity:low @error_handling @data_driven
  Scenario Outline: Validar reglas de formato de campos obligatorios
    Given lleno el formulario con <campo> = "<valor>"
    When intento enviar el formulario
    Then el sistema <resultado>

    Examples:
      | campo      | valor              | resultado                                                        |
      | nombre     |                    | marca el campo nombre como obligatorio, sin enviar el formulario |
      | email      | correo-invalido    | marca el campo email como inválido, sin enviar el formulario     |
      | contraseña | corta1             | marca la contraseña como inválida (no cumple la complejidad mínima) |
      | contraseña | sololetrasminuscul | marca la contraseña como inválida (falta mayúscula, número y símbolo) |

  @story_id:US-001 @origin:discovery_inicial @priority:1 @complexity:low @error_handling
  Scenario: Email ya registrado
    Given ya existe una cuenta con email "ana.perez@mail.com"
    When intento crear otra cuenta con email "ana.perez@mail.com"
    Then el sistema muestra un mensaje general indicando que el correo ya está registrado
    And no se crea la cuenta

  @story_id:US-001 @origin:discovery_inicial @priority:2 @complexity:low
  Scenario: Fallo general del backend al crear
    Given el backend no responde al intentar guardar
    When envío el formulario con datos válidos
    Then el sistema muestra un mensaje de error genérico
    And no se crea la cuenta

  @story_id:US-001 @origin:discovery_inicial @priority:2 @complexity:low @data_driven
  Scenario Outline: Normalización de email y nombre antes de persistir
    Given lleno el formulario con nombre "<nombre_ingresado>" y email "<email_ingresado>"
    When envío el formulario con datos válidos
    Then la cuenta se guarda con nombre "<nombre_normalizado>" y email "<email_normalizado>"

    Examples:
      | nombre_ingresado   | email_ingresado   | nombre_normalizado | email_normalizado |
      | " Ana   Pérez  "   | "Ana@Mail.com"    | "Ana Pérez"        | "ana@mail.com"    |

  @story_id:US-001 @origin:analisis_completitud @area:seguridad @priority:1 @complexity:medium
  Scenario: La contraseña nunca se retorna en la respuesta de creación
    When creo una cuenta con datos válidos
    Then la respuesta del sistema no incluye la contraseña ni su hash en ningún campo

  @story_id:US-001 @origin:analisis_completitud @area:auditoria @priority:2 @complexity:medium
  Scenario: Unicidad de email protegida a nivel de base de datos
    Given dos solicitudes de creación llegan casi simultáneamente con el mismo email "ana.perez@mail.com"
    When ambas intentan persistirse a la vez
    Then solo una cuenta queda creada y la otra falla por violación de la restricción única de email
