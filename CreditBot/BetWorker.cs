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
        private string _server = "irc.quakenet.org";
        private string _botName = "CreditBot";
        private int _port = 6667;
        private string _channel = "#dopefish_lives";
        List<Bet> _bets;
        List<User> _users;
        Thread _listenThread;
        int _defaultStartValue = 100;
        private string _teamOne = "";
        private string _teamTwo = "";
        private bool _muted = false;

        public BetWorker()
        {
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

            //Channel channel = _ircClient.GetChannel(_channel);
            //foreach (ChannelUser user in channel.Users.Values)
            //{
            //    User dbUser = DataManager.GetUser(user.Nick);
            //    if (dbUser != null)
            //    {
            //        _users.Add(dbUser);
            //    }
            //    else
            //    {
            //        _users.Add(new User(user.Nick, _defaultStartValue));
            //    }
            //}
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
            var userObj = _users.Where(u => u.UserName == user);
        }

        private void _ircClient_OnJoin(object sender, JoinEventArgs e)
        {
            if(e.Who != _botName)
            {
                _users.Add(new User(e.Who, _defaultStartValue));
            }
            else
            {

            }
        }

        public bool BettingOpen { get; set; }

        internal void Start(string teamOne, string teamTwo)
        {
            _teamOne = teamOne;
            _teamTwo = teamTwo;
            BettingOpen = true;
            SendMessage(string.Format("Betting open for: {0} vs {1} !", teamOne, teamTwo));
        }

        internal void Muted(bool mute)
        {
            _muted = true;
            SendMessage("Bot Muted.", true);
        }

        private void SendMessage(string message, bool ignoreMute = false)
        {
            if(!_muted || ignoreMute)
                _ircClient.SendMessage(SendType.Message, _channel, message);
        }

        internal void Stop()
        {
            BettingOpen = false;
        }


        private void _ircClient_OnChannelMessage(object sender, IrcEventArgs e)
        {
            string message = e.Data.Message;
            User user = GetUser(e.Data.Nick);

            if (message.StartsWith("!bet"))
            {
                if(!BettingOpen)
                {
                    SendMessage("Betting is currently not open.");
                }

                string[] split = message.Split(' ');
                if (split.Count() != 3)
                {
                    SendMessage("Invalid Bet. !bet ### teamName");
                    return;
                }

                int value = 0;
                bool success = int.TryParse(split[1], out value);

                if (!success)
                {
                    SendMessage("Invalid Bet. !bet ### teamName");
                    return;
                }

                string teamName = split[2];

                _bets.Add(new Bet() { User = user, BetValue = value, Team = teamName });
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
