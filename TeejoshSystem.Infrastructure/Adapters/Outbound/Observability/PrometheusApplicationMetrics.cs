using Prometheus;
using TeejoshSystem.Domain.Ports.Outbound;

namespace TeejoshSystem.Infrastructure.Adapters.Outbound.Observability;

public sealed class PrometheusApplicationMetrics : IApplicationMetrics
{
    private static readonly Counter LoginSuccessCounter =
        Metrics.CreateCounter(
            "teejosh_login_success_total",
            "Número de inicios de sesión exitosos.");

    private static readonly Counter LoginFailureCounter =
        Metrics.CreateCounter(
            "teejosh_login_failure_total",
            "Número de inicios de sesión fallidos.");

    private static readonly Counter SaleSuccessCounter =
        Metrics.CreateCounter(
            "teejosh_sale_success_total",
            "Número de ventas registradas correctamente.");

    private static readonly Counter SaleFailureCounter =
        Metrics.CreateCounter(
            "teejosh_sale_failure_total",
            "Número de ventas que fallaron.");

    private static readonly Histogram SaleDurationHistogram =
        Metrics.CreateHistogram(
            "teejosh_sale_duration_seconds",
            "Tiempo empleado para registrar una venta.",
            new HistogramConfiguration
            {
                Buckets = Histogram.ExponentialBuckets(
                    start: 0.01,
                    factor: 2,
                    count: 10)
            });

    private static readonly Counter ProductCreatedCounter =
        Metrics.CreateCounter(
            "teejosh_product_created_total",
            "Número de productos creados.");

    private static readonly Counter ProductDeletedCounter =
        Metrics.CreateCounter(
            "teejosh_product_deleted_total",
            "Número de productos eliminados.");

    public void LoginSucceeded()
    {
        LoginSuccessCounter.Inc();
    }

    public void LoginFailed()
    {
        LoginFailureCounter.Inc();
    }

    public void SaleSucceeded()
    {
        SaleSuccessCounter.Inc();
    }

    public void SaleFailed()
    {
        SaleFailureCounter.Inc();
    }

    public IDisposable MeasureSaleDuration()
    {
        return SaleDurationHistogram.NewTimer();
    }

    public void ProductCreated()
    {
        ProductCreatedCounter.Inc();
    }

    public void ProductDeleted()
    {
        ProductDeletedCounter.Inc();
    }
}