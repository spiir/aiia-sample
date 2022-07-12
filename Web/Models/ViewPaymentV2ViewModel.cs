namespace Aiia.Sample.Models;

public class ViewPaymentV2ViewModel
{
    public ViewPaymentV2ViewModel(object payment)
    {
        Payment = payment;

    }

    public object Payment { get; set; }
    
}