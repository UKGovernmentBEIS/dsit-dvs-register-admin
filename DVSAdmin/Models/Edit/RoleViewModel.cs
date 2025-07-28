﻿using DVSAdmin.BusinessLogic.Models;
using DVSAdmin.Validations;

namespace DVSAdmin.Models
{
    public class RoleViewModel: ServiceSummaryBaseViewModel
    {
        public List<RoleDto>? AvailableRoles { get; set; }

        [EnsureMinimumCount(ErrorMessage = "Select the roles from the UK digital identity and attributes trust framework that apply to the provider's service")]
        public List<int>? SelectedRoleIds { get; set; }
        public List<RoleDto>? SelectedRoles { get; set; }    
      
    }
}
