using ConferenceRoomBooking.E2ETests.Support;

namespace ConferenceRoomBooking.E2ETests.Hooks
{
    [Binding]
    public class BeforeScenario
    {
        private readonly DatabaseCleaner _databaseCleaner;

        public BeforeScenario(DatabaseCleaner databaseCleaner)
        {
            _databaseCleaner = databaseCleaner;
        }

        [BeforeScenario]
        public async Task CleanDatabase()
        {
            await _databaseCleaner.CleanAsync();
        }
    }
}
