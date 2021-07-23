using System;
using System.Collections.Generic;
using System.Timers;
using Xamarin.Essentials;
using Xamarin.Forms;




namespace BlindsUp
{
    public partial class MainPage : ContentPage
    {

        // describes the blinds structure: levels, blinds and antes, breaks
        // initially loaded from blinds.txt via string passed to MainPage
        List<BLevel> bStructure = new List<BLevel>();

        // loads misc game information
        // initially loaded from blinds.txt via string passed to MainPage
        GameInfo gameInfo;
        System.Timers.Timer aTimer = null;
       
        Plugin.SimpleAudioPlayer.ISimpleAudioPlayer shufflePlayer;
        Plugin.SimpleAudioPlayer.ISimpleAudioPlayer levelPlayer;
        Plugin.SimpleAudioPlayer.ISimpleAudioPlayer blindsupPlayer;

        public MainPage(string settingsData)
        {
            InitializeComponent();

            shufflePlayer = Plugin.SimpleAudioPlayer.CrossSimpleAudioPlayer.CreateSimpleAudioPlayer();
            shufflePlayer.Load("shuffle6.wav");
            levelPlayer = Plugin.SimpleAudioPlayer.CrossSimpleAudioPlayer.CreateSimpleAudioPlayer();
            levelPlayer.Load("endlevel5.wav");
            blindsupPlayer = Plugin.SimpleAudioPlayer.CrossSimpleAudioPlayer.CreateSimpleAudioPlayer();
            blindsupPlayer.Load("blindsup5.wav");

            gameInfo = new GameInfo(settingsData);

            displayMainPanel();
        }
   
  

        void displayMainPanel()
        {
            Console.WriteLine("dispMainPanel");
            Clock.Text = gameInfo.ClockString();
            Blinds.Text = gameInfo.BlindString();
            Level.Text = gameInfo.LevelString();
            MainPanel.IsVisible = true;
        }

        void OnTimedEvent(Object source, ElapsedEventArgs e)
        {
            if (gameInfo.currentState == GameState.GS_RUNNING)
            {
                gameInfo.secondsLeftLevel -= 1;

                // play recordings as needed
                if (gameInfo.secondsLeftLevel == 32)
                {
                    MainThread.BeginInvokeOnMainThread(() =>
                    {
                        levelPlayer.Play();
                    });
                }
                else if (gameInfo.secondsLeftLevel == 2)
                {
                    MainThread.BeginInvokeOnMainThread(() =>
                    {
                        blindsupPlayer.Play();
                    });
                }
                else if (gameInfo.secondsLeftLevel == 0)
                {
                    gameInfo.LevelExpired();
                }
            }

            else if(gameInfo.currentState == GameState.GS_BREAK)
            {
                gameInfo.secondsLeftBreak -= 1;
                if(gameInfo.secondsLeftBreak == 2)
                {
                    MainThread.BeginInvokeOnMainThread(() =>
                    {
                        blindsupPlayer.Play();
                    });
                }
                else if(gameInfo.secondsLeftBreak == 0)
                {
                    gameInfo.BreakExpired();
                }
            }


            MainThread.BeginInvokeOnMainThread(() =>
            {
                displayMainPanel();
            } );
 
        }

        private void Play_Clicked(object sender, EventArgs e)
        {
            BottomIconPanel.IsVisible = false;

            // if unpausing: change state, enable timer, and redisplay
            if (gameInfo.isPaused)
            {
                gameInfo.isPaused = false;
                Console.WriteLine("paused");
                aTimer.Enabled = true;
            }

            // initial startup logic -- create displayString, timer, etc
            else if (gameInfo.currentState == GameState.GS_BUYIN)
            {
                shufflePlayer.Play();
                gameInfo.StartPlay();
            }

            if (aTimer == null)
            {
                    aTimer = new System.Timers.Timer(1000);
                    // Hook up the Elapsed event for the timer.
                    aTimer.Elapsed += OnTimedEvent;
                    aTimer.AutoReset = true;
            }

            aTimer.Enabled = true;
            IBplay.IsVisible = false;
            IBpause.IsVisible = true;
            IBreverse.IsVisible = false;
            IBforward.IsVisible = false;

            displayMainPanel();
        }

        private void Pause_Clicked(object sender, EventArgs e)
        {
            aTimer.Enabled = false;
            IBpause.IsVisible = false;
            IBplay.IsVisible = true;
            IBreverse.IsVisible = true;
            IBforward.IsVisible = true;
            gameInfo.isPaused = true;
            BottomIconPanel.IsVisible = true;
            displayMainPanel();
        }
        private void Configure_Click(object sender, EventArgs e)
        {
            MainPanel.IsVisible = false;
            ConfigPanel.IsVisible = true;
            BottomIconPanel.IsVisible = false;
        }
        private void Configure_Titles(object sender, EventArgs e)
        {
            Title1Entry.Text = gameInfo.title1;
            Title2Entry.Text = gameInfo.title2;
            ConfigPanel.IsVisible = false;
            TitlePanel.IsVisible = true;
        }
       
