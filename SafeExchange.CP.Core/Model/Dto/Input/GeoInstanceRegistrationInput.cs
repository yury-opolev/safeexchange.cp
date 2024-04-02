/// <summary>
/// GeoInstanceRegistrationInput
/// </summary>

namespace SafeExchange.CP.Core.Model.Dto.Input
{
    using System;

    public class GeoInstanceRegistrationInput
    {
        public string LocationName { get; set; }

        public string DisplayName { get; set; }

        public string InstancePrefix { get; set; }

        public string InstanceBaseUrl { get; set; }
    }
}
