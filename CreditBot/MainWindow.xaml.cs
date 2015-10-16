using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace CreditBot
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainViewModel _viewModel;

        public MainWindow()
        {
            _viewModel = new MainViewModel();
            InitializeComponent();

            DataContext = _viewModel;
        }

        private void btnCloseBetting_Click(object sender, RoutedEventArgs e)
        {
            _viewModel.MatchStarted = true;
            _viewModel.CloseBetting();
        }

        private void btnOpenBetting_Click(object sender, RoutedEventArgs e)
        {
            _viewModel.OpenBetting();
        }

        private void btnWinner_Click(object sender, RoutedEventArgs e)
        {
            var btnSender = sender as Button;
            if (btnSender == null)
                return;

            if (btnSender.Name == "btnWinnerOne")
            {
                _viewModel.DistributeWinnings(_viewModel.TeamOneName);
            }
            else if (btnSender.Name == "btnWinnerTwo")
            {
                _viewModel.DistributeWinnings(_viewModel.TeamTwoName);
            }

            _viewModel.MatchStarted = false;
            _viewModel.TeamOneName = "";
            _viewModel.TeamTwoName = "";

        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            _viewModel._betWorker.DisposeThread();
        }

        private void btnMuteBot_Click(object sender, RoutedEventArgs e)
        {
            _viewModel.BotMuted = true;
        }

        private void btnUnmuteBot_Click(object sender, RoutedEventArgs e)
        {
            _viewModel.BotMuted = false;
        }
    }
}
