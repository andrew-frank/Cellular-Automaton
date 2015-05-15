using Cellular_Automaton.Common;
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
    /// Interaction logic for EditRuleWindow.xaml
    /// </summary>
    public partial class EditRuleWindow : Window
    {
        private Rule _modifiedRule;
        public Rule modifiedRule { get { return _modifiedRule; } }

        private int gridWidth {
            get { return RulesExtensions.gridWidthForNeighbourhoodCount(this.modifiedRule.NeighbourhoodCount); }
        }

        private Cell[,] _cellGrid = null;
        public Cell[,] CellGrid
        {
            get {
                if (_cellGrid != null)
                    return _cellGrid;
                else
                    throw (new System.InvalidOperationException("CellGrid Get"));
            }
        }

        private int evaluatedCellIndex {
            get {
                switch (this.modifiedRule.NeighbourhoodCount) {
                    case 4:
                    case 8:
                        return 1;
                    case 24:
                        return 2;
                    default:
                        Debug.Assert(false, "Wrong neighbourhood count");
                        break;
                }
                return -1;
            }
        }

        private bool isCellClickableAtIndex(int row, int col) {
            if(row == this.evaluatedCellIndex && col == this.evaluatedCellIndex)
                return false;

            if (this.modifiedRule.NeighbourhoodCount == 4) {
                if(row == 0 && col == 0)
                    return false;
                if(row == 0 && col == 2)
                    return false;
                if(row == 2 && col == 0)
                    return false;
                if(row == 2 && col == 2)
                    return false;
            }

            return true;
        }

        #region UI handles

        private EditRuleWindow() { }

        public EditRuleWindow(Rule rule) {
            InitializeComponent();
            _modifiedRule = new Rule(rule);

            this.InitGrid();
            this.PopulateGrid();
            this.ApplyRectStyle();

            //bool[,] allowedNeighbourhood = RulesExtensions.neighbourhood1dTo2dArray(this.modifiedRule.NeighbourhoodCount, this.modifiedRule.AllowedNeighbourhood);
            //for (int row = 0; row < this.gridWidth; row++) {
            //    for (int col = 0; col < this.gridWidth; col++) {
            //        _cellGrid[row, col] = new Cell();
            //        _cellGrid[row, col].Alive = allowedNeighbourhood[row, col];
            //        //_cellGrid[row, col].Alive = this.isCellClickableAtIndex(row, col);
            //    }
            //}

            this.ruleTypeLabel.Content = "Rule Type:   (" + rule.NeighbourhoodCount + "-point nehgbourhood)";
            this.nameTextBox.Text = rule.Name;

            switch (rule.Type) {
                case RuleType.Count:
                    this.countRadioBtn.IsChecked = true;
                    this.matchRadioBtn.IsChecked = false;
                    foreach (int c in rule.AllowedAdjecentCount)
                        this.countTextBox.Text += c + ",";
                    this.countTextBox.Text = this.countTextBox.Text.Remove(this.countTextBox.Text.Length - 1);
                    break;
                case RuleType.Match:
                    this.countRadioBtn.IsChecked = false;
                    this.matchRadioBtn.IsChecked = true;
                    break;
                default:
                    break;
            }

            if (rule.RuleResult)
                this.makeAliveCheckBox.IsChecked = true;
            else
                this.makeDeadCheckBox.IsChecked = true;
        }


        private int[] parseAllowedCountString() 
        {
            string[] strings = this.countTextBox.Text.Split(',').ToArray();
            int[] arr = new int[strings.GetLength(0)];

            int i = 0, x = 0;
            bool parsed = false;
            foreach (string str in strings) {
                parsed = Int32.TryParse(str, out x);
                if (!parsed) return null;
                arr[i] = x;
                i++;
            }
            
            return arr;
        }

        private void okBtn_Click(object sender, RoutedEventArgs e) {
            this.modifiedRule.Name = this.nameTextBox.Text;
            if (this.countRadioBtn.IsChecked == true) {
                this.modifiedRule.Type = RuleType.Count;
                int[] allowedCounts = parseAllowedCountString();
                if (allowedCounts == null) {
                    MessageBox.Show("Separate allowed neighbour counts with comma, e.g.: 2,3,4", "Couldn't parse allowed count", 
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }
                this.modifiedRule.AllowedAdjecentCount = allowedCounts;

            } else if(this.matchRadioBtn.IsChecked == true) {
                this.modifiedRule.Type = RuleType.Match;
                this.modifiedRule.AllowedNeighbourhood = RulesExtensions.cellGridTo1dBoolArr(this.modifiedRule.NeighbourhoodCount, this.CellGrid);
            }

            DialogResult = true;
        }


        private void matchRadioBtn_Checked(object sender, RoutedEventArgs e) {
            bool c = this.matchRadioBtn.IsChecked.Value;
            this.countRadioBtn.IsChecked = !c;
            this.countTextBox.IsEnabled = !c;
            if (this.matchGrid == null)
                return;
            if (c) {
                this.gridBorder.IsEnabled = true;
                this.matchGrid.Opacity = 1;
                this.gridBorder.Opacity = 1;
            } else {
                this.matchGrid.Opacity = 0.5;
                this.gridBorder.Opacity = 0.3;
            }
        }

        private void countRadioBtn_Checked(object sender, RoutedEventArgs e) {
            bool c = this.countRadioBtn.IsChecked.Value;
            this.matchRadioBtn.IsChecked = !c;
            this.countTextBox.IsEnabled = c;
            if (this.matchGrid == null)
                return;
            if (c) {
                this.gridBorder.IsEnabled = false;
                this.matchGrid.Opacity = 0.5;
                this.gridBorder.Opacity = 0.3;
            } else {
                this.matchGrid.Opacity = 1;
                this.gridBorder.Opacity = 1;
            }
        }


        public void Rect_OnMouseDown(Object sender, MouseButtonEventArgs e) {
            var element = (UIElement)e.Source;
            int col = Grid.GetColumn(element);
            int row = Grid.GetRow(element);

            Cell cell = ((Rectangle)sender).DataContext as Cell;
            if (cell != null && this.isCellClickableAtIndex(row, col) ) {
                //_lastMouseCell = cell;
                cell.Alive = !cell.Alive;
            } else if( cell == null ) throw (new System.InvalidOperationException("Rect_OnMouseDown"));
        }

        #endregion

        ////////////////////////////////

        #region Private


        private void InitGrid() {

            this.matchGrid.Background = new SolidColorBrush(Colors.White);
            _cellGrid = new Cell[this.gridWidth, this.gridWidth];

            bool[,] allowedNeighbourhood = RulesExtensions.neighbourhood1dTo2dArray(this.modifiedRule.NeighbourhoodCount, this.modifiedRule.AllowedNeighbourhood);
            for (int row = 0; row < this.gridWidth; row++) {
                for (int col = 0; col < this.gridWidth; col++) {
                    _cellGrid[row, col] = new Cell();
                    if(this.isCellClickableAtIndex(row, col))
                        _cellGrid[row, col].Alive = allowedNeighbourhood[row, col];
                }
            }

            this.matchGrid.Children.Clear();
            this.matchGrid.RowDefinitions.Clear();
            this.matchGrid.ColumnDefinitions.Clear();

            for (int i = 0; i <  this.gridWidth ; i++)
                this.matchGrid.RowDefinitions.Add(new RowDefinition());

            for (int i = 0; i < this.gridWidth; i++)
                this.matchGrid.ColumnDefinitions.Add(new ColumnDefinition());
        }


        private void PopulateGrid() {
            for (int row = 0; row < this.gridWidth; row++) {
                for (int col = 0; col < this.gridWidth ; col++) {
                    Rectangle rect = new Rectangle();
                    Grid.SetRow(rect, row);
                    Grid.SetColumn(rect, col);
                    //rect.ClipToBounds = true;
                    //rect.Width = 10;
                    //rect.Height = 10;
                    //rect.Fill = new SolidColorBrush(Colors.Black);
                    //rect.Opacity = 1;
                    rect.Margin = new System.Windows.Thickness(0.5);
                    this.matchGrid.Children.Add(rect);
                    rect.DataContext = this.CellGrid[row, col];
                    rect.SetBinding(OpacityProperty, "Alive");
                    rect.MouseDown += new MouseButtonEventHandler(Rect_OnMouseDown);
                }
            }
        }

        
        private void ApplyRectStyle() {
            SolidColorBrush brush = new SolidColorBrush(Colors.Black);
            brush.Freeze();
            Brush cellBrush = brush;

            Style style = new Style(typeof(Rectangle), (Style)this.matchGrid.FindResource(typeof(Rectangle)));
            Setter setter = new Setter();
            setter.Property = Rectangle.FillProperty;
            setter.Value = cellBrush;
            style.Setters.Add(setter);
            this.matchGrid.Resources.Remove("RectStyle");
            this.matchGrid.Resources.Add("RectStyle", style);

            UIElementCollection rects = this.matchGrid.Children;

            foreach (UIElement uie in rects) {
                System.Windows.Style s = (Style)(this.matchGrid.Resources["RectStyle"]);
                //Rectangle rect = (Rectangle)uie;
                //rect.ClipToBounds = true;
                //rect.Width = 5;
                //rect.Height = 5;
                //rect.Fill = new SolidColorBrush(Colors.Black);
                //rect.Opacity = 1;
                ((Rectangle)uie).Style = s;
            }
        }

        #endregion

        private void makeAliveCheckBox_Checked(object sender, RoutedEventArgs e) {
            if (this.modifiedRule == null)
                return;

            this.modifiedRule.RuleResult = true;
        }

        private void makeDeadCheckBox_Checked(object sender, RoutedEventArgs e) {
            if (this.modifiedRule == null)
                return;

            this.modifiedRule.RuleResult = false;
        }
    }
}
