﻿using System.Collections.Generic;
using System.Security.Claims;

namespace Thismaker.Aba.Server.Authentication
{
    public interface IAbaTokenKeeper
    {
        public bool CheckIfTokenExists(string userId, string token);

        public List<string> GetRequiredClaims();

        public List<Claim> MakeClaims<T>(T userId);
    }
}