        private void EB_NewLevel(int newLevel)
        {
            int ilevel, mins, sb, bb, ante, breakMins;

            // get data for this level
            ilevel = mins = sb = bb = ante = breakMins = 0;
            if (newLevel < gameInfo.bStructure.Count)
            {
                ilevel = newLevel;
                mins = gameInfo.bStructure[ilevel].mins;
                sb = gameInfo.bStructure[ilevel].sb;
                bb = gameInfo.bStructure[ilevel].bb;
                ante = gameInfo.bStructure[ilevel].ante;
                breakMins = gameInfo.bStructure[ilevel].breakMins;
                EBLabel.Text = "Level " + (ilevel+1);
            }
            else
            {
                ilevel = gameInfo.bStructure.Count;
                EBLabel.Text = "NEW Level";
            }

            EBLevelStepper.Minimum = 0;
            EBLevelStepper.Maximum = gameInfo.bStructure.Count;  // always 1 extra level
            EBLevelStepper.Increment = 1;
            EBLevelStepper.Value = ilevel;

            EBDurationValue.Text = mins + " minutes";
            EBDurationStepper.Minimum = 0;
            EBDurationStepper.Maximum = 120;
            EBDurationStepper.Increment = 5;
            EBDurationStepper.Value = mins;

            EBBreakValue.Text = breakMins + " minutes";
            EBBreakStepper.Minimum = 0;
            EBBreakStepper.Maximum = 120;
            EBBreakStepper.Increment = 5;
            EBBreakStepper.Value = breakMins;

            EBAnteValue.Text = "" + ante;
            EBAnteStepper.Minimum = 0;
            EBAnteStepper.Maximum = 1000;
            EBAnteStepper.Increment = 25;
            EBAnteStepper.Value = ante;

            EBBlindValue.Text = sb + "/" + bb;
            EBBlindStepper.Minimum = 0;
            EBBlindStepper.Maximum = 5000;
            EBBlindStepper.Increment = 25;
            EBBlindStepper.Value = sb;

        }
        private void Configure_Blinds(object sender, EventArgs e)
        {
            EB_NewLevel(0);
            ConfigPanel.IsVisible = false;
            EBPanel.IsVisible = true;
        }
        private void EB_LevelChanged(object sender, ValueChangedEventArgs e)
        {
            int newLevel = Convert.ToInt32(e.NewValue);
            EB_NewLevel(newLevel);
        }
        private void EB_BlindsChanged(object sender, ValueChangedEventArgs e)
        {
            int newSB = Convert.ToInt32(e.NewValue);
            EBBlindValue.Text = newSB + "/" + (2* newSB);
        }
        private void EB_DurationChanged(object sender, ValueChangedEventArgs e)
        {
            int newDuration = Convert.ToInt32(e.NewValue);
            EBDurationValue.Text = newDuration + " minutes";
        }
        private void EB_AnteChanged(object sender, ValueChangedEventArgs e)
        {
            int newAnte = Convert.ToInt32(e.NewValue);
            EBAnteValue.Text = "" + newAnte;
        }
        private void EB_BreakChanged(object sender, ValueChangedEventArgs e)
        {
            int newBreak = Convert.ToInt32(e.NewValue);
            EBBreakValue.Text = "" + newBreak + " minutes";
        }

        private void Configure_Exit(object sender, EventArgs e)
        {
            ConfigPanel.IsVisible = false;
            displayMainPanel();
            BottomIconPanel.IsVisible = true;
           
        }
        private void Save_Titles(object sender, EventArgs e)
        {
            ConfigPanel.IsVisible = true;
            TitlePanel.IsVisible = false;
            gameInfo.title1 = Title1Entry.Text;
            gameInfo.title2 = Title2Entry.Text;
        }
        private void Quit_Titles(object sender, EventArgs e)
        {
            ConfigPanel.IsVisible = true;
            TitlePanel.IsVisible = false;
            Title1Entry.Text = gameInfo.title1;
            Title2Entry.Text = gameInfo.title2;
        }
        private void EB_Save(object sender, EventArgs e)
        {
            int ilevel = Convert.ToInt32(EBLevelStepper.Value);
            int mins = Convert.ToInt32(EBDurationStepper.Value);
            int sb = Convert.ToInt32(EBBlindStepper.Value);
            int bb = 2 * sb;
            int breakMins = Convert.ToInt32(EBBreakStepper.Value);
            int ante = Convert.ToInt32(EBAnteStepper.Value);

            if(ilevel < gameInfo.bStructure.Count)
            {
                gameInfo.bStructure[ilevel].ante = ante;
                gameInfo.bStructure[ilevel].sb = sb;
                gameInfo.bStructure[ilevel].bb = bb;
                gameInfo.bStructure[ilevel].breakMins = breakMins; 
                gameInfo.bStructure[ilevel].mins = mins;
            }
            else
            {
                gameInfo.bStructure.Add(new BLevel(mins, sb, bb, ante, breakMins));
            }

            EB_NewLevel(ilevel);
  
        }
        private void EB_Quit(object sender, EventArgs e)
        {
            ConfigPanel.IsVisible = true;
            EBPanel.IsVisible = false;
        }
       


       

        private void Forward_Clicked(object sender, EventArgs e)
        {
            gameInfo.LevelChange(1);
            displayMainPanel();
 
        }

        private void Reverse_Clicked(object sender, EventArgs e)
        {
            gameInfo.LevelChange(-1);
            displayMainPanel();
        }

        private void Blinds_Clicked(object sender, EventArgs e)
        {
            /*
            if (EditBlinds.IsVisible)
                EditBlinds.IsVisible = false;
            else
                EditBlinds.IsVisible = true;
            */
        }

        
        
    }
}
