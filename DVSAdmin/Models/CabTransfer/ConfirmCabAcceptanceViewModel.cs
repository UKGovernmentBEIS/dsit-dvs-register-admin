﻿namespace DVSAdmin.Models.CabTransfer
{
    public class ConfirmCabAcceptanceViewModel
    {
        public int    ServiceId   { get; set; }
        public string ServiceName { get; set; }
        public int    CurrentCabId   { get; set; }
        public string CurrentCabName { get; set; }
        public int    SelectedCabId   { get; set; }
        public string SelectedCabName { get; set; }
        public ServiceTransferViewModel TransferViewModel { get; set; }
    }
}


