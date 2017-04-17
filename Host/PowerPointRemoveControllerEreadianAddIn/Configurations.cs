//------------------------------------------------------------------------------------------------------------------------------------------
// <copyright file="Configurations.cs" company="Ereadian">
//     Copyright (c) Ereadian.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------------------------------------------------------------------

namespace PowerPointRemoveControllerEreadianAddIn
{
    using Microsoft.Win32;

    /// <summary>
    /// Host Configurations
    /// </summary>
    /// <remarks>
    /// Configurations are stored in registry. <see cref="Configurations.ConfigurationRegistryKey"/>
    /// </remarks>
    public class Configurations
    {
        /// <summary>
        /// Configuration registry key
        /// </summary>
        private const string ConfigurationRegistryKey = @"HKEY_CURRENT_USER\SOFTWARE\EreadianPowerPointRemote";

        /// <summary>
        /// Initializes a new instance of the <see cref="Configurations" /> class.
        /// </summary>
        public Configurations()
        {
            this.Enabled = (int)Registry.GetValue(ConfigurationRegistryKey, nameof(this.Enabled), 0) > 0;
            this.RemoteDeviceName = Registry.GetValue(ConfigurationRegistryKey, nameof(this.RemoteDeviceName), null) as string;
            this.SocketPortNumber = (int)Registry.GetValue(ConfigurationRegistryKey, nameof(this.SocketPortNumber), 5000);
            this.ThreadStopTimeout = (int)Registry.GetValue(ConfigurationRegistryKey, nameof(this.ThreadStopTimeout), 5000);
        }

        /// <summary>
        /// Gets name of remote device name
        /// </summary>
        /// <remarks>
        /// Due to security reason, only registered device can be accepted.
        /// </remarks>
        public string RemoteDeviceName { get; }

        /// <summary>
        /// Gets TCP port number
        /// </summary>
        public int SocketPortNumber { get; }

        /// <summary>
        /// Gets the maximum thread stop waiting time
        /// </summary>
        public int ThreadStopTimeout { get; }

        /// <summary>
        /// Enable AddIn
        /// </summary>
        public bool Enabled { get; }
    }
}
