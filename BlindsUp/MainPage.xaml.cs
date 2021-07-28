using System;
using System.Collections.Generic;
using System.Timers;
using Xamarin.Essentials;
using Xamarin.Forms;


public enum PlActions
{
    PL_BUYIN = 0,
    PL_ASSIGN_POSITIONS = 1,
    PL_REBUY = 2,
    PL_KNOCKOUT = 3,
    PL_ICM_CHOP = 4,
}

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
        List<personInfo> peopleInfo;

        // contains index of player seated at the position -1=noone
        int[] playerAtPosTemp = new int[] { -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 };
        int playerIndexOfButton = -1;

        List<Label> seatLabels;
        List<Layout> prizeLayouts;
        List<Label> prizeLabels;
        List<Stepper> prizeSteppers;

        System.Timers.Timer aTimer = null;

        // must match PlActions Enum
        public static string[] plActionStrings =
        {
        "Player Buy-In",
        "Assign Positions",
        "Rebuy/Addon",
        "Record KO",
        "ICM Chop"
        };

        public static string[] rankStrings =
            {"1st","2nd","3rd","4th","5th"};

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
            peopleInfo = new List<personInfo>();
            displayMainPanel();

            // store these xaml built controls in Lists so we can manipulate more easily
            seatLabels = new List<Label>();
            seatLabels.Add(P1);
            seatLabels.Add(P2);
            seatLabels.Add(P3);
            seatLabels.Add(P4);
            seatLabels.Add(P5);
            seatLabels.Add(P6);
            seatLabels.Add(P7);
            seatLabels.Add(P8);
            seatLabels.Add(P9);
            seatLabels.Add(P10);

            prizeLayouts = new List<Layout>();
            prizeLayouts.Add(Layout_p1);
            prizeLayouts.Add(Layout_p2);
            prizeLayouts.Add(Layout_p3);
            prizeLayouts.Add(Layout_p4);
            prizeLayouts.Add(Layout_p5);
            prizeLabels = new List<Label>();
            prizeLabels.Add(Label_p1);
            prizeLabels.Add(Label_p2);
            prizeLabels.Add(Label_p3);
            prizeLabels.Add(Label_p4);
            prizeLabels.Add(Label_p5);
            prizeSteppers = new List<Stepper>();
            prizeSteppers.Add(Stepper_p1);
            prizeSteppers.Add(Stepper_p2);
            prizeSteppers.Add(Stepper_p3);
            prizeSteppers.Add(Stepper_p4);
            prizeSteppers.Add(Stepper_p5);
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

            else if (gameInfo.currentState == GameState.GS_BREAK)
            {
                gameInfo.secondsLeftBreak -= 1;
                if (gameInfo.secondsLeftBreak == 2)
                {
                    MainThread.BeginInvokeOnMainThread(() =>
                    {
                        blindsupPlayer.Play();
                    });
                }
                else if (gameInfo.secondsLeftBreak == 0)
                {
                    gameInfo.BreakExpired();
                }
            }


            MainThread.BeginInvokeOnMainThread(() =>
            {
                displayMainPanel();

            });

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

        private void People_Click(object sender, EventArgs e)
        {
            PLPicker.ItemsSource = null;
            PLPicker.ItemsSource = plActionStrings;
            MainPanel.IsVisible = false;
            PeoplePanel.IsVisible = true;
            BottomIconPanel.IsVisible = false;
        }
        private void PL_Quit(object sender, EventArgs e)
        {
            PeoplePanel.IsVisible = false;
            MainPanel.IsVisible = true;
            BottomIconPanel.IsVisible = true;
            PlName.Text = "";
            PlBuyIn.Text = "";
            PlBuyInPanel.IsVisible = false;
            PlRebuyPanel.IsVisible = false;
            PlKOPanel.IsVisible = false;
            PlAssignTablePanel.IsVisible = false;
            PLNotification.Text = "";
        }

        private void updateActiveLabel()
        {
            if (getNumSeated() == 0)
                PLActive.Text = getNumActive() + " Entrants";
            else if (gameInfo.currentState == GameState.GS_BUYIN)
            {
                // count the number of players seated
                PLActive.Text = getNumSeated() + " of " + getNumActive() + " Seated";
            }
            else if ((gameInfo.currentState == GameState.GS_RUNNING) || (gameInfo.currentState == GameState.GS_BREAK))
            {
                PLActive.Text = getNumActive() + " of " + getNumSeated() + " Alive";
            }
        }
        private void PL_Save(object sender, EventArgs e)
        {
            // case 1: was a buy-in
            if (PLPicker.SelectedIndex == (int)PlActions.PL_BUYIN)
            {
                // validate buy-in and name
                string nam = PlName.Text;

                double bi = 0.0;
                bool isValidNumeric = true;
                try
                {
                    bi = Convert.ToDouble(PlBuyIn.Text);
                    if (bi < .25)
                        isValidNumeric = false;
                }
                catch (Exception e1)
                {
                    e1.ToString();
                    isValidNumeric = false;
                }
                if (!isValidNumeric)
                {
                    PLNotification.Text = "Invalid Buy-In Amount";
                }
                else if (PlName.Text.Length == 0)
                {
                    PLNotification.Text = "Must enter player name";
                }
                else
                {
                    peopleInfo.Add(new personInfo(PlName.Text, bi));
                    PLNotification.Text = "Buy-In successful for " + PlName.Text;
                    updateActiveLabel();
                    PlName.Text = "";
                    PlBuyIn.Text = "";
                }
            }

            // case 2: assign players
            else if (PLPicker.SelectedIndex == (int)PlActions.PL_ASSIGN_POSITIONS)
            {
                PLNotification.Text = "Impossible state";
            }

            // case 3: rebuy/addon
            else if (PLPicker.SelectedIndex == (int)PlActions.PL_REBUY)
            {
                // get name of player selected and find his index
                // add amount of buyin to personInfo, and set isActive=true

                // validate rebuy amount
                double rebi = 0.0;
                bool isValidEntry = true;
                try
                {
                    rebi = Convert.ToDouble(PlBuyIn.Text);
                    if (rebi < .25)
                        isValidEntry = false;
                }
                catch (Exception e1)
                {
                    e1.ToString();
                    isValidEntry = false;
                }
                int playerIndex = PLRebuyer.SelectedIndex;

                if (!isValidEntry)
                {
                    // get rid of invalid amount
                    PLNotification.Text = "Invalid Buy-In Amount";
                }
                else if (playerIndex < 0)
                {
                    PLNotification.Text = "Must select player name";
                }
                else
                {
                    PlSaveButton.IsVisible = false;
                    PlAssignButton.IsVisible = false;
                    peopleInfo[playerIndex].totalFees += rebi;
                    peopleInfo[playerIndex].chipBuys += 1;
                    peopleInfo[playerIndex].isActive = true;

                    PLNotification.Text = "Rebuy successful for " + peopleInfo[playerIndex].name;

                    updateActiveLabel();

                    PlRebuyAmount.Text = "";
                    PLRebuyer.ItemsSource = null;
                    PLRebuyer.ItemsSource = getPlayerNames();
                }
            }
            else if (PLPicker.SelectedIndex == (int)PlActions.PL_KNOCKOUT)
            {
                int koVictim = PLKnockee.SelectedIndex;
                int koPerp = PLKnocker.SelectedIndex;
                if ((koVictim < 0) || (koPerp < 0))
                {
                    PLNotification.Text = "Please select BOTH victim and perp";
                }
                else if (koVictim == koPerp)
                {
                    PLNotification.Text = "Please select DIFFERENT victim and perp";
                }
                else
                {
                    peopleInfo[koVictim].isActive = false;
                    peopleInfo[koVictim].bustOuts += 1;
                    peopleInfo[koVictim].bustOutOrder = getNumBustOuts();

                    // todo: see if any prizes were won and record them [need prize structure]

                    peopleInfo[koPerp].knockOuts += 1;
                    peopleInfo[koPerp].koVictims.Add(koVictim);

                    updateActiveLabel();
                    PLNotification.Text = "Recorded KO of " + peopleInfo[koVictim].name + " by " +
                        peopleInfo[koPerp].name;

                    // reset pickers
                    PLKnockee.ItemsSource = null;
                    PLKnocker.ItemsSource = null;
                    PLKnockee.ItemsSource = getActiveNames();
                    PLKnocker.ItemsSource = getActiveNames();
                }
            }
        }

        private void PL_AssignSeats(object sender, EventArgs e)
        {
            Random rgen = new Random();

            // this function stores player indices in playerAtPosTemp and displays

            // no seat assignments saved yet
            if (getNumSeated() == 0)
            {
                for (int i = 0; i < playerAtPosTemp.Length; i++)
                    playerAtPosTemp[i] = -1;

                for (int i = 0; i < peopleInfo.Count; i++)
                {
                    bool foundOpenSeat = false;
                    while (!foundOpenSeat)
                    {
                        int pos = rgen.Next() % playerAtPosTemp.Length;
                        if (playerAtPosTemp[pos] < 0)
                        {
                            playerAtPosTemp[pos] = i;
                            peopleInfo[i].tablePos = pos;
                            foundOpenSeat = true;
                        }
                    }
                }

                // generate which playIndex has button
                Random r2 = new Random();
                playerIndexOfButton = r2.Next() % peopleInfo.Count;
            }
            else
            {
                // some seats already assigned, assign the new ones
                for (int i = 0; i < playerAtPosTemp.Length; i++)
                    playerAtPosTemp[i] = -1;

                // buttonPos does not change

                for (int i = 0; i < peopleInfo.Count; i++)
                {
                    if (peopleInfo[i].tablePos >= 0)
                    {
                        playerAtPosTemp[peopleInfo[i].tablePos] = i;
                    }
                }

                // find temp positions for the unseated players
                for (int i = 0; i < peopleInfo.Count; i++)
                {
                    if (peopleInfo[i].tablePos < 0)
                    {
                        {
                            bool foundOpenSeat = false;
                            while (!foundOpenSeat)
                            {
                                int pos = rgen.Next() % playerAtPosTemp.Length;
                                if (playerAtPosTemp[pos] < 0)
                                {
                                    playerAtPosTemp[pos] = i;
                                    peopleInfo[i].tablePos = pos;
                                    foundOpenSeat = true;
                                }
                            }
                        }

                    }
                }
            }

            // now walk the seats at the table and fill labels to show seating
            int numLabelsLoaded = 0;
            foreach (int playerIndex in playerAtPosTemp)
            {
                if (playerIndex >= 0)
                {
                    // there is a player in this seat
                    string s = (1 + numLabelsLoaded) + ". " + peopleInfo[playerIndex].name;
                    if (playerIndex == playerIndexOfButton)
                        s += " [BTN]";
                    seatLabels[numLabelsLoaded].Text = s;
                    seatLabels[numLabelsLoaded].IsVisible = true;
                    numLabelsLoaded += 1;
                }
            }
            PL_Heading.Text = "Assigned Positions";
            PlSaveButton.IsVisible = false;
            PlAssignButton.IsVisible = false;
            updateActiveLabel();
            PLNotification.Text = "Seats assigned successfully";
        }

        private void PL_ActionChanged(object sender, EventArgs e)
        {
            // when action changes, hide every thing before showing what we need
            PLNotification.Text = "";
            PlName.Text = "";
            PlBuyIn.Text = "";
            PlBuyInPanel.IsVisible = false;
            PlAssignTablePanel.IsVisible = false;
            PlKOPanel.IsVisible = false;
            PlRebuyPanel.IsVisible = false;
            PlSaveButton.IsVisible = false;
            PlAssignButton.IsVisible = false;

            if (PLPicker.SelectedIndex == (int)PlActions.PL_BUYIN)
            {
                PlSaveButton.IsVisible = true;
                PlBuyInPanel.IsVisible = true;

            }
            else if (PLPicker.SelectedIndex == (int)PlActions.PL_KNOCKOUT)
            {
                PLKnockee.ItemsSource = null;
                PLKnocker.ItemsSource = null;
                PLKnockee.ItemsSource = getActiveNames();
                PLKnocker.ItemsSource = getActiveNames();
                PlSaveButton.IsVisible = true;
                PlKOPanel.IsVisible = true;
            }
            else if (PLPicker.SelectedIndex == (int)PlActions.PL_ICM_CHOP)
            {

            }
            else if (PLPicker.SelectedIndex == (int)PlActions.PL_REBUY)
            {
                PlRebuyPanel.IsVisible = true;
                PLRebuyer.ItemsSource = null;
                PLRebuyer.ItemsSource = getPlayerNames();
                PlRebuyAmount.Text = "";
                PlSaveButton.IsVisible = true;
            }
            else if (PLPicker.SelectedIndex == (int)PlActions.PL_ASSIGN_POSITIONS)
            {
                PlAssignTablePanel.IsVisible = true;

                // hide player labels
                foreach (Label w in seatLabels)
                    w.IsVisible = false;

                // if nobody has been seated, just show current players
                if (getNumSeated() == 0)
                {
                    PL_Heading.Text = "Current Players";
                    if (peopleInfo.Count == 0)
                    {
                        P1.Text = "None";
                        P1.IsVisible = true;
                    }
                    else
                    {
                        for (int i = 0; i < peopleInfo.Count; i++) {
                            seatLabels[i].Text = peopleInfo[i].name;
                            seatLabels[i].IsVisible = true;
                        }
                        PlAssignButton.IsVisible = true;
                    }
                }
                else
                {
                    // seats have been assigned -- show current assignments

                    // first, load assignments back into "temp" from peopleInfo,
                    // which has the last "committed" player seat assignments
                    for (int i = 0; i < playerAtPosTemp.Length; i++)
                        playerAtPosTemp[i] = -1;

                    for (int i = 0; i < peopleInfo.Count; i++)
                    {
                        if (peopleInfo[i].tablePos >= 0)
                        {
                            playerAtPosTemp[peopleInfo[i].tablePos] = i;
                        }
                    }

                    // now write the info assigned seats to the labels
                    int numLabelsLoaded = 0;
                    for (int i = 0; i < playerAtPosTemp.Length; i++)
                    {
                        int playerIndex = playerAtPosTemp[i];
                        if (playerIndex >= 0)
                        {
                            string s = (1 + numLabelsLoaded) + ". " + peopleInfo[playerIndex].name;
                            if (playerIndex == playerIndexOfButton)
                                s += " [BTN]";
                            seatLabels[numLabelsLoaded].Text = s;
                            seatLabels[numLabelsLoaded].IsVisible = true;
                            numLabelsLoaded += 1;
                        }
                    }
                    // write the unassigned players
                    for (int i = 0; i < peopleInfo.Count; i++)
                    {
                        if (peopleInfo[i].tablePos < 0)
                        {
                            string s = peopleInfo[i].name + " [UNSEATED]";
                            seatLabels[numLabelsLoaded].Text = s;
                            seatLabels[numLabelsLoaded].IsVisible = true;
                            numLabelsLoaded += 1;
                        }
                    }

                    PL_Heading.Text = "Assigned Seats";

                    // only allow Reassign if new players have come
                    if (getNumSeated() < getNumActive())
                    {
                        PlAssignButton.Text = "Assign+";
                        PlAssignButton.IsVisible = true;
                    }
                }
            }
            updateActiveLabel();
        }

        private int getNumSeated()
        {
            int n = 0;
            foreach (personInfo p in peopleInfo)
            {
                if (p.tablePos >= 0)
                    n += 1;
            }
            return n;
        }
        private int getNumActive()
        {
            int n = 0;
            foreach (personInfo p in peopleInfo)
            {
                if (p.isActive)
                    n += 1;
            }
            return n;
        }
        private int getNumBustOuts()
        {
            int n = 0;
            foreach (personInfo p in peopleInfo)
            {
                n += p.bustOuts;
            }
            return n;
        }
        private List<string> getPlayerNames()
        {
            List<string> allPlayers = new List<string>();
            foreach (personInfo p in peopleInfo)
                allPlayers.Add(p.name);
            return allPlayers;
        }

        private int IndexOfStepper( Stepper stepperObj)
        {
            for(int i=0;i<prizeSteppers.Count;i++)
            {
                if (prizeSteppers[i] == stepperObj)
                    return i;
            }
            return -1;
        }
        private List<string> getActiveNames()
        {
            List<string> allPlayers = new List<string>();
            foreach (personInfo p in peopleInfo)
                if (p.isActive)
                    allPlayers.Add(p.name);
            return allPlayers;
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

        private void Configure_Prizes(object sender, EventArgs e)
        {
            ConfigPanel.IsVisible = false;
            int numPrizes = gameInfo.prizePct.Count;
            TopPrizesLabel.Text = "Top " + numPrizes;
            TopPrizesStepper.Value = numPrizes;
            for (int i = 0; i < prizeLayouts.Count; i++)
            {
                if (i < numPrizes)
                {
                    prizeSteppers[i].Value = gameInfo.prizePct[i];
                    prizeLabels[i].Text = rankStrings[i] + ":" + " " +
                        gameInfo.prizePct[i] + "%";
                    prizeLayouts[i].IsVisible = true;
                }
                else
                {
                    prizeLayouts[i].IsVisible = false;
                    prizeLabels[i].Text = " ";
                    prizeSteppers[i].Value = 0;
                }
            }
            if (gameInfo.bountyPoolPct <= 0)
            {
                KOCheckBox.IsChecked = false;
                KOPoolLayout.IsVisible = false;
                KOPoolLabel.Text = "";
                KOPoolStepper.Value = 0;
            }
            else
            {
                KOCheckBox.IsChecked = true;
                KOPoolLayout.IsVisible = true;
                KOPoolLabel.Text = "KO Pool: " + gameInfo.bountyPoolPct + "%";
                KOPoolStepper.Value = gameInfo.bountyPoolPct;
            }
            PrizesPanel.IsVisible = true;
        }

        private void NumPrizes_Changed(object sender, ValueChangedEventArgs e)
        {
            int newVal = (int)e.NewValue;
            int oldVal = (int)e.OldValue;
            PrizeNotifyLabel.Text = "";

            TopPrizesLabel.Text = "Top" + (int)e.NewValue;
            
            for(int i=0;i<prizeLayouts.Count;i++)
            {
                if(i < newVal)
                {
                    if (!prizeLayouts[i].IsVisible)
                    {
                        prizeSteppers[i].Value = 0;
                        prizeLabels[i].Text = rankStrings[i] + ":" + " " +
                         prizeSteppers[i].Value + "%";

                        prizeLayouts[i].IsVisible = true;
                    }
                }
                else
                {
                    prizeLayouts[i].IsVisible = false;
                }
            }

        }
        private void Save_Prizes(object sender, EventArgs e)
        {
            // make sure everything adds up to 100%
            int pctSum = 0;
            int numPrizes = (int)TopPrizesStepper.Value;
            for(int i=0;i<numPrizes;i++)
            {
                pctSum += (int)prizeSteppers[i].Value;
            }
            if(KOCheckBox.IsChecked)
            {
                pctSum += (int)KOPoolStepper.Value;
            }
            if(pctSum != 100)
            {
                PrizeNotifyLabel.Text = "Prizes must sum to 100%";
            }
            else
            {
                if(KOCheckBox.IsChecked)
                {
                    gameInfo.bountyPoolPct = (int)KOPoolStepper.Value;
                }
                gameInfo.prizePct.Clear();
                for (int i = 0; i < numPrizes; i++)
                    gameInfo.prizePct.Add((int)prizeSteppers[i].Value);
                PrizeNotifyLabel.Text = "Prize structure updated";
            }
        }
        private void Quit_Prizes(object sender, EventArgs e)
        {
            ConfigPanel.IsVisible = true;
            PrizesPanel.IsVisible = false;
        }

        private void Prize_Changed(object sender, ValueChangedEventArgs e)
        {
            int sendIndex = IndexOfStepper((Stepper)sender);
            PrizeNotifyLabel.Text = "";
            if (sendIndex >= 0)
            {
                prizeLabels[sendIndex].Text = rankStrings[sendIndex] + ":" + " " +
                        (int)e.NewValue + "%";
            }
            else if(KOPoolStepper == (Stepper)sender)
            {
                KOPoolLabel.Text = "KO Pool: " + (int)e.NewValue + "%";
            }
        }
        private void KOCheckBox_Changed(object sender, EventArgs e)
        {
            PrizeNotifyLabel.Text = "";

            if(KOCheckBox.IsChecked) {
                KOPoolLayout.IsVisible = true;
                KOPoolLabel.Text = "KO Pool: " + gameInfo.bountyPoolPct + "%";
                KOPoolStepper.Value = gameInfo.bountyPoolPct;
            }
            else {
                KOPoolLayout.IsVisible = false;
                KOPoolLabel.Text = "";
                KOPoolStepper.Value = gameInfo.bountyPoolPct;
            }
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
