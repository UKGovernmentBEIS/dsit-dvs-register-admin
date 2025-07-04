﻿using DVSAdmin.BusinessLogic.Models;
using DVSAdmin.BusinessLogic.Services;
using DVSAdmin.CommonUtility.Models;
using DVSAdmin.CommonUtility.Models.Enums;
using DVSAdmin.Models.CabTransfer;
using Microsoft.AspNetCore.Mvc;

namespace DVSAdmin.Controllers
{
    [Route("cab-transfer")]
    public class CabTransferController: BaseController
    {
        private readonly ICabTransferService cabTransferService;
        private readonly IUserService userService;
        private readonly IConfiguration configuration;


        public CabTransferController(ICabTransferService cabTransferService, IUserService userService, IConfiguration configuration)
        {
            this.cabTransferService = cabTransferService;
            this.userService = userService;
            this.configuration = configuration;
        }

        [HttpGet("published-service-list")]
        public async Task<IActionResult> AllPublishedServices(string CurrentSort = "status", string CurrentSortAction = "ascending",
            int pageNumber = 1, string SearchText = "", string SearchAction = "", string NewSort = "")
        {
            if (NewSort != string.Empty)
            {
                if (CurrentSort == NewSort)
                {
                    CurrentSortAction = CurrentSortAction == "ascending" ? "descending" : "ascending";
                }
                else
                {
                    CurrentSort = NewSort;
                    CurrentSortAction = "ascending";
                }
                pageNumber = 1;
            }

            if (SearchAction == "clearSearch")
            {
                ModelState.Clear();
                SearchText = string.Empty;
            }
            var results = await cabTransferService.GetServices(pageNumber, CurrentSort, CurrentSortAction, SearchText);
            var totalPages = (int)Math.Ceiling((double)results.TotalCount / 10);

            ViewBag.CurrentPage = pageNumber;

            var serviceListViewModel = new ServiceListViewModel
            {
                Services = results.Items,
                TotalPages = totalPages,
                CurrentSort = CurrentSort,
                CurrentSortAction = CurrentSortAction
            };
            return View(serviceListViewModel);
        }

        [HttpGet("reassign-service")]
        public async Task<IActionResult> ServiceReassign(int serviceId, bool isTransferOngoing)
        {
            if (isTransferOngoing)
            {
                CabTransferRequestDto cabTransferRequestDto = await cabTransferService.GetCabTransferDetails(serviceId);
                ServiceDto service = cabTransferRequestDto.Service;
                ViewBag.ToCabName = cabTransferRequestDto.ToCab.CabName;
                return View(service);
            }
            else
            {
                ServiceDto service = await cabTransferService.GetServiceDetails(serviceId);  
                return View(service);
            }
           
        }

        [HttpGet("service-reassign-start")]
        public async Task<IActionResult> ReassignServiceToCAB(int serviceId)
        {
            ServiceDto service = await cabTransferService.GetServiceDetails(serviceId);
            return View(service);
        }
        
        [HttpPost("service-reassign-start")]
        public IActionResult ReassignServiceToCABPost(int serviceId)
        {
            return RedirectToAction(nameof(SelectConformityAssessmentBody), new { serviceId });
        }
        
        [HttpGet("select-cab")]
        public async Task<IActionResult> SelectConformityAssessmentBody(int serviceId)
        {
            ViewData["Cabs"] = await cabTransferService.ListCabsExceptCurrentAsync(serviceId);

            var service = await cabTransferService.GetServiceDetails(serviceId);
            var selectCabViewModel = new SelectCabViewModel
            {
                ServiceId     = serviceId,
                CurrentCabId    = service.CabUser.CabId,
                CurrentCabName  = service.CabUser.Cab.CabName,
                SelectedCabId = null
            };
            return View("~/Views/CabTransfer/SelectConformityAssessmentBody.cshtml", selectCabViewModel);
        }

