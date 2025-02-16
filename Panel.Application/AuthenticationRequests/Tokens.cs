﻿using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Panel.Application.AuthenticationRequests
{
    public class Tokens: IRequest<IActionResult>
    {
        public string AccessToken { get; set; }
        public string RefreshToken { get; set; }
    }
}
