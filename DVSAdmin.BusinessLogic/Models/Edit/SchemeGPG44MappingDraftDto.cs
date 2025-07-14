﻿using DVSAdmin.BusinessLogic.Models.CertificateReview;

namespace DVSAdmin.BusinessLogic.Models
{
    public class SchemeGPG44MappingDraftDto
    {       
        public int Id { get; set; }    
        public int QualityLevelId { get; set; }
        public QualityLevelDto QualityLevel { get; set; }     
        public int ServiceSupSchemeMappingDraftId { get; set; }
        public ServiceSupSchemeMappingDraftDto ServiceSupSchemeMappingDraft { get; set; }
    }
}
