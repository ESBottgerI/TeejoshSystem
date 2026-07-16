using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TeejoshSystem.Domain.Ports.Outbound;

public interface IApplicationMetrics
{
    void LoginSucceeded();
    void LoginFailed();

    void SaleSucceeded();
    void SaleFailed();

    IDisposable MeasureSaleDuration();

    void ProductCreated();
    void ProductDeleted();
}