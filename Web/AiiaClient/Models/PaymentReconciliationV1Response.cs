﻿using System.Collections.Generic;

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

    // This string is a date-time. Here we use string to keep it simple, but it should be an appropriate type like NodaTime's Instant
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