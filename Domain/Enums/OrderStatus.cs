using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Enums
{
    public enum OrderStatus
    {
        NoPaper = 0,
        Awaiting = 1,   // “Чекає у пункті видачі”
        WithUser = 2    // “У користувача”
    }
}

