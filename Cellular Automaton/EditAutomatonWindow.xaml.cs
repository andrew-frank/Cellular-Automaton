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
using System.Windows.Shapes;

namespace Cellular_Automaton
{
    /// <summary>
    /// Interaction logic for EditAutomatonWindow.xaml
    /// </summary>
    public partial class EditAutomatonWindow : Window
    {
        public EditAutomatonWindow() {
            InitializeComponent();
        }

        private void saveBtn_Click(object sender, RoutedEventArgs e) {
            DialogResult = true;
        }

        private void discardBtn_Click(object sender, RoutedEventArgs e) {
            DialogResult = false;
        }

        private void RadioButton_Checked(object sender, RoutedEventArgs e) {

        }

        private void RadioButton_Checked_1(object sender, RoutedEventArgs e) {

        }

        private void RadioButton_Checked_2(object sender, RoutedEventArgs e) {

        }

        private void deleteRuleBtn_Click(object sender, RoutedEventArgs e) {

        }

        private void newRuleBtn_Click(object sender, RoutedEventArgs e) {

        }
    }
}
