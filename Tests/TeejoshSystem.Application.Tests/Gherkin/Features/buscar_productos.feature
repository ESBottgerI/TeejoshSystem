Feature: Busqueda de productos

  Scenario: Buscar productos existentes
    Given existen productos registrados
    When el administrador busca productos
    Then el sistema debe devolver resultados

  Scenario: Buscar producto inexistente
    Given no existen productos registrados
    When el administrador busca productos
    Then el sistema no debe devolver resultados