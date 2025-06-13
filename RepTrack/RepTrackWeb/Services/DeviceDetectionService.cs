using Microsoft.AspNetCore.Http;
using System.Text.RegularExpressions;

namespace RepTrackWeb.Services
{
    /// <summary>
    /// Implementation of device detection service for optimizing user experience
    /// </summary>
    public class DeviceDetectionService : IDeviceDetectionService
    {
        private static readonly Regex MobileRegex = new(
            @"(android|bb\d+|meego).+mobile|avantgo|bada\/|blackberry|blazer|compal|elaine|fennec|hiptop|iemobile|ip(hone|od)|iris|kindle|lge |maemo|midp|mmp|mobile.+firefox|netfront|opera m(ob|in)i|palm( os)?|phone|p(ixi|re)\/|plucker|pocket|psp|series(4|6)0|symbian|treo|up\.(browser|link)|vodafone|wap|windows ce|xda|xiino",
            RegexOptions.IgnoreCase | RegexOptions.Multiline | RegexOptions.Compiled);

        private static readonly Regex TabletRegex = new(
            @"android|ipad|playbook|silk",
            RegexOptions.IgnoreCase | RegexOptions.Multiline | RegexOptions.Compiled);

        public bool IsMobileDevice(HttpContext context)
        {
            var userAgent = GetUserAgent(context);
            if (string.IsNullOrEmpty(userAgent))
                return false;

            return MobileRegex.IsMatch(userAgent) && !IsTabletDevice(context);
        }

        public bool IsTabletDevice(HttpContext context)
        {
            var userAgent = GetUserAgent(context);
            if (string.IsNullOrEmpty(userAgent))
                return false;

            return TabletRegex.IsMatch(userAgent);
        }

        public DeviceType GetDeviceType(HttpContext context)
        {
            if (IsMobileDevice(context))
                return DeviceType.Mobile;
            
            if (IsTabletDevice(context))
                return DeviceType.Tablet;

            return DeviceType.Desktop;
        }

        public bool SupportsTouchInput(HttpContext context)
        {
            return IsMobileDevice(context) || IsTabletDevice(context);
        }

        private static string GetUserAgent(HttpContext context)
        {
            return context.Request.Headers["User-Agent"].FirstOrDefault() ?? string.Empty;
        }
    }
}
