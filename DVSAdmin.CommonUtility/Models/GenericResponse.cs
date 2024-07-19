﻿namespace DVSAdmin.CommonUtility.Models
{
    public class GenericResponse
    {
        public bool Success { get; set; }
        public bool EmailSent { get; set; }
        public string? Data { get; set; }
        public int InstanceId { get; set; }
        public string ErrorMessage { get; set; }
    }
}
