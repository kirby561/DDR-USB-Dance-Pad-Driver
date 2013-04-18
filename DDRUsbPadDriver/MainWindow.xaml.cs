using System;
using System.Collections.Generic;
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

namespace DDRUsbPadDriver {
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window {
        private DancePadDriver _driver;

        private SolidColorBrush _pressedBrush = new SolidColorBrush(Colors.DarkRed);
        private SolidColorBrush _upBrush = new SolidColorBrush(Colors.DarkBlue);

        public MainWindow() {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e) {
            _driver = new DancePadDriver();
            _driver.Initialize(OnPortStateChanged);
            foreach (String port in _driver.GetPorts())
                cmbPorts.Items.Add(port);

            cmbPorts.SelectedValue = _driver.GetPortName();

            OnPortStateChanged(_driver.GetStates());
        }

        private void UpdateButtonBackground(Button button, int state) {
            if (state == 0)
                button.Background = _upBrush;
            else
                button.Background = _pressedBrush;
        }

        private void OnPortStateChanged(int[] states) {
            // Update all of our button's backgrounds to show which
            //    buttons on the gamepad are being pressed.
            UpdateButtonBackground(_btnUp, states[0]);
            UpdateButtonBackground(_btnDown, states[1]);
            UpdateButtonBackground(_btnLeft, states[2]);
            UpdateButtonBackground(_btnRight, states[3]);
            UpdateButtonBackground(_btnEscape, states[4]);
            UpdateButtonBackground(_btnEnter, states[5]);
        }

        private void cmbPorts_SelectionChanged(object sender, SelectionChangedEventArgs e) {
            _driver.ChangePort(cmbPorts.SelectedItem.ToString());
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e) {
            // Shut down our driver
            _driver.Stop();
        }
    }
}
