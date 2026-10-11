using System.Threading;
using System.Threading.Tasks;

namespace Quang.Base
{
    public interface IPurchaseGrantHandler
    {
        Task<PurchaseGrantStatus> GrantAsync(PurchaseGrantRequest request, CancellationToken token = default);
    }
}
