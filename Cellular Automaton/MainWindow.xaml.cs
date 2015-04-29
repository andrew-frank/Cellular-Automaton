using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Cellular_Automaton
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        ////////////////////////////////////////////////////////////

        #region view lifecycle

        public MainWindow()
        {
            InitializeComponent();
            this.Title = "Cellular Automaton - Andrzej Frankowski";
        }

        private void WindowLoaded(object sender, RoutedEventArgs e)
        {
            //for(int i = 0 ; i < )
        }

        #endregion

        ////////////////////////////////////////////////////////////

        #region handlers

        private void automatonGrid_MouseDown(object sender, MouseButtonEventArgs e)
        {

        }

        #endregion


    }
}
