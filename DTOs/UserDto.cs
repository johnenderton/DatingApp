using System;

namespace DTOs
{
    public class UserDto
    {
        public string Username { get; set; }
        public string Token { get; set; }
        public string PhotoUrl { get; set; } // main photo
        public string KnownAs { get; set; }
    }
}