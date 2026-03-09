using EventBus.Abstractions;
using EventBusRabbitMQ.example;
using Microsoft.AspNetCore.Mvc;

namespace Producer.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class EventController : ControllerBase
    {
        private readonly IEventBus _eventProducer;

        public EventController(IEventBus eventProducer)
        {
            _eventProducer = eventProducer;
        }

        [HttpGet]
        public async Task<IActionResult> Get()
        {
            var data = new EventData();
            data.Message = "Message from ID:" + data.Id;

            await _eventProducer.PublishAsync(MessageQueue.Queue, data);

            return Ok();
        }
    }
}