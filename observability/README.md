# Observabilidad

Esta carpeta contiene la configuración utilizada para la instrumentación SRE del proyecto TeejoshSystem.

## Componentes

- Prometheus
- Grafana
- Alertmanager

---

## Estructura

```
observability/

    prometheus/
        prometheus.yml
        rules.yml

    alertmanager/
        alertmanager.yml

    grafana/
        dashboard.json
```

---

## Prometheus

Ejecutar:

```bash
./prometheus \
    --config.file=prometheus.yml
```

Interfaz:

```
http://localhost:9090
```

---

## Grafana

Importar:

```
grafana/dashboard.json
```

---

## Alertmanager

Ejecutar:

```bash
./alertmanager \
    --config.file=alertmanager.yml
```

Interfaz:

```
http://localhost:9093
```

---

## Endpoint instrumentado

```
http://localhost:5050/metrics
```

---

## Métricas propias

- teejosh_login_success_total
- teejosh_login_failure_total
- teejosh_sale_success_total
- teejosh_sale_failure_total
- teejosh_sale_duration_seconds
- teejosh_product_created_total
- teejosh_product_deleted_total

---

## Dashboard

El dashboard utilizado durante el laboratorio se encuentra en:

```
grafana/dashboard.json
```

Puede importarse directamente desde Grafana.