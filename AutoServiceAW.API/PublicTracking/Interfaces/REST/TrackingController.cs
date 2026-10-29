using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using AutoServiceAW.API.WorkshopOperations.Domain.Services;
using AutoServiceAW.API.PublicTracking.Application.Internal;
using AutoServiceAW.API.CustomerManagement.Domain.Services;
using AutoServiceAW.API.FleetManagement.Domain.Services;
using AutoServiceAW.API.TenantManagement.Domain.Repositories;
using AutoServiceAW.API.StaffCoordination.Domain.Repositories;
using System.Linq;
using System.Threading.Tasks;

namespace AutoServiceAW.API.PublicTracking.Interfaces.REST
{
    [ApiController]
    [Route("api/v1/tracking")]
    [AllowAnonymous]
    public class TrackingController : ControllerBase
    {
        private readonly IWorkOrderService _workOrderService;
        private readonly IVehicleService _vehicleService;
        private readonly ITaskService _taskService;
        private readonly ICustomerService _customerService;
        private readonly IWorkshopRepository _workshopRepository;
        private readonly IMechanicRepository _mechanicRepository;

        public TrackingController(
            IWorkOrderService workOrderService,
            IVehicleService vehicleService,
            ITaskService taskService,
            ICustomerService customerService,
            IWorkshopRepository workshopRepository,
            IMechanicRepository mechanicRepository)
        {
            _workOrderService = workOrderService;
            _vehicleService = vehicleService;
            _taskService = taskService;
            _customerService = customerService;
            _workshopRepository = workshopRepository;
            _mechanicRepository = mechanicRepository;
        }

        [HttpGet("workorders")]
        public async Task<IActionResult> GetOrderByCode([FromQuery] string trackingCode)
        {
            if (string.IsNullOrWhiteSpace(trackingCode)) return BadRequest();

            var order = await _workOrderService.GetByTrackingCodeAsync(trackingCode);

            if (order == null) return NotFound();
            
            var publicOrder = new TrackingWorkOrderResource(
                order.TrackingCode,
                order.Status,
                order.EstimatedDate,
                order.VehicleId,
                order.CustomerId,
                order.WorkshopId
            );

            return Ok(new[] { publicOrder });
        }

        [HttpGet("summary")]
        public async Task<IActionResult> GetSummaryByCode([FromQuery] string trackingCode)
        {
            if (string.IsNullOrWhiteSpace(trackingCode)) return BadRequest();

            var order = await _workOrderService.GetByTrackingCodeAsync(trackingCode);
            if (order == null) return NotFound();

            var tasks = await _taskService.ListByWorkOrderIdAsync(order.Id);
            var mechanic = await _mechanicRepository.FindByIdAsync(order.MechanicId);
            if (mechanic != null && !string.Equals(mechanic.WorkshopId, order.WorkshopId, StringComparison.Ordinal))
            {
                mechanic = null;
            }

            return Ok(TrackingSummaryFactory.Create(order, tasks, mechanic));
        }

        [HttpGet("vehicles/{id}")]
        public async Task<IActionResult> GetVehicle(int id)
        {
            var vehicle = await _vehicleService.GetByIdAsync(id);
            if (vehicle == null) return NotFound();
            
            return Ok(vehicle);
        }

        [HttpGet("customers/{id}")]
        public async Task<IActionResult> GetCustomer(int id)
        {
            var customer = await _customerService.GetByIdAsync(id);
            if (customer == null) return NotFound();
            
            return Ok(new { id = customer.Id, fullName = customer.FullName });
        }

        [HttpGet("workshops/{id}")]
        public async Task<IActionResult> GetWorkshop(string id)
        {
            var workshops = await _workshopRepository.ListAsync();
            var workshop = workshops.FirstOrDefault(w => w.TenantId == id);
            
            if (workshop == null) return NotFound();
            
            return Ok(new { id = workshop.Id, name = workshop.Name }); 
        }
    }
}
