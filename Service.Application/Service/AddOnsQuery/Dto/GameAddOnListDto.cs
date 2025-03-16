using System;
using System.Collections.Generic;
using System.Linq;

namespace Service.Application.Service.AddOnsQuery.Dto
{
    public class GameAddOnListDto
    {
        public Guid ProductId { get; set; }
        public string AddOnName { get; set; }
        public string GameName { get; set; }
        public string Image { get; set; }
        public string Platform { get; set; }
        public decimal Price { get; set; }
        public decimal JPrice { get; set; }
        public string DiscountPercent { get; set; }
    }

}
