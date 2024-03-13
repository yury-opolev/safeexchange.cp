﻿/// <summary>
/// AuthenticationConfiguration
/// </summary>

namespace SafeExchange.CP.Core.Configuration
{
    using System;

    public class AuthenticationConfiguration
    {
        public string MetadataAddress { get; set; }

        public bool ValidateAudience { get; set; }

        public string ValidAudiences { get; set; }

        public bool ValidateIssuer { get; set; }

        public string ValidIssuers { get; set; }
    }
}
