using System;
using System.Collections.Generic;

namespace ClinicMS.Domain.Entities;

public partial class RefreshToken
{
    public string Id { get; set; } = Guid.NewGuid().ToString();

    public string Token { get; set; } = null!;

    public string UserId { get; set; } = null!;

    public DateTime ExpiresAt { get; set; }

    public bool IsRevoked { get; set; }

    public DateTime CreatedAt { get; set; }
}
