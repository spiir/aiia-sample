using System.Collections.Generic;
using Aiia.Sample.AiiaClient.Models;

namespace Aiia.Sample.Models;

public class ViewPaymentV1ViewModel
{
    public ViewPaymentV1ViewModel(object payment, PaymentType paymentType, PaymentReconciliationV1Response reconciliation)
    {
        Payment = payment;
        PaymentType = paymentType;
        Reconciliation = reconciliation;

    }

    public object Payment { get; set; }
    public PaymentType PaymentType { get; set; }
    public PayerTokenModel PayerToken { get; set; }
    public PaymentReconciliationV1Response Reconciliation { get; set; }
}