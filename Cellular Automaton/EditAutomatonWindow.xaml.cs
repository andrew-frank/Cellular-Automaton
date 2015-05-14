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
        public Automaton modifiedAutomaton { get; private set; }
        public Automaton originalAutomaton { get; private set; }

        public EditAutomatonWindow() {
            InitializeComponent();
            this.modifiedAutomaton = new Automaton();
        }

        public EditAutomatonWindow(Automaton original) {
            InitializeComponent();
            this.modifiedAutomaton = new Automaton(original);
            this.originalAutomaton = original;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e) {
            //nameTextBox.DataContext = this.automaton;
            this.nameTextBox.Text = this.modifiedAutomaton.Name;

            if (modifiedAutomaton.NeighbourhoodEnvironment == 4)
                this.fourCheckBox.IsChecked = true;
            else if (modifiedAutomaton.NeighbourhoodEnvironment == 8)
                this.eightCheckBox.IsChecked = true;
            else if (modifiedAutomaton.NeighbourhoodEnvironment == 24)
                this.twentyfourCheckBox.IsChecked = true;
            else
                Debug.Assert(false, "Wrong neighbourhood");

            this.rulesListBox.ItemsSource = modifiedAutomaton.Rules;

            switch (this.modifiedAutomaton.LogicalOperator) {
                case RulesLogicalOperator.Conjunction:
                    this.conjunctionRadioBtn.IsChecked = true;
                    this.disjunctionRadioBtn.IsChecked = false;
                    this.altDenialRadioBtn.IsChecked = false;
                    this.exlDisRadioBtn.IsChecked = false;
                    break;
                case RulesLogicalOperator.Disjunction:
                    this.conjunctionRadioBtn.IsChecked = false;
                    this.disjunctionRadioBtn.IsChecked = true;
                    this.altDenialRadioBtn.IsChecked = false;
                    this.exlDisRadioBtn.IsChecked = false;
                    break;
                case RulesLogicalOperator.AlternativeDenial:
                    this.conjunctionRadioBtn.IsChecked = false;
                    this.disjunctionRadioBtn.IsChecked = false;
                    this.altDenialRadioBtn.IsChecked = true;
                    this.exlDisRadioBtn.IsChecked = false;
                    break;
                case RulesLogicalOperator.ExclusiveDisjunction:
                    this.conjunctionRadioBtn.IsChecked = false;
                    this.disjunctionRadioBtn.IsChecked = false;
                    this.altDenialRadioBtn.IsChecked = false;
                    this.exlDisRadioBtn.IsChecked = true;
                    break;
                default:
                    Debug.Assert(false, "Wrong logical operator");
                    break;
            }
        }



        private void saveBtn_Click(object sender, RoutedEventArgs e) {
            DialogResult = true;
            //this.automaton = this.automaton;
        }

        private void discardBtn_Click(object sender, RoutedEventArgs e) {
            DialogResult = false;
        }

        private void deleteRuleBtn_Click(object sender, RoutedEventArgs e) {
            if (this.rulesListBox.SelectedIndex < 0)
                return;
            this.modifiedAutomaton.Rules.RemoveAt(this.rulesListBox.SelectedIndex);
        }

        private void newRuleBtn_Click(object sender, RoutedEventArgs e) {
            EditRuleWindow wnd = new EditRuleWindow(new Rule(null, this.modifiedAutomaton.NeighbourhoodEnvironment, RuleType.Count));
            wnd.ShowDialog();
            if (wnd.DialogResult == true) {
                this.modifiedAutomaton.Rules.Add(wnd.modifiedRule);
            }
        }

        private void editRuleBtn_Click(object sender, RoutedEventArgs e) {
            Rule rule = (Rule)this.rulesListBox.SelectedItem;
            if (rule == null)
                return;
            int index = this.rulesListBox.SelectedIndex;
            EditRuleWindow wnd = new EditRuleWindow(rule);
            wnd.ShowDialog();
            if (wnd.DialogResult == true) {
                this.modifiedAutomaton.Rules.Remove(rule);
                this.modifiedAutomaton.Rules.Insert(index, wnd.modifiedRule);
            }
        }

        private void fourCheckBox_Checked(object sender, RoutedEventArgs e) {
            if (this.modifiedAutomaton.NeighbourhoodEnvironment == 4)
                return;
            this.modifiedAutomaton.NeighbourhoodEnvironment = 4;
            this.modifiedAutomaton.Rules.Clear();
        }

        private void eightCheckBox_Checked(object sender, RoutedEventArgs e) {
            if (this.modifiedAutomaton.NeighbourhoodEnvironment == 8)
                return;
            this.modifiedAutomaton.NeighbourhoodEnvironment = 8;
            this.modifiedAutomaton.Rules.Clear();
        }

        private void twentyfourCheckBox_Checked(object sender, RoutedEventArgs e) {
            if (this.modifiedAutomaton.NeighbourhoodEnvironment == 24)
                return;
            this.modifiedAutomaton.NeighbourhoodEnvironment = 24;
            this.modifiedAutomaton.Rules.Clear();
        }

        private void conjunctionRadioBtn_Checked(object sender, RoutedEventArgs e) {
            if (this.modifiedAutomaton == null)
                return;
            this.modifiedAutomaton.LogicalOperator = RulesLogicalOperator.Conjunction;
        }

        private void disjunctionRadioBtn_Checked(object sender, RoutedEventArgs e) {
            if (this.modifiedAutomaton == null)
                return;
            this.modifiedAutomaton.LogicalOperator = RulesLogicalOperator.Disjunction;
        }

        private void exlDisRadioBtn_Checked(object sender, RoutedEventArgs e) {
            if (this.modifiedAutomaton == null)
                return;
            this.modifiedAutomaton.LogicalOperator = RulesLogicalOperator.ExclusiveDisjunction;
        }

        private void altDenialRadioBtn_Checked(object sender, RoutedEventArgs e) {
            if (this.modifiedAutomaton == null)
                return;
            this.modifiedAutomaton.LogicalOperator = RulesLogicalOperator.AlternativeDenial;
        }
    }
}
