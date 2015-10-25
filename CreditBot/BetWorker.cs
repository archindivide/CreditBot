using Meebey.SmartIrc4net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;

namespace CreditBot
{
    public class BetWorker
    {
        IrcClient _ircClient;
        private string _server = Config.Default.IRCServer;
        private string _botName = Config.Default.BotName;
        private int _port = Config.Default.IRCPort;
        private string _channel = Config.Default.IRCChannel;
        List<Bet> _bets;
        List<User> _users;
        Thread _listenThread;
        int _defaultStartValue = Config.Default.DefaultStartValue;
        private string _teamOne = "";
        private string _teamTwo = "";
        private bool _muted = false;
        private bool _gettingUsers = false;
        private MainViewModel _viewModel;

        public BetWorker(MainViewModel viewModel)
        {
            _viewModel = viewModel;
            _bets = new List<Bet>();
            _users = new List<User>();

            _ircClient = new IrcClient();
            _ircClient.Connect(_server, _port);
            _ircClient.WriteLine(Rfc2812.Nick(_botName), Priority.Critical);
            _ircClient.WriteLine(Rfc2812.User(_botName, 0, _botName), Priority.Critical);
            _ircClient.WriteLine(Rfc2812.Join(_channel));
            _listenThread = new Thread(() => 
            {
                _ircClient.OnChannelMessage += _ircClient_OnChannelMessage;
                _ircClient.OnJoin += _ircClient_OnJoin;
                _ircClient.OnPart += _ircClient_OnPart;
                _ircClient.OnQuit += _ircClient_OnQuit;
                _ircClient.Listen();
            });
            _listenThread.Start();
        }

        private void _ircClient_OnQuit(object sender, QuitEventArgs e)
        {
            UserQuit(e.Who);
        }

        private void _ircClient_OnPart(object sender, PartEventArgs e)
        {
            UserQuit(e.Who);
        }

        private void UserQuit(string user)
        {
            var userObj = _users.Where(u => u.UserName == user).FirstOrDefault();
            if(userObj != null)
            {
                _users.Remove(userObj);
                DataManager.SaveUserData(userObj);
            }
        }

        private void _ircClient_OnJoin(object sender, JoinEventArgs e)
        {
            if(e.Who != _botName)
            {
                User existingUser = GetUser(e.Who);
                if (existingUser != null)
                    _users.Add(existingUser);
                else
                    _users.Add(new User(e.Who, _defaultStartValue));
            }
        }

        public void SaveAllUserData()
        {
            foreach(User u in _users.ToList())
            {
                _users.Remove(u);
                DataManager.SaveUserData(u);
            }
        }

        public bool BettingOpen { get; set; }

        public int GetBetCount()
        {
            return _bets.Count;
        }

        public int GetBetTotal()
        {
            return _bets.Select(b => b.BetValue).Sum();
        }

        internal void Start(string teamOne, string teamTwo)
        {
            _teamOne = teamOne;
            _teamTwo = teamTwo;
            BettingOpen = true;
            SendMessage(string.Format("Betting open for: {0} vs {1} !", teamOne, teamTwo));
        }

        internal void DistributeWinnings(string winnerName)
        {
            Dictionary<User, int> winners = new Dictionary<User, int>();
            int winnerNum = _bets.Where(b => b.Team == winnerName).Select(b => b.BetValue).Sum();
            int loserNum = _bets.Where(b => b.Team != winnerName).Select(b => b.BetValue).Sum();

            if (winnerNum == 0)
                winnerNum = 1;

            if (loserNum == 0)
                loserNum = 1;

            double winnerOdds = (double)loserNum / winnerNum;

            foreach (var bet in _bets)
            {
                if (bet.Team == winnerName)
                {
                    var user = _users.First(u => u.UserName == bet.User.UserName);
                    int winnings = bet.BetValue + (int)(bet.BetValue * winnerOdds);
                    user.Value += winnings;
                    winners.Add(user, winnings);
                }
            }

            _bets.Clear();
            _viewModel.OnPropertyChanged("BetsPlaced");
            _viewModel.OnPropertyChanged("TotalPot");

            StringBuilder winnerString = new StringBuilder();
            winnerString.Append(winnerName + " wins! ");
            if(winners.Any())
            {
                winnerString.Append("Top Winners: ");
                int counter = 1;

                foreach (var winner in winners.OrderByDescending(w => w.Value).Take(5))
                {
                    winnerString.Append(counter);
                    winnerString.Append(". ");
                    winnerString.Append(winner.Key.UserName);
                    winnerString.Append(" - ");
                    winnerString.Append(winner.Value);
                    winnerString.Append(", ");
                }

                winnerString.Remove(winnerString.Length - 2, 2);
            }
            else
            {
                winnerString.Append("No winning bets placed.");
            }
            

            SendMessage(winnerString.ToString());
        }

        internal void Muted(bool mute)
        {
            _muted = mute;
            if (_muted)
                SendMessage("Bot Muted.", true);
            else
                SendMessage("Bot Unmuted.");
        }

        private void SendMessage(string message, bool ignoreMute = false)
        {
            if(!_muted || ignoreMute)
                _ircClient.SendMessage(SendType.Message, _channel, message);
        }

        internal void Stop()
        {
            BettingOpen = false;
            SendMessage(string.Format("Betting closed for: {0} vs {1} .", _teamOne, _teamTwo));
            _teamOne = "";
            _teamTwo = "";
        }

        private void _ircClient_OnChannelMessage(object sender, IrcEventArgs e)
        {
            string message = e.Data.Message;
            User user = GetUser(e.Data.Nick);
            if(user == null)
            {
                user = new User(e.Data.Nick, _defaultStartValue);
                _users.Add(user);
            }

            if (message.StartsWith("!bet"))
            {
                if (!BettingOpen)
                {
                    SendMessage(user.UserName + ": Betting is not open.");
                    return;
                }

                string[] split = message.Split(' ');
                if (split.Count() != 3)
                {
                    SendMessage(user.UserName + ": Invalid Bet. !bet ### <team name>");
                    return;
                }

                int value = 0;
                bool success = int.TryParse(split[1], out value);

                if (!success)
                {
                    SendMessage(user.UserName + ": Invalid Bet. !bet ### <team name>");
                    return;
                }

                if(value > user.Value)
                {
                    SendMessage(user.UserName + ": Cannot bet more than you have.");
                    return;
                }

                string teamName = split[2];
                if (teamName != _teamOne && teamName != _teamTwo)
                {
                    SendMessage(user.UserName + ": Invalid Bet. !bet ### <team name>");
                    return;
                }

                _bets.Add(new Bet() { User = user, BetValue = value, Team = teamName });
                user.Value -= value;
                _viewModel.OnPropertyChanged("BetsPlaced");
                _viewModel.OnPropertyChanged("TotalPot");
            }

            if (message.StartsWith("!credits"))
            {
                SendMessage(user.UserName + ": " + user.Value + " credits.");
            }
        }

        private User GetUser(string userName)
        {
            User user = _users.Where(u => u.UserName == userName).FirstOrDefault();

            if(user == null)
            {
                user = DataManager.GetUser(userName);
            }

            return user;
        }

        public void DisposeThread()
        {
            _ircClient.Disconnect();
            _listenThread.Abort();
        }
    }
}
