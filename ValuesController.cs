using Contracts;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Collections;
using System.Collections.Generic;

namespace WebApiMS
{
    [Route("api/[controller]")]
    [ApiController]
    public class ValuesController : ControllerBase
    {
        private readonly IRepositoryManager _repositoryManager;
        public ValuesController(IRepositoryManager repositoryManager)
        {
            _repositoryManager = repositoryManager;
        }
        private ILoggerManager _logger;
        public ValuesController(ILoggerManager logger)
        {
            _logger = logger;
        }
        [HttpGet]
        public IEnumerable<string>Get()
        {
            _logger.LogInfo("Here is info message");
            _logger.LogDebug("Here is debug message");
            _logger.LogWarning("Here is warn message");
            _logger.LogError("Here is error message");

            return new string[] {"value1","value2"};
        }
        
    }
}
