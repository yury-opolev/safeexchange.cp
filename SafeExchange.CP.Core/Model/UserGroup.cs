/// <summary>
/// UserGroup
/// </summary>

namespace SafeExchange.CP.Core.Model
{
    using Microsoft.EntityFrameworkCore;

    [Owned]
    public class UserGroup
    {
        public string AadGroupId { get; set; }
    }
}
