//Copyright 2011 Cuno Pfister
//
//Licensed under the Apache License, Version 2.0 (the "License");
//you may not use this file except in compliance with the License.
//You may obtain a copy of the License at
//
//    http://www.apache.org/licenses/LICENSE-2.0
//
//Unless required by applicable law or agreed to in writing, software
//distributed under the License is distributed on an "AS IS" BASIS,
//WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//See the License for the specific language governing permissions and
//limitations under the License.

//Developed for the book
//  "Getting Started with the Internet of Things", by Cuno Pfister.
//  Copyright 2011 Cuno Pfister, Inc., 978-1-4493-9357-1.
//
//Version 0.9 (beta release)

// These classes provide objects for sensors and actuators that implement
// a common interface pattern: Properties are used to configure an object,
// and Open method checks the properties and created internal objects as
// needed, and HandleGet and HandleSet methods provide a standard way for
// getting new samples from a sensor, or for setting and getting setpoints
// for an actuator.

using Gsiot.Contracts;
using Microsoft.SPOT.Hardware;
//using SecretLabs.NETMF.Hardware;

namespace Gsiot.Server
{
    /// <summary>
    /// Represents a digital input (GPIO pin).
    /// </summary>
    public class DigitalSensor
    {
        Cpu.Pin pin = Cpu.Pin.GPIO_NONE;
        InputPort port;

        /// <summary>
        /// Mandatory property that denotes the pin to be used as input.
        /// Must be set before sensor is opened.
        /// </summary>
        public Cpu.Pin InputPin
        {
            get { return pin; }

            set
            {
                Contract.Requires(port == null);    // sensor is not open
                pin = value;
            }
        }

        /// <summary>
        /// To be called after setting the properties of the object.
        /// If not called explicitly, it is automatically called when
        /// the sensor is used for the first time.
        /// Preconditions
        ///     Sensor is not open
        ///     InputPin is set
        /// Postconditions
        ///     Sensor is open
        /// </summary>
        public void Open()
        {
            Contract.Requires(port == null);        // sensor is not open
            Contract.Requires(InputPin != Cpu.Pin.GPIO_NONE);
            port = new InputPort(InputPin, false,
                Port.ResistorMode.Disabled);
        }

        /// <summary>
        /// Returns a new sample.
        /// If the sensor is not open, Open is called first.
        /// Postconditions
        ///     (Result == null) || (Result is bool)
        /// </summary>
        public object HandleGet()
        {
            if (port == null) { Open(); }
            return port.Read();     // bool converted to object
        }
    }

    /// <summary>
    /// Represents a digital output (GPIO pin).
    /// </summary>
    public class DigitalActuator
    {
        Cpu.Pin pin = Cpu.Pin.GPIO_NONE;
        bool actualState;
        OutputPort port;

        /// <summary>
        /// Mandatory property that denotes the pin to be used as output.
        /// Must be set before actuator is opened.
        /// </summary>
        public Cpu.Pin OutputPin
        {
            get { return pin; }

            set
            {
                Contract.Requires(port == null);    // actuator is not open
                pin = value;
            }
        }

        /// <summary>
        /// Optional property that denotes the initial state of the output.
        /// Default: false
        /// Preconditions
        ///      Actuator is not open
        /// </summary>
        public bool InitialState
        {
            get { return actualState; }

            set
            {
                Contract.Requires(port == null);    // actuator is not open
                actualState = value;
            }
        }

        /// <summary>
        /// To be called after setting the properties of the object.
        /// If not called explicitly, it is automatically called when
        /// the sensor is used for the first time.
        /// Preconditions
        ///     Actuator is not open
        ///     OutputPin is set
        /// Postconditions
        ///     Actuator is open
        /// </summary>
        public void Open()
        {
            Contract.Requires(port == null);        // actuator is not open
            Contract.Requires(OutputPin != Cpu.Pin.GPIO_NONE);
            port = new OutputPort(OutputPin, actualState);
        }

        /// <summary>
        /// Sets a new setpoint.
        /// If the actuator is not open, Open is called first.
        /// Preconditions
        ///     setpoint != null
        ///     setpoint is bool
        /// </summary>
        public void HandlePut(object setpoint)
        {
            Contract.Requires(setpoint != null);
            Contract.Requires(setpoint is bool);
            if (port == null) { Open(); }
            actualState = (bool)setpoint;
            port.Write(actualState);
        }

        /// <summary>
        /// Returns most recent setpoint.
        /// If none has been set yet, the initial state is returned.
        /// If the actuator is not open, Open is called first.
        /// Postconditions
        ///     (Result == null) || (Result is bool)
        /// </summary>
        public object HandleGet()
        {
            if (port == null) { Open(); }
            return actualState;
        }
    }

    /*
    /// <summary>
    /// Represents an analog input.
    /// Assumes 10 bit resolution, i.e., values from 0 to 1023.
    /// If the MinValue and MaxValue properties are set, then the
    /// input value is linearly scaled to this range and returned
    /// as a double value.
    /// Otherwise the raw integer value is returned as an int value.
    /// </summary>
    public class AnalogSensor
    {
        const int maxAdcValue = 1023;           // for 10 bit resolution

        Cpu.Pin pin = Cpu.Pin.GPIO_NONE;
        AnalogInput port;

        /// <summary>
        /// Mandatory property that denotes the pin to be used as input.
        /// Must be set before sensor is opened.
        /// </summary>
        public Cpu.Pin InputPin
        {
            get { return pin; }

            set
            {
                Contract.Requires(port == null);    // sensor is not open
                pin = value;
            }
        }

        public double MinValue { get; set; }
        public double MaxValue { get; set; }
        double Delta;
        bool doTransformation;

        /// <summary>
        /// To be called after setting the properties of the object.
        /// If not called explicitly, it is automatically called when
        /// the sensor is used for the first time.
        /// Preconditions
        ///     Sensor is not open
        ///     InputPin is set
        ///     if MinValue or MaxValue is set: MinValue lessOrEqual MaxValue
        /// Postconditions
        ///     Sensor is open
        /// </summary>
        public void Open()
        {
            Contract.Requires(port == null);        // sensor is not open
            Contract.Requires(InputPin != Cpu.Pin.GPIO_NONE);
            Contract.Requires(MinValue <= MaxValue);
            port = new AnalogInput(InputPin);
            Delta = MaxValue - MinValue;
            doTransformation = Delta > 0;
        }

        /// <summary>
        /// Returns a new sample.
        /// If the sensor is not open, Open is called first.
        /// Postconditions
        ///     (Result == null) || (Result is int) || (Result is double)
        /// </summary>
        public object HandleGet()
        {
            if (port == null) { Open(); }
            int rawValue = port.Read();
            if (doTransformation)
            {
                return MinValue + ((rawValue * Delta) / maxAdcValue);
            }
            else
            {
                return rawValue;
            }
        }
    }
     */
}
