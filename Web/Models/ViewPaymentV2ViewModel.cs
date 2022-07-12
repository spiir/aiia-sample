using System.Collections.Generic;
using Aiia.Sample.AiiaClient.Models;

namespace Aiia.Sample.Models;

public class ViewPaymentV2ViewModel
{
    public ViewPaymentV2ViewModel(object payment)
    {
        Payment = payment;

    }

    public object Payment { get; set; }
    
}