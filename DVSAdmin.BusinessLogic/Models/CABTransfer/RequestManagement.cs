﻿using DVSAdmin.CommonUtility.Models.Enums;

namespace DVSAdmin.BusinessLogic.Models
{
    public class RequestManagementDto
    {       
        public string Id { get; set; }      
        public int InitiatedUserId { get; set; }
        public UserDto User { get; set; }       
        public int CabId { get; set; }      
        public RequestTypeEnum RequestType { get; set; }
        public RequestStatusEnum RequestStatus { get; set; }
        public DateTime ModifiedTime { get; set; }
    }
}
