using Microsoft.EntityFrameworkCore;
using NoteWeb.Entity.Model;

namespace NoteWeb.Entity;

public class MyDbContext : DbContext
{
    public DbSet<Note> Notes { get; set; }

    public MyDbContext(DbContextOptions<MyDbContext> options) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // 配置Note实体
        modelBuilder.Entity<Note>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Content)
                .HasMaxLength(30);
            // 索引配置
            entity.HasIndex(e => e.CreatedAt);

            entity.HasIndex(e => e.Id)
                .IsUnique();
        });
    }
}