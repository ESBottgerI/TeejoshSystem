Feature: Registro de productos

  Scenario: Registrar producto Hot Wheels valido
    Given existe un producto Hot Wheels llamado "Ford GT"
    And el producto tiene precio 25
    And el producto tiene stock 10
    When el administrador registra el producto
    Then el producto debe registrarse correctamente

  Scenario: Registrar producto con precio negativo
    Given existe un producto Hot Wheels llamado "Ford GT"
    And el producto tiene precio -10
    And el producto tiene stock 10
    When el administrador registra el producto
    Then el sistema debe rechazar el registro

  Scenario: Registrar producto sin detalle requerido
    Given existe un producto sin detalle
    When el administrador registra el producto
    Then el sistema debe rechazar el registro