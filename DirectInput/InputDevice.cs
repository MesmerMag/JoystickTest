using System;
using System.Collections.Generic;
using System.Diagnostics;
using Vortice.DirectInput;

namespace JoystickTest.DirectInput;

public sealed class InputDevice
{
    private readonly IDirectInput8 _directInput;

    private Dictionary<Guid, DeviceInstance>? _joysticks;
    private readonly Dictionary<Guid, IDirectInputDevice8> _joystickDevices;

    public InputDevice()
    {
        _directInput = DInput.DirectInput8Create();
        _joystickDevices = new Dictionary<Guid, IDirectInputDevice8>();
    }

    public void Initialize(IntPtr handle)
    {
        try
        {
            _joysticks = EnumerateAllAttachedGameDevices(_directInput);
            foreach (var item in _joysticks.Keys)
            {
                InitializeJoystickDevice(_directInput, item, handle);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
        }
    }

    private void InitializeJoystickDevice(IDirectInput8 directInput, Guid guid, IntPtr windowHandle)
    {
        var inputDevice = directInput.CreateDevice(guid);
        inputDevice.SetCooperativeLevel(windowHandle, CooperativeLevel.NonExclusive | CooperativeLevel.Foreground);

        // Play the values back to see that BufferSize is working correctly
        inputDevice.Properties.BufferSize = 16;

        if (!directInput.IsDeviceAttached(guid)) return;

        var result = inputDevice.SetDataFormat<RawJoystickState>();
        if (result.Success)
        {
            _joystickDevices.Add(guid, inputDevice);
        }
    }

    public IEnumerable<JoystickUpdate> GetJoystickUpdates()
    {
        foreach (var joystick in _joystickDevices)
        {
            var result = joystick.Value.Poll();
            if (result.Failure)
            {
                result = joystick.Value.Acquire();
                if (result.Failure)
                    return Array.Empty<JoystickUpdate>();
            }
    
            try
            {
                return joystick.Value.GetBufferedJoystickData();
            }
            catch (Exception ex)
            {
                Trace.Write(ex.Message);
            }
        }
    
        return Array.Empty<JoystickUpdate>();
    }

    private static Dictionary<Guid, DeviceInstance> EnumerateAllAttachedGameDevices(IDirectInput8 directInput)
    {
        var connectedDeviceList = new Dictionary<Guid, DeviceInstance>();

        foreach (var deviceInstance in
                 directInput.GetDevices(DeviceClass.GameControl, DeviceEnumerationFlags.AttachedOnly))
        {
            if (deviceInstance.Type is DeviceType.Gamepad or DeviceType.Joystick)
            {
                connectedDeviceList.Add(deviceInstance.InstanceGuid, deviceInstance);
            }
            else
            {
                Console.WriteLine(deviceInstance.ProductName + " does not match input type, ignored.");
            }
        }

        return connectedDeviceList;
    }
}
