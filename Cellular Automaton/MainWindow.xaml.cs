using Cellular_Automaton.Common;
using Cellular_Automaton.Controllers;
using Cellular_Automaton.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
        #region Properties and ivars
        ////////////////////////////////////////////////////////////

        private AutomatonController _automatonController;

        private ObservableCollection<AutomatonConfiguration> _initialConfigurations = new ObservableCollection<AutomatonConfiguration>();
        private ObservableCollection<Automaton> _automatonModels = new ObservableCollection<Automaton>();


        private Brush CellBrush {
            get {
                SolidColorBrush brush = new SolidColorBrush(AutomatonSettings.defaults.ActiveCellColor);
                brush.Freeze();

                return brush;
            }
        }


        /// <summary>
        /// True if we are dragging across cells in edit mode
        /// </summary>
        private bool _editMode = false;
        public bool EditMode
        {
            get { return _editMode; }
            set { _editMode = value; }
        }


        /// <summary>
        /// Tracks the last cell that the mouse was in
        /// </summary>
        private Cell _lastMouseCell = null;


        #endregion


        ////////////////////////////////////////////////////////////
        #region ui & handlers
        ////////////////////////////////////////////////////////////


        public MainWindow() {
            InitializeComponent();
            this.Title = "Cellular Automaton - Andrzej Frankowski";
        }


        private void applyGridSizeBtn_Click(object sender, RoutedEventArgs e) {
            int x = 0, y = 0;
            bool parsed = false;

            _automatonController.IsPaused = true;
            parsed = (Int32.TryParse(this.xGridSizeTextBox.Text, out x) && Int32.TryParse(this.yGridSizeTextBox.Text, out y));

            bool valid = parsed && (x > 0 && y > 0);
            if (!valid) {
                MessageBox.Show("Only positive integer numbers are acceptable", "Couldn't parse grid size",
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                this.setGridSizeControls();
                return;
            }

            bool[,] workGrid = this._automatonController.CurrentAutomaton.GetCurrentWorkGridConfiguration();
            bool[,] grid = new bool[x,y];
            int xMax = Math.Min(workGrid.GetLength(0), grid.GetLength(0));
            int yMax = Math.Min(workGrid.GetLength(1), grid.GetLength(1));

            for (int i = 0; i < (xMax); i++) {
                for (int j = 0; j < (yMax) ; j++)
                    grid[i, j] = workGrid[i, j];
            }

            AutomatonConfiguration config = new AutomatonConfiguration("", grid);

            if (workGrid.GetLength(0) == x && workGrid.GetLength(1) == y) {
                    this._automatonController.SetNewConfiguration(config);
            } else {
                _automatonController.SetNewConfiguration(config);
                this.NewGame(this._automatonController.CurrentAutomaton);
            }

            this.setGridSizeControls();
        }


        private void loadInitStateBtn_Click(object sender, RoutedEventArgs e) {
            var item = this.initConfigsListBox.SelectedItem;
            if (item == null)
                return;
            Debug.Assert(item is AutomatonConfiguration);
            
            AutomatonConfiguration config = (AutomatonConfiguration)item;
            _automatonController.IsPaused = true;

            if (config.Rows == this._automatonController.CurrentAutomaton.Rows 
                && config.Columns == this._automatonController.CurrentAutomaton.Columns) {
                _automatonController.SetNewConfiguration(config);

            } else {
                _automatonController.SetNewConfiguration(config);
                this.NewGame(this._automatonController.CurrentAutomaton);
            }

            this.setGridSizeControls();
        }

        private void saveInitStateBtn_Click(object sender, RoutedEventArgs e) {
            NamePromptDialog dialog = new NamePromptDialog();
            if (dialog.ShowDialog() == true) {
                bool[,] grid = _automatonController.CurrentAutomaton.GetCurrentWorkGridConfiguration();
                AutomatonConfiguration config = new AutomatonConfiguration(dialog.nameTextBox.Text, grid);
                this._initialConfigurations.Add(config);
            }
        }

        private void deleteInitStateBtn_Click(object sender, RoutedEventArgs e) {
            var item = this.initConfigsListBox.SelectedItem;
            if (item == null)
                return;
            Debug.Assert(item is AutomatonConfiguration);
            AutomatonConfiguration config = (AutomatonConfiguration)item;
            this._initialConfigurations.Remove(config);
        }

        private void loadAutomatonBtn_Click(object sender, RoutedEventArgs e) {
            var item = this.modelsListBox.SelectedItem;
            if (item == null)
                return;
            Debug.Assert(item is Automaton);
            Automaton automaton = (Automaton)item;
            _automatonController.IsPaused = true;
            this.NewGame(automaton);
        }

        private void editAutomatonBtn_Click(object sender, RoutedEventArgs e) {
            var item = this.modelsListBox.SelectedItem;
            int index = this.modelsListBox.SelectedIndex;
            if (item == null)
                return;
            _automatonController.IsPaused = true;
            Debug.Assert(item is Automaton);
            Automaton automaton = (Automaton)item;
            EditAutomatonWindow wnd = new EditAutomatonWindow(automaton);
            if (wnd.ShowDialog() == true) {
                _automatonModels.Remove(automaton);
                _automatonModels.Insert(index, wnd.modifiedAutomaton);
            }
        }


        private void newAutomatonModelBtn_Click(object sender, RoutedEventArgs e) 
        {
            _automatonController.IsPaused = true;
            Automaton automaton = new Automaton(_automatonController.CurrentAutomaton.Rows, _automatonController.CurrentAutomaton.Columns);
            EditAutomatonWindow wnd = new EditAutomatonWindow(automaton);
            if (wnd.ShowDialog() == true) {
                _automatonModels.Add(wnd.modifiedAutomaton);
            }
        }


        private void deleteAutomatonModelBtn_Click(object sender, RoutedEventArgs e) {
            var item = this.modelsListBox.SelectedItem;
            int index = this.modelsListBox.SelectedIndex;
            if (item == null)
                return;
            Debug.Assert(item is Automaton);
            Automaton automaton = (Automaton)item;
            this._automatonModels.Remove(automaton);
        }


        private void Window_Loaded(object sender, RoutedEventArgs e) {
            this.NewGame();
            this.automatonGrid.Background = new SolidColorBrush(AutomatonSettings.defaults.GridBackground);
            this.buildExampleListBoxModels();
        }

        private void editModeCheckBox_Checked(object sender, RoutedEventArgs e)
        {
            _editMode = true;
        }

        private void editModeCheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            _editMode = false;
        }


        private void resumeBtn_Click(object sender, RoutedEventArgs e)
        {
            _automatonController.IsPaused = false;
        }

        private void pauseBtn_Click(object sender, RoutedEventArgs e)
        {
            _automatonController.IsPaused = true;
        }

        private void clearBtn_Click(object sender, RoutedEventArgs e)
        {
            _automatonController.IsPaused = true;
            _automatonController.Reset();
        }


        public void Rect_OnMouseDown(Object sender, MouseButtonEventArgs e)
        {
                Cell cell = ((Rectangle)sender).DataContext as Cell;
                if (cell != null) {
                    _lastMouseCell = cell;
                    cell.Alive = !cell.Alive;
                } else throw (new System.InvalidOperationException("Rect_OnMouseDown"));
        }


        public void Rect_OnMouseEnter(Object sender, MouseEventArgs e)
        {
            if (_editMode) {
                Cell cell = ((Rectangle)sender).DataContext as Cell;
                if (cell != null && cell != _lastMouseCell) {
                    _lastMouseCell = cell;
                    cell.Alive = !cell.Alive;
                } else if (cell == null)
                    throw (new System.InvalidOperationException("Rect_OnMouseEnter"));
            }
        }


        #endregion


        ////////////////////////////////////////////////////////////
        #region Private
        ////////////////////////////////////////////////////////////



        private void buildExampleListBoxModels() {

            ///init configs
            bool[,] grid = new bool[_automatonController.CurrentAutomaton.Rows, _automatonController.CurrentAutomaton.Columns];
            for (int i = 0; i < grid.GetLength(0); i++) {
                for (int j = 0; j < grid.GetLength(1); j++) {
                    grid[i, j] = false;
                }
            }
            AutomatonConfiguration config = new AutomatonConfiguration("Clear config", grid);
            _initialConfigurations.Add(config);

            grid = new bool[_automatonController.CurrentAutomaton.Rows, _automatonController.CurrentAutomaton.Columns];
            for (int i = 0; i < grid.GetLength(0); i++) {
                for (int j = 0; j < grid.GetLength(1); j++) {
                    if ((i + j) % 5 == 0)
                        grid[i, j] = true;
                }
            }

            config = new AutomatonConfiguration("Example config 2", grid);
            _initialConfigurations.Add(config);

            grid = new bool[_automatonController.CurrentAutomaton.Rows, _automatonController.CurrentAutomaton.Columns];
            for (int i = 0; i < grid.GetLength(0); i++) {
                for (int j = 0; j < grid.GetLength(1); j++) {
                    if ((i + j) % 2 == 0)
                        grid[i, j] = true;
                }
            }

            config = new AutomatonConfiguration("Example config 2", grid);
            _initialConfigurations.Add(config);

            grid = new bool[_automatonController.CurrentAutomaton.Rows, _automatonController.CurrentAutomaton.Columns];
            for (int i = 0; i < grid.GetLength(0); i++) {
                for (int j = 0; j < grid.GetLength(1); j++) {
                    if (_random.Next(0, 100)%3 == 0)
                        grid[i, j] = true;
                }
            }

            config = new AutomatonConfiguration("Example config 3", grid);
            _initialConfigurations.Add(config);

            //models

            //m1
            Automaton model = new Automaton();
            model.Name = "Game of life #1";
            model.NeighbourhoodEnvironment = 4;
            model.Rules.Add(new Rule("Life rule #1", model.NeighbourhoodEnvironment, RuleType.Count) );
            _automatonModels.Add(model);

            //m2
            model = new Automaton();
            model.Name = "Matching model #1";
            model.NeighbourhoodEnvironment = 8;
            Rule r = new Rule("Match rule #1", model.NeighbourhoodEnvironment, RuleType.Match);
            r.AllowedNeighbourhood = new bool[9];
            for (int i = 0 ; i < r.AllowedNeighbourhood.GetLength(0) ; i++) {
                if (i % 2 == 0)
                    r.AllowedNeighbourhood[i] = true;
            }
            model.Rules.Add(r);
            
            r = new Rule("Match rule #2", model.NeighbourhoodEnvironment, RuleType.Match);
            r.AllowedNeighbourhood = new bool[9];
            for (int i = 0; i < r.AllowedNeighbourhood.GetLength(0); i++) {
                if (i % 3 == 0)
                    r.AllowedNeighbourhood[i] = true;
            }
            model.Rules.Add(r);

            _automatonModels.Add(model);

            //m3
            model = new Automaton();
            model.Name = "Game of life #2";
            model.NeighbourhoodEnvironment = 8;
            model.Rules.Add(new Rule("Life rule #1", model.NeighbourhoodEnvironment, RuleType.Count));
            _automatonModels.Add(model);

            //m4
            model = new Automaton();
            model.Name = "Game of life #4";
            model.NeighbourhoodEnvironment = 24;
            model.Rules.Add(new Rule("Life rule #1", model.NeighbourhoodEnvironment, RuleType.Count));
            int[] t = new int[2];
            t[0] = 1;
            t[1] = 4;
            model.Rules.First().AllowedAdjecentCount = t;
            _automatonModels.Add(model);
        }

        private Random _random = new Random();


        private void InitUIState()
        {
            InitGrid();
            PopulateGrid();
            ApplyRectStyle(); //flickers?
            setGridSizeControls();

            this.initConfigsListBox.ItemsSource = _initialConfigurations;
            this.modelsListBox.ItemsSource = _automatonModels;

            StatusGenCount.DataContext = _automatonController;
            RunSpeedSlider.DataContext = _automatonController;
            CellBirthCount.DataContext = _automatonController.CurrentAutomaton;
            CellDeathCount.DataContext = _automatonController.CurrentAutomaton;
            PopulationCount.DataContext = _automatonController.CurrentAutomaton;
            PeakPopulationCount.DataContext = _automatonController.CurrentAutomaton;
        }

        private void InitGrid()
        {
            this.automatonGrid.Children.Clear();
            this.automatonGrid.RowDefinitions.Clear();
            this.automatonGrid.ColumnDefinitions.Clear();

            for (int i = 0; i < _automatonController.CurrentAutomaton.Rows; i++)
                this.automatonGrid.RowDefinitions.Add(new RowDefinition());

            for (int i = 0; i < _automatonController.CurrentAutomaton.Columns; i++)
                this.automatonGrid.ColumnDefinitions.Add(new ColumnDefinition());
        }


        private void NewGame()
        {
            if (_automatonController == null)
                _automatonController = new AutomatonController();
            else
                _automatonController.NewModel();

            InitUIState();
            _automatonController.IsPaused = true;
        }

        private void NewGame(Automaton automaton) 
        {
            _automatonController = new AutomatonController(automaton);
            InitUIState();
            _automatonController.IsPaused = true;
        }

        private void ApplyRectStyle()
        {
            Brush cellBrush = this.CellBrush;

            Style style = new Style(typeof(Rectangle), (Style)this.automatonGrid.FindResource(typeof(Rectangle)));
            Setter setter = new Setter();
            setter.Property = Rectangle.FillProperty;
            setter.Value = cellBrush;
            style.Setters.Add(setter);
            this.automatonGrid.Resources.Remove("RectStyle");
            this.automatonGrid.Resources.Add("RectStyle", style);

            UIElementCollection rects = this.automatonGrid.Children;

            foreach (UIElement uie in rects) {
                System.Windows.Style s = (Style)(this.automatonGrid.Resources["RectStyle"]);
                //Rectangle rect = (Rectangle)uie;
                //rect.ClipToBounds = true;
                //rect.Width = 5;
                //rect.Height = 5;
                //rect.Fill = new SolidColorBrush(Colors.Black);
                //rect.Opacity = 1;
                ((Rectangle)uie).Style = s;
            }
        }


        private void PopulateGrid()
        {
            for (int row = 0; row < _automatonController.CurrentAutomaton.Rows; row++) {
                for (int col = 0; col < _automatonController.CurrentAutomaton.Columns; col++) {
                    Rectangle rect = new Rectangle();
                    Grid.SetRow(rect, row);
                    Grid.SetColumn(rect, col);
                    //rect.ClipToBounds = true;
                    //rect.Width = 10;
                    //rect.Height = 10;
                    //rect.Fill = new SolidColorBrush(Colors.Black);
                    //rect.Opacity = 1;
                    this.automatonGrid.Children.Add(rect);
                    rect.DataContext = _automatonController.CurrentAutomaton.CellGrid[row, col];
                    rect.SetBinding(OpacityProperty, "Alive");
                    rect.MouseDown += new MouseButtonEventHandler(Rect_OnMouseDown);
                    rect.MouseMove += new MouseEventHandler(Rect_OnMouseEnter);
                }
            }
        }

        private void setGridSizeControls()
        {
            this.xGridSizeTextBox.Text = "" + this._automatonController.CurrentAutomaton.Rows;
            this.yGridSizeTextBox.Text = "" + this._automatonController.CurrentAutomaton.Columns;
        }

        #endregion

    }
}
