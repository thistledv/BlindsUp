using System;
using System.Collections.Generic;
using System.Text;

namespace BlindsUp
{

    public enum GameState
    {
        GS_BUYIN,
        GS_RUNNING,
        GS_BREAK,
        GS_DONE
    }

    class personInfo
    {
        public string name;
        public int chipBuys;
        public int bustOuts;
        public int bustOutOrder; // used to determine finish
        public int knockOuts;
        public double totalFees;
        public double koPrize;
        public double itmPrize;
        public List<int>koVictims;
        public bool isActive;
        public int tablePos;

        public personInfo (string _name, double _buyIn)
        {
            name = _name;
            chipBuys = 1;
            isActive = true;
            koVictims = new List<int>();
            totalFees = _buyIn;
            koPrize = 0.0;
            itmPrize = 0.0;
            bustOuts = 0;
            knockOuts = 0;
            tablePos = -1;
            bustOutOrder = 0;
        }
    }
    
    class BLevel
    {
        public int mins;  // minutes in this level
        public int sb;
        public int bb;
        public int ante;
        public int breakMins;  // minutes of breaktime after this level

        public BLevel ( int _mins, int _sb, int _bb, int _ante, int _breakMins)
        {
            mins = _mins; sb = _sb; bb = _bb; ante = _ante; breakMins = _breakMins;
        }
    }
    class GameInfo
    {
        public string title1;
        public string title2;
        public string currentBlindString;
        public int currentBlindLevel;
        public int secondsLeftLevel;
        public int secondsLeftBreak;
        public GameState currentState;
        public List<BLevel> bStructure;
        public bool isPaused;
        public List<int> prizePct;
        public double bounty;

        public GameInfo( string dataString)
        {
            title1 = title2 = currentBlindString = "";
            secondsLeftLevel = secondsLeftBreak = 0;
            currentBlindLevel = -1;
            currentState = GameState.GS_BUYIN;
            bStructure = new List<BLevel>();
            isPaused = false;
            prizePct = new List<int>();
            prizePct.Add(60);
            prizePct.Add(40);
            bounty = 0.0;

            string[] lines = dataString.Split(new char[] { ',' });
            for (int i = 0; i < lines.Length; i += 2)
            {
                if (lines[i].StartsWith("TITLE1") && ((i + 1) < lines.Length))
                    title1 = lines[i + 1].Trim();
                else if (lines[i].StartsWith("TITLE2") && ((i + 1) < lines.Length))
                    title2 = lines[i + 1].Trim();
                else if (lines[i].StartsWith("LEVEL") && ((i+1) < lines.Length))
                {
                    string[] fields = lines[i + 1].Trim().Split(new char[] { ' ' });
                    if (fields.Length == 5)
                    {
                        bStructure.Add(new BLevel(
                            Convert.ToInt32(fields[0]),
                            Convert.ToInt32(fields[1]),
                            Convert.ToInt32(fields[2]),
                            Convert.ToInt32(fields[3]),
                            Convert.ToInt32(fields[4])));

                    }
                    else
                    {
                        bStructure.Add(new BLevel(0, 0, 0, 0, 0));
                    }
                }
            }
        }

        
        public string LevelString()
        {
            if (currentState == GameState.GS_BUYIN)
                return "Starting Soon";
            else if (currentState == GameState.GS_BREAK)
            {
                if (isPaused)
                    return "Break [PAUSED]";
                else
                    return "Break";
            }
            else if (currentState == GameState.GS_RUNNING)
            {
                if (isPaused)
                    return "Level " + (currentBlindLevel + 1) + " [PAUSED]";
                else
                    return "Level " + (currentBlindLevel + 1);
            }
            else
                return "Completed";
        }

        public string BlindString()
        {
            return currentBlindString;
        }

        public string ClockString ()
        {
            int seconds = 0;
            if (currentState == GameState.GS_RUNNING)
                seconds = secondsLeftLevel;
            else if (currentState == GameState.GS_BREAK)
                seconds = secondsLeftBreak;

            var ts = new TimeSpan(0, 0, seconds);
            if (seconds >= 3600)
            {
                return new DateTime(ts.Ticks).ToString("h:mm:ss");
            }
            else if (seconds >= 0)
                return new DateTime(ts.Ticks).ToString("mm:ss");
            else
                return " Completed";
        }

