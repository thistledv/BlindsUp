using System;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace BlindsUp
{
    public partial class App : Application
    {
        public App(String blindData)
        {
            InitializeComponent();

            MainPage = new MainPage(blindData);
        }

        protected override void OnStart()
        {
        }

        protected override void OnSleep()
        {
        }

        protected override void OnResume()
        {
        }
    }
}
