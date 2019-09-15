using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace bub
{
    /// <summary>
    /// Interaction logic for winMain.xaml
    /// </summary>
    public partial class winMain : Window
    {
        public winMain()
        {
            InitializeComponent();
        }

        private void btnExit_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }

        private void btnNew_Click(object sender, RoutedEventArgs e)
        {
            Bubbler.NewGame();
        }

        private void btnUndo_Click(object sender, RoutedEventArgs e)
        {
            Bubbler.Undo();
        }

        private void btnHighscores_Click(object sender, RoutedEventArgs e)
        {
            Bubbler.ShowHighscores();
        }
    }
}
