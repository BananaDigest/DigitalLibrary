using BLL.DTOs;
using Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BLL.Factory
{
    public abstract class BookFactory
    {
        public abstract Book Create(ActionBookDto dto);
    }
}
