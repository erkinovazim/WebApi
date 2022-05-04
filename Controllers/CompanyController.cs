using AutoMapper;
using Contracts;
using Entities.DataTransferObjects;
using Entities.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding.Binders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WebApiMS.ModelBinders;

namespace WebApiMS.Controllers
{
    [Route("api/companies")]
    [ApiController]
    public class CompanyController : ControllerBase
    {
        private readonly IRepositoryManager _repository;
        private readonly ILoggerManager _logger;
        private readonly IMapper _mapper;
        public CompanyController(IRepositoryManager repository, ILoggerManager logger, IMapper mapper)
        {
            _repository= repository;   
            _logger= logger;
            _mapper= mapper;
        }
        [HttpGet]
        public async Task<IActionResult> GetCompaniesAsync()
        {
                var companies =await _repository.Company.GetAllCompaniesAsync(trackChanges: false);

                var companiesDto = _mapper.Map<IEnumerable<CompanyDto>>(companies);

                return Ok(companiesDto);         
        }
        [HttpGet("{id}", Name ="CompanyById")] // Name will come in handy in the action method for creating a new company 
        public async Task<IActionResult> GetCompanyAsync(Guid id)
        {
            var company =await _repository.Company.GetCompanyAsync(id, trackChanges: false);
            if(company == null)
            {
                _logger.LogInfo($"Company with id : {id} does not exit in the database");
                return NotFound();
            }
            else
            {
                var companyDto = _mapper.Map<CompanyDto>(company);
                return Ok(companyDto);
            }
        }
        [HttpGet("collection/{ids}",Name ="GetCompanyCollection")]
        public async Task<IActionResult> GetCompanyCollectionAsync([ModelBinder(BinderType = typeof(ArrayModelBinder))]IEnumerable<Guid> ids)
        {
            // ArrayModelBinder will convert sent spring parameter to IEnumerable<Guid> type
            if (ids == null)
            {
                _logger.LogError("Parameter ids is null");
                return BadRequest("Parameter ids is null");
            }
            var companyEntities =await _repository.Company.GetByIdsAsync(ids, trackChanges: false);
            if(ids.Count()!=companyEntities.Count())
            {
                _logger.LogError("Some ids are not valid in a collection");
                return NotFound();
            }
            var companiesToReturn = _mapper.Map<IEnumerable<CompanyDto>>(companyEntities);
            return Ok(companiesToReturn);
        }
        [HttpPost]
        [ServiceFilter(typeof(ValidationFilterAttribute))]
        public async Task<IActionResult> CreateCompanyAsync([FromBody] CompanyForCreationDto company)
        {
            var companyEntity = _mapper.Map<Company>(company);
            _repository.Company.CreateCompany(companyEntity);
            await _repository.SaveAsync();
            var companyToReturn =_mapper.Map<CompanyDto>(companyEntity);
            return CreatedAtRoute("CompanyById", new { id = companyToReturn.Id }, companyToReturn);
        }
        [HttpPost("collection")]
        public async Task<IActionResult> CreateCompanyCollectionAsync([FromBody] IEnumerable<CompanyForCreationDto> companyCollection)
        {
            if(companyCollection == null)
            {
                _logger.LogError("Company collection sent from user is null");
                return BadRequest("Company collection is null");
            }
            var companyEntities = _mapper.Map<IEnumerable<Company>>(companyCollection);
            foreach(var company in companyEntities)
            {
                _repository.Company.CreateCompany(company);
            }
            await _repository.SaveAsync();

            var companyCollectionToReturn = _mapper.Map<IEnumerable<CompanyDto>>(companyEntities);
            var ids = string.Join(",", companyCollectionToReturn.Select(c => c.Id));

            return CreatedAtRoute("GetCompanyCollection",new {ids}, companyCollectionToReturn);
        }
        [HttpDelete("{id}")]
        [ServiceFilter(typeof(ValidateCompanyExistsAttribute))]
        public async Task<IActionResult> DeleteCompanyAsync(Guid id)
        {
            var company = HttpContext.Items["company"] as Company;
   
            _repository.Company.DeleteCompany(company);
            await _repository.SaveAsync();
            return NoContent();
        }
        [HttpPut("{id}")]
        [ServiceFilter(typeof(ValidationFilterAttribute))]
        [ServiceFilter(typeof(ValidateCompanyExistsAttribute))]
        public async Task<IActionResult> UpdateCompanyAsync(Guid id, [FromBody] CompanyForUpdateDto company)
        {
            var companyEntity = HttpContext.Items["company"] as Company;
            _mapper.Map(company,companyEntity);
            await _repository.SaveAsync();
            return NoContent();
        }
    }
}
