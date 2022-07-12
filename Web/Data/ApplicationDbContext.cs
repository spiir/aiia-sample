using System;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Aiia.Sample.Data;

public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
{
    public DbSet<Webhook> Webhooks { get; set; }
    
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }
}

public class Webhook
{
    // internal (progressive) integer id.
    public long Id { get; set; }
    // Internal user
    public ApplicationUser User { get; set; }
    // Internal: Timestamp of when we received the webhook.
    public long ReceivedAtTimestamp { get; set; }
    
    // Guid id provided by Aiia.
    public Guid EventId { get; set; }
    // Timestamp of when the webhook originated.
    public long Timestamp { get; set; }
    public string EventType { get; set; }
    public string Signature { get; set; }
    public string DataAsJson { get; set; }
}