﻿using DVSAdmin.BusinessLogic.Models;
using System.ComponentModel.DataAnnotations;

namespace DVSAdmin.Models
{
    public class CertificateReviewViewModel
    {
        public int CertificateReviewId { get; set; }
        public ServiceDto? Service { get; set; }      

        [Required(ErrorMessage = "Enter the reasons for your decision")]
        public string? Comments { get; set; }

        [Required (ErrorMessage =  "Select an option")]
        public bool? InformationMatched { get; set; }
        public string? SubmitValidation { get; set; }
    }
}
