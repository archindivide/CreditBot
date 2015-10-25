using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;

namespace CreditBot
{
    public class MainViewModel : INotifyPropertyChanged
    {
        public MainViewModel()
        {
            _betWorker = new BetWorker(this);
        }

        public event PropertyChangedEventHandler PropertyChanged;
        public BetWorker _betWorker;

        public int BetsPlaced
        {
            get
            {
                return _betWorker.GetBetCount();
            }
        }

        public int TotalPot
        {
            get
            {
                return _betWorker.GetBetTotal();
            }
        }

        private bool _matchStarted;
        public bool MatchStarted
        {
            get
            {
                return _matchStarted;
            }
            set
            {
                _matchStarted = value;
                OnPropertyChanged("MatchStarted");
                OnPropertyChanged("NotMatchStarted");
            }
        }

        public bool NotMatchStarted
        {
            get
            {
                return !MatchStarted;
            }
        }

        private string _teamOneName;
        public string TeamOneName
        {
            get
            {
                return _teamOneName;
            }
            set
            {
                _teamOneName = value;
                OnPropertyChanged("TeamOneName");
            }
        }

        private string _teamTwoName;
        public string TeamTwoName
        {
            get
            {
                return _teamTwoName;
            }
            set
            {
                _teamTwoName = value;
                OnPropertyChanged("TeamTwoName");
            }
        }

        private string _errorMessage;
        public string ErrorMessage
        {
            get
            {
                return _errorMessage;
            }
            set
            {
                _errorMessage = value;
                OnPropertyChanged("ErrorMessage");
            }
        }

        private bool _botMuted = false;
        public bool BotMuted
        {
            get
            {
                return _botMuted;
            }
            set
            {
                _botMuted = value;
                OnPropertyChanged("BotMuted");
                OnPropertyChanged("BotUnmuted");
                _betWorker.Muted(value);
            }
        }

        public string GiveUserName { get; set; }
        public string GiveAmount { get; set; }

        internal void ExecuteGive()
        {
            ErrorMessage = "";
            if(string.IsNullOrEmpty(GiveUserName) || string.IsNullOrEmpty(GiveAmount))
                ErrorMessage = "You must define a username and an amount to give credits.";

            if (string.IsNullOrEmpty(ErrorMessage))
                ErrorMessage = _betWorker.Give(GiveUserName, GiveAmount);
        }

        public bool BotUnmuted
        {
            get
            {
                return !_botMuted;
            }
        }

        public void OnPropertyChanged(string name)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(name));
            }
        }

        internal void DistributeWinnings(string winnerName)
        {
            _betWorker.DistributeWinnings(winnerName);
        }

        internal void OpenBetting()
        {
            if(!_betWorker.BettingOpen)
            {
                ErrorMessage = "";

                if (string.IsNullOrEmpty(TeamOneName) || string.IsNullOrEmpty(TeamTwoName))
                    ErrorMessage = "Both team names have to be set before opening betting.";

                if (string.IsNullOrEmpty(ErrorMessage))
                    _betWorker.Start(TeamOneName, TeamTwoName);
            }
        }

        internal void CloseBetting()
        {
            _betWorker.Stop();
        }
    }
}
