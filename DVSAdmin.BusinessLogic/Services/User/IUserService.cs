﻿using DVSAdmin.BusinessLogic.Models;

namespace DVSAdmin.BusinessLogic.Services
{
    public interface IUserService
    {
        public Task<UserDto> GetUser(string email);

        public Task<List<string>> GetUserEmailsExcludingLoggedIn(string loggedInUserEmail);
        public Task UpdateUserProfile(string loggedInUserEmail, string profile);
    }
}
