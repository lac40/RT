using Microsoft.AspNetCore.Http;

namespace RepTrackWeb.Services
{
    /// <summary>
    /// Service for detecting device capabilities and optimizing user experience
    /// </summary>
    public interface IDeviceDetectionService
    {
        /// <summary>
        /// Determines if the current request is from a mobile device
        /// </summary>
        bool IsMobileDevice(HttpContext context);

        /// <summary>
        /// Determines if the current request is from a tablet device
        /// </summary>
        bool IsTabletDevice(HttpContext context);

        /// <summary>
        /// Gets the device type for the current request
        /// </summary>
        DeviceType GetDeviceType(HttpContext context);

        /// <summary>
        /// Determines if the device supports touch input
        /// </summary>
        bool SupportsTouchInput(HttpContext context);
    }

    public enum DeviceType
    {
        Desktop,
        Tablet,
        Mobile
    }
}
