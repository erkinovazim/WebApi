using AutoMapper;
using Contracts;
using Entities.DataTransferObjects;
using Entities.Models;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace WebApiMS.Controllers
{
    [Route("api/companies/{companyId}/employees")]
    [ApiController]
    public class EmployeesController : ControllerBase
    {
        private readonly ILoggerManager _loggerManager;
        private readonly IRepositoryManager _repositoryManager;
        private readonly IMapper _mapper;
        public EmployeesController(ILoggerManager loggerManager, IRepositoryManager repositoryManager, IMapper mapper)
        {
            _loggerManager = loggerManager;
            _repositoryManager = repositoryManager;
            _mapper = mapper;
        }
        [HttpGet]
        public async Task<IActionResult> GetEmployeesForCompanAsync(Guid companyId)
        {
            var company =await _repositoryManager.Company.GetCompanyAsync(companyId, trackChanges: false);
            if (company==null)
            {
                _loggerManager.LogInfo($"Company with id : {companyId} does not exist in the database");
                return NotFound();
            }
            else
            {
                var employeesFromDb =await _repositoryManager.Employee.GetEmployeesAsync(companyId, trackChanges: false);
                var employeesDto = _mapper.Map<IEnumerable<EmployeeDto>>(employeesFromDb);
                return Ok(employeesDto);
            }
        }
        [HttpGet("{id}",Name ="GetEmployeeForCompany")]
        public async Task<IActionResult> GetEmployeeForCompanyAsync(Guid companyid, Guid id)
        {
            var company =await _repositoryManager.Company.GetCompanyAsync(companyid, trackChanges: false);
            if (company == null)
            {
                _loggerManager.LogInfo($"Company with id : {companyid} does not exist");
                return NotFound();
            }
            var employeeDb =await _repositoryManager.Employee.GetEmployeeAsync(companyid, id, trackChanges: false);
            if (employeeDb == null)
            {
                _loggerManager.LogInfo($"Employee with id : {id} does not exist");
                return NotFound();
            }
            var employee = _mapper.Map<EmployeeDto>(employeeDb);
            return Ok(employee);
        }
        [HttpPost]
        public async Task<IActionResult> CreateEmployeeForCompanyAsync(Guid companyId,[FromBody] EmployeeForCreationDto employee)
        {
            if (employee == null)
            {
                _loggerManager.LogError("EmployeeForCreationDto object sent from client is null");
                return BadRequest("EmployeeForCreationDto object is null");
            }
            if(!ModelState.IsValid)
            {
                _loggerManager.LogError("Invalid model state for the EmployeeForCreationDto");
                return UnprocessableEntity(ModelState);
            }
            var company =await _repositoryManager.Company.GetCompanyAsync(companyId,trackChanges:false);
            if(company == null)
            {
                _loggerManager.LogInfo($"Company with id {companyId} doesn't exist in the database");
                return NotFound();
            }
            var employeeEntity = _mapper.Map<Employee>(employee);
            _repositoryManager.Employee.CreateEmployee(companyId, employeeEntity);
            await _repositoryManager.SaveAsync();

            var employeeToReturn = _mapper.Map<EmployeeDto>(employeeEntity);
            return CreatedAtRoute("GetEmployeeForCompany", new { companyId, id = employeeToReturn.Id }, employeeToReturn);
        }
        [HttpDelete("{id}")]
        [ServiceFilter(typeof(ValidateEmployeeForCompanyExistsAttribute))]
        public async Task<IActionResult> DeleteEmployeeForCompanyAsync(Guid companyId, Guid id)
        {
            var employeeForCompany = HttpContext.Items["employee"] as Employee;
            _repositoryManager.Employee.DeleteEmployee(employeeForCompany);
            await _repositoryManager.SaveAsync();
            return NoContent();
        }
        [HttpPut("{id}")]
        [ServiceFilter(typeof(ValidationFilterAttribute))]
        [ServiceFilter(typeof(ValidateEmployeeForCompanyExistsAttribute))]
        public async Task<IActionResult> UpdateEmployeeForCompanyAsync(Guid companyId, Guid id, [FromBody] EmployeeForUpdateDto employee)
        {
            var employeeEntity = HttpContext.Items["employee"] as Employee;
            _mapper.Map(employee,employeeEntity);
            await _repositoryManager.SaveAsync();
            return NoContent();
        }
        [HttpPatch("{id}")]
        [ServiceFilter(typeof(ValidateEmployeeForCompanyExistsAttribute))]
        public async Task<IActionResult> PartiallyUpdateEmployeeForCompanyAsync(Guid companyId,Guid id,[FromBody] JsonPatchDocument<EmployeeForUpdateDto> patchDoc)
        {
            if(patchDoc==null)
            {
                _loggerManager.LogError("pachtDoc object sent from client is null");
                return BadRequest("pachtDoc object is null");
            }
            var employeeEntity = HttpContext.Items["employee"] as Employee;

            var employeeToPatch = _mapper.Map<EmployeeForUpdateDto>(employeeEntity);
            patchDoc.ApplyTo(employeeToPatch, ModelState);
            TryValidateModel(employeeToPatch);
            if(!ModelState.IsValid)
            {
                _loggerManager.LogError("Invalid model for the patch document");
                return UnprocessableEntity(ModelState);
            }
            _mapper.Map(employeeToPatch,employeeEntity);
            await _repositoryManager.SaveAsync();
            return NoContent();
        }
    }
}
