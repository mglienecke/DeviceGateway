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
using System;
using Gsiot.Contracts;

namespace Gsiot.Server.Simulator
{
    public class Pins
    {
        public const int GPIO_NONE = -1;
    }

    /// <summary>
    /// Represents a digital input (GPIO pin).
    /// </summary>
    public abstract class DigitalSensor
    {
        Int32 pin = Pins.GPIO_NONE;
        object port;

        /// <summary>
        /// Mandatory property that denotes the pin to be used as input.
        /// Must be set before sensor is opened.
        /// </summary>
        public Int32 InputPin
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
            Contract.Requires(InputPin != Pins.GPIO_NONE);
            port = new object();
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
            return ReadValue();     // bool converted to object
        }

        /// <summary>
        /// The method simulates reading the current boolean value from the opened input port.
        /// </summary>
        /// <returns></returns>
        public abstract bool ReadValue();
    }

    /// <summary>
    /// The class simulates a digital output (GPIO pin).
    /// </summary>
    public abstract class DigitalActuator
    {
        Int32 pin = Pins.GPIO_NONE;
        bool actualState;
        object port;

        /// <summary>
        /// Mandatory property that denotes the pin to be used as output.
        /// Must be set before actuator is opened.
        /// </summary>
        public Int32 OutputPin
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
            Contract.Requires(OutputPin != Pins.GPIO_NONE);
            port = new object();
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
            WriteValue(actualState);
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

        /// <summary>
        /// The method simulates write a value to the opened output port.
        /// </summary>
        /// <param name="value"></param>
        protected abstract void WriteValue(bool value);
    }


    /// <summary>
    /// The class simulates an analog sensor.
    /// Assumes 10 bit resolution, i.e., values from 0 to 1023.
    /// If the MinValue and MaxValue properties are set, then the
    /// input value is linearly scaled to this range and returned
    /// as a double value.
    /// Otherwise the raw integer value is returned as an int value.
    /// </summary>
    public abstract class AnalogSensorSimulator
    {
        const int maxAdcValue = 1023;           // for 10 bit resolution
        private object port = null;
        Int32 pin = Pins.GPIO_NONE;

        /// <summary>
        /// Mandatory property that denotes the pin to be used as input.
        /// Must be set before sensor is opened.
        /// </summary>
        public Int32 InputPin
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
            Contract.Requires(InputPin != Pins.GPIO_NONE);
            Contract.Requires(MinValue <= MaxValue);
            port = new Object();
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
            int rawValue = ReadValue();
            if (doTransformation)
            {
                return MinValue + ((rawValue * Delta) / maxAdcValue);
            }
            else
            {
                return rawValue;
            }
        }

        /// <summary>
        /// The method simulates reading the current analog value from the sensor.
        /// </summary>
        /// <returns></returns>
        public abstract int ReadValue();
    }
}
