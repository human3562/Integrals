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
using OxyPlot;
using OxyPlot.Wpf;
using OxyPlot.Series;

namespace WpfApp2 {
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window {
        MainViewModel mvm = new MainViewModel();
        public MainWindow() {
            InitializeComponent();
            this.DataContext = mvm;
            tb_graphingErrors.Text = "";
        }

        private void Grid_KeyDown(object sender, KeyEventArgs e) {
            mvm.keyDown(e.Key);           
        }

        private void cb_functions_selectionChanged(object sender, SelectionChangedEventArgs e) {
            mvm.changeFunction(cb_functions.SelectedIndex);
        }

        private void draw_graph_click(object sender, RoutedEventArgs e) {
            if (!double.TryParse(input_a.Text, out double a) || !double.TryParse(input_b.Text, out double b) || !double.TryParse(input_dx.Text, out double dx)) {
                tb_graphingErrors.Text = "Введены неверные значения.";
                return;
            }
            tb_graphingErrors.Text = "";
            mvm.drawGraph(a, b, dx);
        }

        private void simpson_calculate_click(object sender, RoutedEventArgs e) {
            if (!int.TryParse(input_simpsonDivisions.Text, out int n)) return;
            tb_simpsonResult.Text = mvm.getIntegralOfActiveFunction(n).ToString();
        }

        private void montecarlo_calculate_click(object sender, RoutedEventArgs e) {
            if (!int.TryParse(input_montecarloAmt.Text, out int n)) return;
            tb_montecarloResult.Text = mvm.getMonteCarloOfActiveFunction(n).ToString();
        }
    }
}
