using Cellular_Automaton.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
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
        public Automaton automaton { get; private set; }


        public EditAutomatonWindow() {
            InitializeComponent();
            this.automaton = new Automaton();
        }

        public EditAutomatonWindow(Automaton original) {
            InitializeComponent();
            this.automaton = original;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e) {
            //nameTextBox.DataContext = this.automaton;
            this.nameTextBox.Text = this.automaton.Name;

            if (automaton.NeighbourhoodEnvironment == 4)
                this.fourCheckBox.IsChecked = true;
            else if (automaton.NeighbourhoodEnvironment == 8)
                this.eightCheckBox.IsChecked = true;
            else if (automaton.NeighbourhoodEnvironment == 24)
                this.twentyfourCheckBox.IsChecked = true;
            else
                Debug.Assert(false, "Wrong neighbourhood");

            this.rulesListBox.ItemsSource = automaton.Rules;
        }



        private void saveBtn_Click(object sender, RoutedEventArgs e) {
            DialogResult = true;
            this.automaton = this.automaton;
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
            this.automaton.Rules.RemoveAt(this.rulesListBox.SelectedIndex);
        }

        private void newRuleBtn_Click(object sender, RoutedEventArgs e) {
            EditRuleWindow wnd = new EditRuleWindow();
            wnd.ShowDialog();
        }

        private void editRuleBtn_Click(object sender, RoutedEventArgs e) {
            EditRuleWindow wnd = new EditRuleWindow();
            wnd.ShowDialog();
        }

    }
}
