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
    public ulong Id { get; set; }
    // Internal user
    public ApplicationUser User { get; set; }
    // Internal: Date when we received the webhook in UTC
    public DateTime ReceivedAt { get; set; }
    
    // Guid id provided by Aiia.
    public Guid EventId { get; set; }
    // Timestamp of when the webhook originated.
    public ulong Timestamp { get; set; }
    public string EventType { get; set; }
    public string Signature { get; set; }
    public string DataAsJson { get; set; }
}