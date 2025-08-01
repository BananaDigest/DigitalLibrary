﻿using System;
using System.Collections.Generic;
using Domain.Enums;

namespace Domain.Entities
{
    public class User
    {
        public int Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }

        public UserRole Role { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }

        public ICollection<Order> Orders { get; set; } = new List<Order>();
    }
}
