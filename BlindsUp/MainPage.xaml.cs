using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;
using Xamarin.Essentials;
using System.Timers;




namespace BlindsUp
{
    public partial class MainPage : ContentPage
    {
        string blindDataSave = "";
        List<int[]> blindNumbers;
        int numBlindLevels = 0;
        int currentBlindLevel = 0;
        string title1 = "";
        string title2 = "";

        Plugin.SimpleAudioPlayer.ISimpleAudioPlayer shufflePlayer;
        Plugin.SimpleAudioPlayer.ISimpleAudioPlayer levelPlayer;
        Plugin.SimpleAudioPlayer.ISimpleAudioPlayer blindsupPlayer;

        public MainPage( string blindData)
        {
            blindDataSave = blindData;
            InitializeComponent();
            // new text for github
            shufflePlayer = Plugin.SimpleAudioPlayer.CrossSimpleAudioPlayer.CreateSimpleAudioPlayer();
            shufflePlayer.Load("shuffle6.wav");
            levelPlayer = Plugin.SimpleAudioPlayer.CrossSimpleAudioPlayer.CreateSimpleAudioPlayer();
            levelPlayer.Load("endlevel5.wav");
            blindsupPlayer = Plugin.SimpleAudioPlayer.CrossSimpleAudioPlayer.CreateSimpleAudioPlayer();
            blindsupPlayer.Load("blindsup5.wav");

            // extract title and blind info (TODO: put in try/catch block)
            char[] delim = new char[] { ',' };
            string[] bdLines = blindDataSave.Split(delim,3);
            title1 = bdLines[0].Trim();
            title2 = bdLines[1].Trim();
            string[] blindStrings = bdLines[2].Split(delim);
            numBlindLevels = blindStrings.Length;
            blindNumbers = new List<int[]>();
            foreach(string b in blindStrings)
            {
                string bTrimmed = b.Trim();
                char[] delimSpace = new char[] { ' ' };
                string[] fields = bTrimmed.Split(delimSpace);
                int[] fieldNums = new int[fields.Length];
                for (int i = 0; i < fieldNums.Length; i++)
                    fieldNums[i] = Convert.ToInt32(fields[i]);
                blindNumbers.Add(fieldNums);
            }
        }
        private static System.Timers.Timer aTimer;
        private static bool timerHasStarted = false;
        private static int totalSecondsLeft = 0;

        public static string SecondsToMinutes(int seconds, int currentBlindLevel)
        {
            var ts = new TimeSpan(0, 0, seconds);
            if (seconds >= 3600)
            {
                return new DateTime(ts.Ticks).ToString("h:mm:ss");
            }
            else if(seconds >= 0)
                return new DateTime(ts.Ticks).ToString("mm:ss");
            else 
                return  " Completed";
        }

        private void Play_Clicked(object sender, EventArgs e)
        {
            if (!timerHasStarted)
            {

                //BtnDurations.IsVisible = false;
                //EditDurations.IsVisible = false;
                //BtnBlinds.IsVisible = false;
                //EditBlinds.IsVisible = false;

                shufflePlayer.Play();

                // 3+ tokens: pretourney label1, pretourney label2, blind1, blind2...
                currentBlindLevel = 1;
                int[] bnums = blindNumbers[currentBlindLevel - 1];
                string currentBlinds = "" + bnums[1] + " - " + bnums[2];
                if (bnums[3] > 0) currentBlinds += "  +" + bnums[3];
                totalSecondsLeft = 60 * bnums[0];

                Time.Text =  SecondsToMinutes(totalSecondsLeft, currentBlindLevel);
                Time.IsVisible = true;
                Blinds.Text = currentBlinds;
                Blinds.IsVisible = true;
                Level.Text = "Level " + currentBlindLevel;
                Level.IsVisible = true;
                

                aTimer = new System.Timers.Timer(1000);
                // Hook up the Elapsed event for the timer.
                aTimer.Elapsed += OnTimedEvent;
                aTimer.AutoReset = true;
                aTimer.Enabled = true;
                timerHasStarted = true;
            }
            else
            {
                aTimer.Enabled = true;    
                Level.Text = "Level " + currentBlindLevel;
            }

            IBplay.IsVisible = false;
            IBpause.IsVisible = true;

        }

        private void Settings_Clicked(object sender, EventArgs e)
        {
        }
        private void Durations_Clicked(object sender, EventArgs e)
        {/*
            if (EditDurations.IsVisible)
                EditDurations.IsVisible = false;
            else
                EditDurations.IsVisible = true;
            */
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

        private void OnTimedEvent(Object source, ElapsedEventArgs e)
        {
            totalSecondsLeft -= 1;

            if (totalSecondsLeft == 32)
            {
                MainThread.BeginInvokeOnMainThread(() =>
                {
                    levelPlayer.Play();
                });
            }
            else if(totalSecondsLeft == 2)
            {
                MainThread.BeginInvokeOnMainThread(() =>
                {
                    blindsupPlayer.Play();
                });
            }

            if ((totalSecondsLeft == 0) && (currentBlindLevel == numBlindLevels))
            {
                aTimer.Enabled = false;
                MainThread.BeginInvokeOnMainThread(() =>
                {
                    Level.Text = "Level " + currentBlindLevel + " Completed";
                    Time.Text = SecondsToMinutes(totalSecondsLeft, currentBlindLevel);
                    Blinds.Text = blindNumbers[currentBlindLevel - 1][1] + "/" +
                           blindNumbers[currentBlindLevel - 1][2];
                });
            }
            else
            {
                if(totalSecondsLeft == 0)
                {
                    currentBlindLevel += 1;
                    totalSecondsLeft = blindNumbers[currentBlindLevel - 1][0] * 60;
                }
                MainThread.BeginInvokeOnMainThread(() =>
                {
                    Time.Text = SecondsToMinutes(totalSecondsLeft, currentBlindLevel);
                    Level.Text = "Level " + currentBlindLevel;
                    Blinds.Text = blindNumbers[currentBlindLevel - 1][1] + "/" +
                       blindNumbers[currentBlindLevel - 1][2];
                });
            }
        }
        private void Pause_Clicked(object sender, EventArgs e)
        {
            aTimer.Enabled = false;
            IBpause.IsVisible = false;
            IBplay.IsVisible = true;

            Time.Text = SecondsToMinutes(totalSecondsLeft, currentBlindLevel);
            Level.Text = "Level " + currentBlindLevel + "  [PAUSED]" ;
            Blinds.Text = blindNumbers[currentBlindLevel - 1][1] + " / " +
                  blindNumbers[currentBlindLevel - 1][2] ;
        }
    }
}
