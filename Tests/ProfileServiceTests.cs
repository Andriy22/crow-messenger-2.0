using BLL.Services.Abstractions;
using NUnit.Framework;

namespace Tests
{
    [TestFixture]
    public class ProfileServiceTests
    {
        private readonly IProfileService _profileService;

        public ProfileServiceTests(IProfileService profileService)
        {
            _profileService = profileService;
        }

        // we should add tests in feature 
    }
}
