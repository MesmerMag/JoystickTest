using System;
using System.Windows.Threading;
using JoystickTest.DirectInput;
using Vortice.DirectInput;

namespace JoystickTest
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow
    {
        private readonly InputDevice _directInputDevice;

        public MainWindow()
        {
            InitializeComponent();

            _directInputDevice = new InputDevice();
            _directInputDevice.Initialize(IntPtr.Zero);

            var timer = new DispatcherTimer(DispatcherPriority.Normal) { Interval = TimeSpan.FromMilliseconds(100) };
            timer.Tick += TimerTick;
            timer.Start();
        }

        private static string HandleButtonPressUpdate(string button, JoystickUpdate update)
        {
            return update.Value switch
            {
                // Pressed:
                128 => button,
                // Let go (Value == 0):
                _ => ""
            };
        }

        private void TimerTick(object? sender, EventArgs eventArgs)
        {
            foreach (var update in _directInputDevice.GetJoystickUpdates())
            {
                switch (update.Offset)
                {
                    case JoystickOffset.Buttons0:
                        ButtonDebugText.Text = HandleButtonPressUpdate("A", update);
                        break;
                    case JoystickOffset.Buttons1:
                        ButtonDebugText.Text = HandleButtonPressUpdate("B", update);
                        break;
                    case JoystickOffset.Buttons2:
                        ButtonDebugText.Text = HandleButtonPressUpdate("X", update);
                        break;
                    case JoystickOffset.Buttons3:
                        ButtonDebugText.Text = HandleButtonPressUpdate("Y", update);
                        break;

                    // POV-hat
                    case JoystickOffset.PointOfViewControllers0:
                        switch (update.Value)
                        {
                            case -1:
                                ButtonDebugText.Text = "";
                                break;
                            case 0:
                                ButtonDebugText.Text = "Up";
                                break;
                            case 9000:
                                ButtonDebugText.Text = "Right";
                                break;
                            case 18000:
                                ButtonDebugText.Text = "Down";
                                break;
                            case 27000:
                                ButtonDebugText.Text = "Left";
                                break;
                        }

                        break;
                }
            }
        }
    }
}
