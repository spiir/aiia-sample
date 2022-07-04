using System.Collections.Generic;

namespace Aiia.Sample.AiiaClient.Models;

public class PaymentReconciliationV1Response
{
    public PaymentReconciliationV1Status Status { get; set; }

    public string TransactionId { get; set; }

    public List<PaymentReconciliationEvent> Events { get; set; } = new();
}

public enum PaymentReconciliationV1Status
{
    NotStarted,
    InProgress,
    Succeeded,
    Failed,
    NotEnabledForProvider
}

public class PaymentReconciliationEvent
{
    public PaymentReconciliationEventType Event { get; set; }

    // TODO: Fix type. Here it should be datetime
    public string Timestamp { get; set; }
}

public enum PaymentReconciliationEventType
{
    Created,
    Started,
    Reconciled,
    AttemptFailed,
    ReconciliationFailed,
    StartedManually,
    AccountUnavailable,
    NotSupportedByProvider
}