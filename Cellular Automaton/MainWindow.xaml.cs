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



        private void loadInitStateBtn_Click(object sender, RoutedEventArgs e) {
            var item = this.initConfigsListBox.SelectedItem;
            if (item == null)
                return;
            Debug.Assert(item is AutomatonConfiguration);
            AutomatonConfiguration config = (AutomatonConfiguration)item;
            _automatonController.IsPaused = true;
            _automatonController.SetNewConfiguration(config);
        }

        private void saveInitStateBtn_Click(object sender, RoutedEventArgs e) {
            NamePromptDialog dialog = new NamePromptDialog();
            if (dialog.ShowDialog() == true) {
                bool[,] grid = _automatonController.CurrentAutomaton.GetCurrentWorkGridConfiguration();
                AutomatonConfiguration config = new AutomatonConfiguration(dialog.nameTextBox.Text, grid);
                this._initialConfigurations.Add(config);
            }
        }

        private void loadAutomatonBtn_Click(object sender, RoutedEventArgs e) {
            var item = this.initConfigsListBox.SelectedItem;
            if (item == null)
                return;
            Debug.Assert(item is Automaton);
            Automaton automaton = (Automaton)item;
            _automatonController.IsPaused = true;
            this.NewGame(automaton);
        }

        private void editAutomatonBtn_Click(object sender, RoutedEventArgs e) {
            EditAutomatonWindow wnd = new EditAutomatonWindow();
            if (wnd.ShowDialog() == true) {

            }
        }



        private void Window_Loaded(object sender, RoutedEventArgs e) {
            this.NewGame();
            this.automatonGrid.Background = new SolidColorBrush(AutomatonSettings.defaults.GridBackground);
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

        /// <summary>
        /// Rect_OnMouseDown(Object, MouseButtonEventArgs)
        /// 
        /// Handles the mouse down event on a Rectangle in the grid, and if the game is
        /// in the paused state (editable) flips the cell state. Since we allow dragging
        /// _lastMouseCell is used to avoid the effects of repeated MouseEnter events
        /// getting fired.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void Rect_OnMouseDown(Object sender, MouseButtonEventArgs e)
        {
            //if (_automatonController.IsPaused) { //&& (_automatonController.Generation == 0)
                Cell cell = ((Rectangle)sender).DataContext as Cell;
                if (cell != null) {
                    _lastMouseCell = cell;
                    cell.Alive = !cell.Alive;
                    //UIStateChange(UIStateChanges.ModelCellEdited);
                } else throw (new System.InvalidOperationException("Rect_OnMouseDown"));
            //}
        }


        /// <summary>
        /// Rect_OnMouseEnter(Object, MouseEventArgs)
        /// 
        /// Handles the mouseover event for an Ellipse. If the game is paused and the mouse
        /// is entering a new cell, flip that cell's state.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
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
            bool[,] grid = new bool[_automatonController.CurrentAutomaton.Rows, _automatonController.CurrentAutomaton.Columns];
            for (int i = 0; i < grid.GetLength(0); i++) {
                for (int j = 0; j < grid.GetLength(1); j++) {
                    if ((i + j) % 3 == 0)
                        grid[i, j] = true;
                }
            }
            AutomatonConfiguration config = new AutomatonConfiguration("Example config 1", grid);
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
        }



        /// <summary>
        /// InitUIState()
        /// 
        /// This function peforms common setup work when a game is created or loaded.
        /// Initializes the grid and populates it, sets up some data contexts, and sets
        /// the window title.
        /// </summary>
        private void InitUIState()
        {
            InitGrid();
            PopulateGrid();
            ApplyRectStyle(); //flickers?
            //SetGridSizeMenu();

            this.buildExampleListBoxModels();
            this.initConfigsListBox.ItemsSource = _initialConfigurations;

            StatusGenCount.DataContext = _automatonController;
            RunSpeedSlider.DataContext = _automatonController;
            CellBirthCount.DataContext = _automatonController.CurrentAutomaton;
            CellDeathCount.DataContext = _automatonController.CurrentAutomaton;
            PopulationCount.DataContext = _automatonController.CurrentAutomaton;
            PeakPopulationCount.DataContext = _automatonController.CurrentAutomaton;
        }


        /// <summary>
        /// InitGrid()
        /// 
        /// Called from the OnLoaded event handler for the main window to initialize the
        /// UI display grid with the appropriate number of rows and columns based on the
        /// _lm.Rows and _lm.Columns properties. It then adds an ellipse to each cell and
        /// sets its style.
        /// </summary>
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


        /// <summary>
        /// NewGame()
        /// 
        /// Does the grunt work of initializing a new game with an empty grid. Initializes
        /// the model and controller, populates the grid, and wires up some UI fields by setting
        /// data contexts for items with property bindings.
        /// </summary>
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

        /// <summary>
        /// ApplyRectStyle()
        /// 
        /// If you look at the source you'll see that this function is only called when a new
        /// grid is being initialized, and when the cell brush has been changed. It builds a new
        /// style that sets the Fill property of a rectangle to the new brush, and then goes
        /// through the children of the grid setting this style. The style is based on an
        /// existing style that binds the opacity property to govern visibility. So why go to
        /// all this trouble to change fill brushes? Why not just set the fill property on the
        /// rects and be done with it? Here's the issue: again, opacity is controlled by a
        /// binding in a style. Element properties override style settings, and they happen at
        /// different times too. If in the process of creating a new grid I set the rectangle
        /// DataContext to point to a LifeCell, which will drive the opacity binding, and then
        /// set the Fill property directly, sometimes, depending on timing, I get a repaint
        /// before the opacity property is correctly set, and the grid renders all the cells
        /// visible. It looks messy, and I don't want the grid repainted until the state of all
        /// the cells is correct. I'm sure there must be other ways to handle suppressing the
        /// repaint, but the issue there is that the flash happens after I return control to
        /// the message pump. If I somehow surpress the repaint when will I unsurpress it? The
        /// best way around this that I have found so far is to do as I have below: change
        /// brushes by building a new style and then applying that style.
        /// </summary>
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


        /// <summary>
        /// PopulateGrid()
        /// 
        /// Does the work of setting up the rectangles in the cells of the life grid. Creates
        /// the rectangles and assigns them to the grid, adds them to the child collection,
        /// sets up rectangle data contexts for the rect->cell link, sets the rectangle style,
        /// and wires up the rectangle mouse events
        /// </summary>
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


        /// <summary>
        /// SetGridSizeMenu()
        /// 
        /// Called from InitUIState to update the state of the grid size menu
        /// to reflect the size of a model.
        /// </summary>
        private void SetGridSizeMenu()
        {
            //MenuGridSizeText.Text = "r:" + _automatonController.CurrentAutomaton.Rows.ToString() + " c:" + _automatonController.CurrentAutomaton.Columns.ToString();
        }

        #endregion
    }
}
