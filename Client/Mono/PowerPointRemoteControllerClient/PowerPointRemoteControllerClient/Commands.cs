using System;

namespace Ereadian.PowerPointRemoteController.Client
{
    /// <summary>
    /// Remote commends to PowerPoint host
    /// </summary>
    public static class Commands
    {
        /// <summary>
        /// Back to previous step
        /// </summary>
        public static byte PreviousClick = 0;

        /// <summary>
        /// "Click" the PowerPoint 
        /// </summary>
        public static byte NextClick = 1;
    }
}

