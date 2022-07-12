using Aiia.Sample.AiiaClient.Models;

namespace Aiia.Sample.Models;

public class ViewAuthorizationViewModel
{
    public PaymentAuthorization Authorization { get; }

    public ViewAuthorizationViewModel(PaymentAuthorization authorization)
    {
        Authorization = authorization;
    }
}