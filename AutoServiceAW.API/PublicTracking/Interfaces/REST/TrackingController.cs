using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using AutoServiceAW.API.WorkshopOperations.Domain.Services;
using AutoServiceAW.API.CustomerManagement.Domain.Services;
using AutoServiceAW.API.FleetManagement.Domain.Services;
using AutoServiceAW.API.TenantManagement.Domain.Repositories;
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

        public TrackingController(
            IWorkOrderService workOrderService,
            IVehicleService vehicleService,
            ITaskService taskService,
            ICustomerService customerService,
            IWorkshopRepository workshopRepository)
        {
            _workOrderService = workOrderService;
            _vehicleService = vehicleService;
            _taskService = taskService;
            _customerService = customerService;
            _workshopRepository = workshopRepository;
        }

        [HttpGet("workorders")]
        public async Task<IActionResult> GetOrderByCode([FromQuery] string trackingCode)
        {
            var orders = await _workOrderService.ListAsync();
            var order = orders.FirstOrDefault(o => o.TrackingCode == trackingCode);

            if (order == null) return NotFound();
            
            return Ok(new[] { order });
        }

        [HttpGet("vehicles/{id}")]
        public async Task<IActionResult> GetVehicle(int id)
        {
            var vehicle = await _vehicleService.GetByIdAsync(id);
            if (vehicle == null) return NotFound();
            
            return Ok(vehicle);
        }

        [HttpGet("tasks")]
        public async Task<IActionResult> GetTasksByOrder([FromQuery] int workOrderId)
        {
            var tasks = await _taskService.ListByWorkOrderIdAsync(workOrderId);
            return Ok(tasks);
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