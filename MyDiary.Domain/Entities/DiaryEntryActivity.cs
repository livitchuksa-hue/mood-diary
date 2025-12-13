using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyDiary.Domain.Entities;

public class DiaryEntryActivity
{
    public Guid DiaryEntryId { get; set; }
    public DiaryEntry DiaryEntry { get; set; } = null!;

    public Guid UserActivityId { get; set; }
    public UserActivity UserActivity { get; set; } = null!;
}