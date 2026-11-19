using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace ClinicMS.Domain.Entities;

public partial class utblCMSRefreshToken
{
    public string Id { get; set; } = Guid.NewGuid().ToString();

    public string Token { get; set; } = null!;

    public string UserId { get; set; } = null!;

    public DateTime ExpiresAt { get; set; }

    public bool IsRevoked { get; set; }

    public DateTime TransDate { get; set; }
}
