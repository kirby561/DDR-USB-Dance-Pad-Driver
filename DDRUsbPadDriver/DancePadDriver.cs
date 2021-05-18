using System;
using System.IO.Ports;
using KeyStrokeEmulator;
using System.Windows;
using System.Threading;

namespace DDRUsbPadDriver {
    public delegate void PortDataChangedEventHandler(int[] states);

    class DancePadDriver {
        private String[] _ports;
        private SerialPort _port;
        private PortDataChangedEventHandler _portStateChangedCallback = null;
        private Mutex _portClosingMutex = new Mutex();

        // The state of each of the keys
        private int[] _states = new int[] { 0, 0, 0, 0, 0, 0 };

        // The mapping of state indices to key codes.
        private KeyboardEmulator.KeyCode[] _keyCodes = new KeyboardEmulator.KeyCode[] { 
            KeyboardEmulator.KeyCode.UP,
            KeyboardEmulator.KeyCode.DOWN,
            KeyboardEmulator.KeyCode.LEFT,
            KeyboardEmulator.KeyCode.RIGHT,
            KeyboardEmulator.KeyCode.ESC,
            KeyboardEmulator.KeyCode.ENTER 
        };

        /// <summary>
        /// Initializes the driver with the given callback for state updates.
        /// The driver automatically picks the last COM port to start reading data to.
        /// Use ChangePort to switch to a different one.
        /// </summary>
        /// <param name="onPortDataChangedEventHandler">The event handler to call when the list of buttons down changes.</param>
        public void Initialize(PortDataChangedEventHandler onPortDataChangedEventHandler) {
            _ports = SerialPort.GetPortNames();

            _portStateChangedCallback = onPortDataChangedEventHandler;
        }

        /// <summary>
        /// Gets the current state of the buttons of the dance pad.
        /// </summary>
        /// <returns>The current state of the buttons of the dance pad.</returns>
        public int[] GetStates() {
            return _states;
        }

        /// <summary>
        /// Gets the available serial ports.
        /// </summary>
        /// <returns>The available serial ports.</returns>
        public String[] GetPorts() {
            return _ports;
        }

        /// <summary>
        /// Gets the name of the currently selected port.
        /// </summary>
        /// <returns>The name of the currently selected port.</returns>
        public String GetPortName() {
            if (_port == null)
                return "";
            return _port.PortName;
        }

        /// <summary>
        /// Called whenever data is receieved from the current port.
        /// </summary>
        /// <param name="sender">The sender of the event</param>
        /// <param name="e">Parameters associated with the event.</param>
        private void onPortDataReceived(object sender, SerialDataReceivedEventArgs e) {
            string line = "";
            bool success = false;
            while (!success) {
                try {
                    line = _port.ReadLine();
                    success = true;
                } catch (Exception ex) {
                    Console.WriteLine("Error reading from the port.  Closing. Exception: " + ex.Message);
                    _port.Close(); 
                    break;
                }
            }
           
            // Get the buttons that are down
            String[] buttonStates = line.Split(new char[] { '\0', '\r' }, StringSplitOptions.RemoveEmptyEntries);
            if (buttonStates.Length == 6) {
                // Iterate thru and get each button state
                for (int stateIndex = 0; stateIndex < buttonStates.Length; stateIndex++) {
                    String buttonState = buttonStates[stateIndex];
                    int state = -1;
                    bool valid = Int32.TryParse(buttonState, out state);
                    if (state == 0 && state != _states[stateIndex]) { // Down
                        KeyboardEmulator.SendKeyUp(_keyCodes[stateIndex]);
                        _states[stateIndex] = state;
                        Console.WriteLine("SendKeyUp, key = " + stateIndex);
                    } else if (state == 1 && state != _states[stateIndex]) { // Up
                        KeyboardEmulator.SendKeyDown(_keyCodes[stateIndex]);
                        _states[stateIndex] = state;
                        Console.WriteLine("SendKeyDown, key = " + stateIndex);
                    } else if (state != 1 && state != 0) {
                        valid = false;
                    }

                    if (!valid)
                        Console.WriteLine("Error: Invalid state " + buttonState);
                }

                // Let the UI update its buttons
                if (_portStateChangedCallback != null)
                    Application.Current.Dispatcher.Invoke(_portStateChangedCallback, GetStatesCopy());
            } else {
                Console.WriteLine("Error: Invalid number of entries in the split.  Buttons = " + buttonStates.ToString() + " line = " + line);
            }
        }

        /// <summary>
        /// Gets a copy of the current state of the buttons.
        /// </summary>
        /// <returns>The copy of the button states.</returns>
        public int[] GetStatesCopy() {
            int[] states = new int[_states.Length];
            for (int i = 0; i < _states.Length; i++)
                states[i] = _states[i];
            return states;
        }

        /// <summary>
        /// Changes the current port to the given port.
        /// </summary>
        /// <param name="portName">The name of the port to change to.</param>
        public void ChangePort(string portName) {
            if (_port != null && _port.IsOpen)
                ClosePort();

            _port = new SerialPort(portName);
            _port.BaudRate = 9600;
            _port.DataReceived += new SerialDataReceivedEventHandler(onPortDataReceived);
            _port.Open();
            _port.DiscardInBuffer();
        }

        /// <summary>
        /// A helper function that closes the port on a background thread.
        ///    due to a bug in SerialPort.Close()
        /// </summary>
        private void ClosePortOnBackgroundThread() {
            try {
                // Try to close the port.
                //    If the chord was ripped out, this
                //    will throw an exception.
                if (_port != null && _port.IsOpen) {
                    _port.Close();
                }
            } catch (Exception e) {
                Console.WriteLine("Error closing the existing port: " + e.Message);
            }
        }

        /// <summary>
        /// Closes the current port.
        /// </summary>
        private void ClosePort() {
            // ?? Due to a bug in SerialPort (See: http://social.msdn.microsoft.com/forums/en-US/Vsexpressvcs/thread/ce8ce1a3-64ed-4f26-b9ad-e2ff1d3be0a5/)
            //    we must close in a new thread and wait for it to complete.
            Thread closePortThread = new Thread(new ThreadStart(ClosePortOnBackgroundThread));
            closePortThread.Start();

            // Wait for the port to be closed.
            while (_port.IsOpen)
                Thread.Sleep(5);
        }

        /// <summary>
        /// Stops the driver and closes the current port.
        ///     Blocks until the port is closed.
        /// </summary>
        public void Stop() {
            if (_port != null && _port.IsOpen) {
                _port.DataReceived -= new SerialDataReceivedEventHandler(onPortDataReceived);
                ClosePort();
            }
        }
    }
}
