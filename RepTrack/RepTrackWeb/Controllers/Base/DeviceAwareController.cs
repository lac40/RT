using Microsoft.AspNetCore.Mvc;
using RepTrackWeb.Services;

namespace RepTrackWeb.Controllers.Base
{
    /// <summary>
    /// Base controller that provides device-aware functionality
    /// </summary>
    public abstract class DeviceAwareController : Controller
    {
        protected readonly IDeviceDetectionService _deviceDetectionService;

        protected DeviceAwareController(IDeviceDetectionService deviceDetectionService)
        {
            _deviceDetectionService = deviceDetectionService ?? throw new ArgumentNullException(nameof(deviceDetectionService));
        }

        /// <summary>
        /// Gets the current device type
        /// </summary>
        protected DeviceType DeviceType => _deviceDetectionService.GetDeviceType(HttpContext);

        /// <summary>
        /// Indicates if the current device is mobile
        /// </summary>
        protected bool IsMobile => _deviceDetectionService.IsMobileDevice(HttpContext);

        /// <summary>
        /// Indicates if the current device supports touch
        /// </summary>
        protected bool SupportsTouchInput => _deviceDetectionService.SupportsTouchInput(HttpContext);

        /// <summary>
        /// Returns a view optimized for the current device type
        /// </summary>
        protected IActionResult DeviceView(string viewName, object? model = null)
        {
            ViewBag.IsMobile = IsMobile;
            ViewBag.DeviceType = DeviceType.ToString();
            ViewBag.SupportsTouchInput = SupportsTouchInput;

            if (IsMobile)
            {
                // Try to find mobile-specific view first
                var mobileViewName = $"{viewName}.Mobile";
                // For simplicity, just return the standard view for now
                // In a production app, you would check if the mobile view exists
                return View(viewName, model);
            }

            return View(viewName, model);
        }
    }
}
