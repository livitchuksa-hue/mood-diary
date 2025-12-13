using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyDiary.Domain.Entities
{
    public class Activity
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        
        public string Name { get; set; }

        public string Description { get; set; }
        public Guid DiaryEntryId { get; set; }
        public DiaryEntry DiaryEntry { get; set; }
    }
}
