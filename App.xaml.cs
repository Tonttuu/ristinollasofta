
namespace TicTacToeApp
{
    public partial class App : Application
    {
        private MainPage mainPage;

        public App()
        {
            InitializeComponent();

            mainPage = new MainPage();
            MainPage = mainPage;
        }

        protected override void OnStart()
        {
            // Handle when your app starts
        }
        protected override void OnSleep()
        {
            // Handle when your app sleeps
            (MainPage as MainPage)?.SavePlayerData();
        }
        protected override void OnResume()
        {
            // Handle when your app resumes
        }

        private void SavePlayerData()
        {
            // Check if MainPage is an instance of MainPage
            if (mainPage is MainPage page)
            {
                // Call the SavePlayerData method on the MainPage instance
                page.SavePlayerData();
            }
        }
    }
}
