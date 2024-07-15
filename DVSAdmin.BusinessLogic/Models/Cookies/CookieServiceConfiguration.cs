﻿namespace DVSAdmin.BusinessLogic.Models.Cookies;

public class CookieServiceConfiguration
{
    public const string ConfigSection = "Cookies";
    public string CookieSettingsCookieName { get; set; }
    public int CurrentCookieMessageVersion { get; set; }
    public int DefaultDaysUntilExpiry { get; set; }
}