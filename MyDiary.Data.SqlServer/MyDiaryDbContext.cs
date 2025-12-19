using Microsoft.EntityFrameworkCore;
using MyDiary.Domain.Entities;

namespace MyDiary.Data.SqlServer;

public class MyDiaryDbContext : DbContext
{
    public MyDiaryDbContext(DbContextOptions<MyDiaryDbContext> options)
        : base(options)
    {
    }

    public DbSet<User> Users => Set<User>();
    public DbSet<DiaryEntry> DiaryEntries => Set<DiaryEntry>();
    public DbSet<UserActivity> UserActivities => Set<UserActivity>();
    public DbSet<DiaryEntryActivity> DiaryEntryActivities => Set<DiaryEntryActivity>();
    public DbSet<Subscription> Subscriptions => Set<Subscription>();
    public DbSet<PaymentMethod> PaymentMethods => Set<PaymentMethod>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(x => x.Id);

            entity.Property(x => x.Login).IsRequired();
            entity.Property(x => x.Name).IsRequired();
            entity.Property(x => x.PasswordHash).IsRequired();

            entity.HasIndex(x => x.Login).IsUnique();
        });

        modelBuilder.Entity<DiaryEntry>(entity =>
        {
            entity.HasKey(x => x.Id);

            entity.Property(x => x.Title).IsRequired();
            entity.Property(x => x.Content).IsRequired();

            entity.HasIndex(x => x.UserId);
            entity.HasIndex(x => x.Date);
        });

        modelBuilder.Entity<UserActivity>(entity =>
        {
            entity.HasKey(x => x.Id);

            entity.Property(x => x.Name).IsRequired();
            entity.Property(x => x.Description).IsRequired();

            entity.HasIndex(x => x.UserId);

            // Опционально (по желанию): чтобы у одного пользователя не было 2 активностей с одинаковым именем
            //entity.HasIndex(x => new { x.UserId, x.Name }).IsUnique();
        });

        modelBuilder.Entity<DiaryEntryActivity>(entity =>
        {
            entity.HasKey(x => new { x.DiaryEntryId, x.UserActivityId });

            entity.HasOne(x => x.DiaryEntry)
                .WithMany(x => x.DiaryEntryActivities)
                .HasForeignKey(x => x.DiaryEntryId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(x => x.UserActivity)
                .WithMany(x => x.DiaryEntryActivities)
                .HasForeignKey(x => x.UserActivityId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<Subscription>(entity =>
        {
            entity.HasKey(x => x.Id);

            entity.Property(x => x.Plan).HasConversion<int>();
            entity.HasIndex(x => x.UserId);
        });

        modelBuilder.Entity<PaymentMethod>(entity =>
        {
            entity.HasKey(x => x.Id);

            entity.Property(x => x.Provider).IsRequired().HasMaxLength(32);
            entity.Property(x => x.Token).IsRequired().HasMaxLength(256);
            entity.Property(x => x.Last4).IsRequired().HasMaxLength(4);
            entity.Property(x => x.Brand).IsRequired().HasMaxLength(32);
            entity.Property(x => x.CreatedAtUtc).IsRequired();
            entity.Property(x => x.IsDefault).IsRequired();

            entity.HasIndex(x => x.Token).IsUnique();
            entity.HasIndex(x => x.UserId);
        });
    }
}