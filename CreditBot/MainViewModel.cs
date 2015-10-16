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
            _betWorker = new BetWorker();
        }

        public event PropertyChangedEventHandler PropertyChanged;
        public BetWorker _betWorker;

        private int _betsPlaced;
        public int BetsPlaced
        {
            get
            {
                return _betsPlaced;
            }
            set
            {
                _betsPlaced = value;
                OnPropertyChanged("BetsPlaced");
            }
        }

        private int _totalPot;
        public int TotalPot
        {
            get
            {
                return _totalPot;
            }
            set
            {
                _totalPot = value;
                OnPropertyChanged("TotalPot");
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

        public bool BotUnmuted
        {
            get
            {
                return !_botMuted;
            }
        }

        protected void OnPropertyChanged(string name)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(name));
            }
        }

        internal void DistributeWinnings(string winnerName)
        {
            
        }

        internal void OpenBetting()
        {
            ErrorMessage = "";

            if (string.IsNullOrEmpty(TeamOneName) || string.IsNullOrEmpty(TeamTwoName))
                ErrorMessage = "Both team names have to be set before opening betting.";

            if(string.IsNullOrEmpty(ErrorMessage))
                _betWorker.Start(TeamOneName, TeamTwoName);
        }

        internal void CloseBetting()
        {
            _betWorker.Stop();
        }
    }
}
