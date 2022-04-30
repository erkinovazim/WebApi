﻿using AutoMapper;
using Contracts;
using Entities.DataTransferObjects;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;

namespace WebApiMS.Controllers
{
    [Route("api/companies")]
    [ApiController]
    public class CompanyEmployees : ControllerBase
    {
        private readonly IRepositoryManager _repository;
        private readonly ILoggerManager _logger;
        private readonly IMapper _mapper;
        public CompanyEmployees(IRepositoryManager repository, ILoggerManager logger, IMapper mapper)
        {
            _repository= repository;   
            _logger= logger;
            _mapper= mapper;
        }
        [HttpGet]
        public IActionResult GetCompanies()
        {
                var companies = _repository.Company.GetAllCompanies(trackChanges: false);

                var companiesDto = _mapper.Map<IEnumerable<CompanyDto>>(companies);

                return Ok(companiesDto);         
        }
    }
}
