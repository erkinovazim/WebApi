using AutoMapper;
using Contracts;
using Entities.DataTransferObjects;
using Entities.Models;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;

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
        public IActionResult GetEmployeesForCompany(Guid companyId)
        {
            var company = _repositoryManager.Company.GetCompany(companyId, trackChanges: false);
            if (company==null)
            {
                _loggerManager.LogInfo($"Company with id : {companyId} does not exist in the database");
                return NotFound();
            }
            else
            {
                var employeesFromDb = _repositoryManager.Employee.GetEmployees(companyId, trackChanges: false);
                var employeesDto = _mapper.Map<IEnumerable<EmployeeDto>>(employeesFromDb);
                return Ok(employeesDto);
            }
        }
        [HttpGet("{id}",Name ="GetEmployeeForCompany")]
        public IActionResult GetEmployeeForCompany(Guid companyid, Guid id)
        {
            var company = _repositoryManager.Company.GetCompany(companyid, trackChanges: false);
            if (company == null)
            {
                _loggerManager.LogInfo($"Company with id : {companyid} does not exist");
                return NotFound();
            }
            var employeeDb = _repositoryManager.Employee.GetEmployee(companyid, id, trackChanges: false);
            if (employeeDb == null)
            {
                _loggerManager.LogInfo($"Employee with id : {id} does not exist");
                return NotFound();
            }
            var employee = _mapper.Map<EmployeeDto>(employeeDb);
            return Ok(employee);
        }
        [HttpPost]
        public IActionResult CreateEmployeeForCompany(Guid companyId,[FromBody] EmployeeForCreationDto employee)
        {
            if (employee == null)
            {
                _loggerManager.LogError("EmployeeForCreationDto object sent from client is null");
                return BadRequest("EmployeeForCreationDto object is null");
            }
            var company = _repositoryManager.Company.GetCompany(companyId,trackChanges:false);
            if(company == null)
            {
                _loggerManager.LogInfo($"Company with id {companyId} doesn't exist in the database");
                return NotFound();
            }
            var employeeEntity = _mapper.Map<Employee>(employee);
            _repositoryManager.Employee.CreateEmployee(companyId, employeeEntity);
            _repositoryManager.Save();

            var employeeToReturn = _mapper.Map<EmployeeDto>(employeeEntity);
            return CreatedAtRoute("GetEmployeeForCompany", new { companyId, id = employeeToReturn.Id }, employeeToReturn);
        }
        [HttpDelete("{id}")]
        public IActionResult DeleteEmployeeForCompany(Guid companyId, Guid id)
        {
            var company = _repositoryManager.Company.GetCompany(companyId,trackChanges:!false);
            if(company==null)
            {
                _loggerManager.LogInfo($"Company with id : {companyId} doesn't exist in the database");
                return NotFound();
            }
            var employeeForCompany = _repositoryManager.Employee.GetEmployee(companyId,id,trackChanges:false);
            if(employeeForCompany == null)
            {
                _loggerManager.LogInfo($"Employee with id : {id} doesn't exist in the database");
                return NotFound();
            }
            _repositoryManager.Employee.DeleteEmployee(employeeForCompany);
            _repositoryManager.Save();
            return NoContent();
        }
        [HttpPut("{id}")]
        public IActionResult UpdateEmployeeForCompany(Guid companyId, Guid id, [FromBody] EmployeeForUpdateDto employee)
        {
            if(employee==null)
            {
                _loggerManager.LogError("EmployeeForUpdateDto object sent from client is null.");
                return BadRequest("EmployeeForUpdateDto object is null");
            }
            var company = _repositoryManager.Company.GetCompany(companyId, trackChanges: false);
            if(company==null)
            {
                _loggerManager.LogInfo($"Company with id: {companyId} doesn't exist in the database.");
                return NotFound();
            }
            var employeeEntity = _repositoryManager.Employee.GetEmployee(companyId,id,trackChanges:true);
            if(employeeEntity == null)
            {
                _loggerManager.LogInfo($"Employee with id: {id} doesn't exist in the database.");
                return NotFound();
            }
            _mapper.Map(employee,employeeEntity);
            _repositoryManager.Save();
            return NoContent();
        }
    }
}
