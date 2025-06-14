using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Service.Application.Service.OrderQuery.Dto
{
    public class TransactionsHistoryDto
    {
        public string TgId { get; set; }
        public string OrderCode { get; set; }
        public decimal JoyAmount { get; set; }
        public DateTime DateCreate { get; set; }

    }
}
