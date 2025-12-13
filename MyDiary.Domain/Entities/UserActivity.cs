using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyDiary.Domain.Entities;

public class UserActivity
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }

    public required string Name { get; set; }
    public required string Description { get; set; }

    public List<DiaryEntryActivity> DiaryEntryActivities { get; set; } = new();
}