        public void StartPlay()
        {
            currentState = GameState.GS_RUNNING;
            currentBlindLevel = 0;
            secondsLeftLevel = bStructure[currentBlindLevel].mins * 60;
            secondsLeftBreak = bStructure[currentBlindLevel].breakMins * 60;
            currentBlindString = bStructure[currentBlindLevel].sb + "/" + bStructure[currentBlindLevel].bb;
            if (bStructure[currentBlindLevel].ante > 0)
                currentBlindString += " +" + bStructure[currentBlindLevel].ante; 
        }

        public void LevelChange(int direction)
        {
            
            if(direction < 0)
            {
                // try to go back a level
                if(currentBlindLevel <= 0)
                {
                    currentBlindLevel = -1;
                    currentState = GameState.GS_BUYIN;
                    secondsLeftLevel = 0;
                    secondsLeftBreak = 0;
                    isPaused = false;
                }
                else
                {
                    currentBlindLevel -= 1;
                    currentState = GameState.GS_RUNNING;
                    secondsLeftLevel = bStructure[currentBlindLevel].mins * 60;
                    secondsLeftBreak = bStructure[currentBlindLevel].breakMins * 60;
                    currentBlindString = bStructure[currentBlindLevel].sb + "/" + bStructure[currentBlindLevel].bb;
                    if (bStructure[currentBlindLevel].ante > 0)
                        currentBlindString += " +" + bStructure[currentBlindLevel].ante;
                }
            }
            else if(direction > 0)
            {
                // try to advance a level
                if(currentBlindLevel < 0)
                {
                    currentBlindLevel = 0;
                    currentState = GameState.GS_RUNNING;
                    secondsLeftLevel = bStructure[currentBlindLevel].mins * 60;
                    secondsLeftBreak = bStructure[currentBlindLevel].breakMins * 60;
                    currentBlindString = bStructure[currentBlindLevel].sb + "/" + bStructure[currentBlindLevel].bb;
                    if (bStructure[currentBlindLevel].ante > 0)
                        currentBlindString += " +" + bStructure[currentBlindLevel].ante;
                }
                else if(currentBlindLevel < (bStructure.Count - 1))
                {
                    currentBlindLevel += 1;
                    currentState = GameState.GS_RUNNING;
                    secondsLeftLevel = bStructure[currentBlindLevel].mins * 60;
                    secondsLeftBreak = bStructure[currentBlindLevel].breakMins * 60;
                    currentBlindString = bStructure[currentBlindLevel].sb + "/" + bStructure[currentBlindLevel].bb;
                    if (bStructure[currentBlindLevel].ante > 0)
                        currentBlindString += " +" + bStructure[currentBlindLevel].ante;
                }
            }
        }
        public void LevelExpired()
        {
            // gets called when level second timer gets to 0

            // this level followed by break, change state and show next level blinds
            if (secondsLeftBreak > 0)
            {
                currentState = GameState.GS_BREAK;
                // show next level blinds, if there is a next level
                if (currentBlindLevel < (bStructure.Count - 1))
                {
                    currentBlindString = "Next: " +
                        bStructure[currentBlindLevel+1].sb + "/" + bStructure[currentBlindLevel + 1].bb;
                    if (bStructure[currentBlindLevel+1].ante > 0)
                        currentBlindString += " +" + bStructure[currentBlindLevel + 1].ante;
                }
            }

            // no break, advance to next blind level
            else if (currentBlindLevel < (bStructure.Count - 1))
            {
                currentBlindLevel += 1;
                secondsLeftLevel = bStructure[currentBlindLevel].mins * 60;
                secondsLeftBreak = bStructure[currentBlindLevel].breakMins * 60;
                currentBlindString = bStructure[currentBlindLevel].sb + "/" + bStructure[currentBlindLevel].bb;
                if (bStructure[currentBlindLevel].ante > 0)
                    currentBlindString += " +" + bStructure[currentBlindLevel].ante;
            }

            else
            {
                currentState = GameState.GS_DONE;
            }
        }

        public void BreakExpired()
        {
            // called when break timer gets to 0
            if(currentBlindLevel == (bStructure.Count - 1))
            {
                // we just finished last level
                currentState = GameState.GS_DONE;
            }
            else
            {
                // advance to next blind level
                currentState = GameState.GS_RUNNING;
                currentBlindLevel += 1;
                secondsLeftLevel = bStructure[currentBlindLevel].mins * 60;
                secondsLeftBreak = bStructure[currentBlindLevel].breakMins * 60;
                currentBlindString = bStructure[currentBlindLevel].sb + "/" + bStructure[currentBlindLevel].bb;
                if (bStructure[currentBlindLevel].ante > 0)
                    currentBlindString += " +" + bStructure[currentBlindLevel].ante;
            }
        }
    } // end class
}