        [HttpPost("select-cab")]
        public async Task<IActionResult> SelectConformityAssessmentBody(
            int serviceId,
            SelectCabViewModel selectCabViewModel
        )
        {
            if (!selectCabViewModel.SelectedCabId.HasValue)
            {
                ViewData["Cabs"] = await cabTransferService.ListCabsExceptCurrentAsync(serviceId);
                return View(selectCabViewModel);
            }
            
            return RedirectToAction(nameof(ConfirmCabAcceptance), new { serviceId = serviceId, toCabId = selectCabViewModel.SelectedCabId.Value }
            );
        }
        
        [HttpGet("confirm-cab-acceptance")]
        public async Task<IActionResult> ConfirmCabAcceptance(int serviceId, int toCabId)
        {
            var service = await cabTransferService.GetServiceDetails(serviceId);
            var chosenCab   = (await cabTransferService.ListCabsExceptCurrentAsync(serviceId))
                .Single(c => c.Id == toCabId);

            var confirmCabAcceptanceViewModel = new ConfirmCabAcceptanceViewModel
            {
                ServiceId          = serviceId,
                ServiceName        = service.ServiceName,
                CurrentCabId       = service.CabUser.CabId,
                CurrentCabName     = service.CabUser.Cab.CabName,
                SelectedCabId      = chosenCab.Id,
                SelectedCabName    = chosenCab.CabName,

                TransferViewModel = new ServiceTransferViewModel
                {
                    Service              = service,
                    ToCabName            = chosenCab.CabName,
                    IsServiceDetailsPage = false
                }
            };

            return View(confirmCabAcceptanceViewModel);
        }
        
        [HttpPost("confirm-cab-acceptance")]
        public async Task<IActionResult> ReassignmentSubmitted(int serviceId, int toCabId)
        {
        
            var serviceDto = await cabTransferService.GetServiceDetails(serviceId);
            var providerName = serviceDto.Provider.RegisteredName ?? "";

            var userDto = await userService.GetUser(UserEmail);
            
            var requestDto = new CabTransferRequestDto {
                ServiceId               = serviceId,               
                FromCabUserId           = serviceDto.CabUser.Id,
                ToCabId                 = toCabId,
                PreviousServiceStatus   = serviceDto.ServiceStatus,
                RequestManagement       = new RequestManagementDto {
                    RequestStatus = RequestStatusEnum.Pending,
                    CabId = toCabId,
                    InitiatedUserId = userDto.Id,
                    RequestType = RequestTypeEnum.CabTransfer,
                },
            };
            
            var result = await cabTransferService.SaveCabTransferRequest(requestDto, serviceDto.ProviderProfileId, serviceDto.ServiceName, providerName, UserEmail);
            
            if (!result.Success)
                throw new InvalidOperationException(string.IsNullOrWhiteSpace(result.ErrorMessage) ? "Unable to reassign CAB" : result.ErrorMessage);
            
            var dto = await cabTransferService.GetCabTransferDetails(serviceId);

            var reassignmentRequestSubmittedViewModel = new ReassignmentRequestSubmittedViewModel
            {
                ToCabName     = dto.ToCab.CabName,
                ProviderName  = dto.Service.Provider.RegisteredName,
                ServiceName   = dto.Service.ServiceName
            };
            
            return View("ReassignmentRequestSubmitted", reassignmentRequestSubmittedViewModel);
        }

        [HttpGet("cancel-reassign")]
        public async Task<IActionResult> CancelAssignmentRequest(int serviceId)
        {
            CabTransferRequestDto cabTransferRequestDto = await cabTransferService.GetCabTransferDetails(serviceId);
            return View(cabTransferRequestDto);
        }

        [HttpPost("reassign-cancelled")]
        public async Task<IActionResult> ReassignmentRequestCancelled(int cabTransferRequestId, int toCabId, int providerId, string serviceName, string providerName)
        {
            GenericResponse genericResponse = await cabTransferService.CancelCabTransferRequest(cabTransferRequestId, serviceName, providerName, toCabId, providerId, UserEmail);
            return View();
        }
    }
}
