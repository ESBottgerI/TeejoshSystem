using System.Threading;
using System.Threading.Tasks;
﻿

namespace TeejoshSystem.AvaloniaUI.Adapters.Inbound.Services
{
    public interface ILoadable
    {
        Task LoadAsync(CancellationToken cancellationToken = default);
    }
}
