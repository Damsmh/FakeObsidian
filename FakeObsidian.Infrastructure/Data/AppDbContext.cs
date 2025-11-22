using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using FakeObsidian.Domain.Entities;

namespace FakeObsidian.Infrastructure.Data
{
    public partial class AppDbContext(DbContextOptions<AppDbContext> options) : IdentityDbContext<AppUser>(options)
    {
        public DbSet<RefreshToken> RefreshTokens { get; set; }
        public DbSet<Post> Posts { get; set; }
        public DbSet<PostBlock> PostBlocks { get; set; }
        public DbSet<PostPermission> PostPermissions { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);
            builder.Entity<RefreshToken>(b =>
            {
                b.HasKey(rt => rt.Id);
                b.Property(rt => rt.Token).IsRequired();
                b.HasOne(rt => rt.User)
                 .WithMany(u => u.RefreshTokens!)
                 .HasForeignKey(rt => rt.UserId)
                 .OnDelete(DeleteBehavior.Cascade);
            });

            builder.Entity<PostBlock>()
                .HasOne(b => b.Post)
                .WithMany(p => p.Blocks)
                .HasForeignKey(b => b.PostId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<PostPermission>()
                .HasOne(p => p.Post)
                .WithMany(po => po.Permissions)
                .HasForeignKey(p => p.PostId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<PostPermission>()
                .HasOne(p => p.User)
                .WithMany(u => u.PostPermissions)
                .HasForeignKey(p => p.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<PostPermission>()
                .HasOne(p => p.GrantedBy)
                .WithMany(u => u.GrantedPermissions)
                .HasForeignKey(p => p.GrantedById)
                .OnDelete(DeleteBehavior.SetNull);

            builder.Entity<PostPermission>()
                .HasIndex(p => new { p.PostId, p.UserId })
                .IsUnique();

            builder.Entity<PostBlock>()
                .HasIndex(b => b.PostId);

            builder.Entity<PostPermission>()
                .HasIndex(p => p.UserId);
        }
    }
}
