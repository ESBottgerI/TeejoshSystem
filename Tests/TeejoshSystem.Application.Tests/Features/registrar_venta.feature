Feature: Registro de ventas

  Scenario: Registrar venta valida
    Given existe un producto con ID 1 llamado "Ford GT" con stock 10 y precio 25
    And se desea vender 2 unidades del producto 1
    When el administrador registra la venta
    Then la venta debe registrarse correctamente

  Scenario: Registrar venta con stock insuficiente
    Given existe un producto con ID 1 llamado "Ford GT" con stock 1 y precio 25
    And se desea vender 5 unidades del producto 1
    When el administrador registra la venta
    Then el sistema debe rechazar la venta por stock insuficiente

  Scenario: Registrar venta con producto inexistente
    Given no existe el producto con ID 99
    And se desea vender 1 unidades del producto 99
    When el administrador registra la venta
    Then el sistema debe rechazar la venta por producto inexistente