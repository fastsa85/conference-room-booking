using ConferenceRoomBooking.E2ETests.Support;
using ConferenceRoomBooking.E2ETests.Support.Models;
using ConferenceRoomBooking.TestInfrastructure;
using Microsoft.Extensions.Configuration;
using Reqnroll.BoDi;

namespace ConferenceRoomBooking.E2ETests.Hooks
{
    [Binding]
    public static class BeforeTestRun
    {
        [BeforeTestRun]
        public static void Initialize(IObjectContainer container)
        {
            var configuration = new ConfigurationBuilder()
                .SetBasePath(AppContext.BaseDirectory)
                .AddJsonFile("appsettings.json")
                .Build();

            var testSettings = configuration
            .GetRequiredSection("TestSettings")
            .Get<TestSettings>()
            ?? throw new InvalidOperationException("TestSettings configuration is missing.");

            container.RegisterInstanceAs(testSettings);

            var databaseCleaner = new DatabaseCleaner(testSettings.ConnectionString);

            container.RegisterInstanceAs(databaseCleaner);
        }
    }
}
