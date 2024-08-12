using ChatDb.Models;
using Microsoft.EntityFrameworkCore;

namespace ChatDb
{
    public class ChatContext : DbContext
    {
        public DbSet<UserEntity> Users { get; set; }
        public DbSet<MessageEntity> Messages { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseLazyLoadingProxies().UseNpgsql("Host=localhost;Username=postgres;Password=example;Database=ChatDB");
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {            
            modelBuilder.Entity<UserEntity>(entity =>
            {
                entity.HasKey(x => x.Id).HasName("user_pkey");
                entity.ToTable("Users");
                entity.Property(x => x.Id).HasColumnName("id").ValueGeneratedOnAdd();
                entity.Property(x => x.Name).HasMaxLength(255).HasColumnName("name");
                entity.HasIndex(x => x.Name).IsUnique();
            });

            modelBuilder.Entity<MessageEntity>(entity =>
            {
                entity.HasKey(x => x.Id)
                .HasName("message_pkey");
                entity.ToTable("Messages");
                entity.Property(x => x.Id).HasColumnName("id").ValueGeneratedOnAdd();        
                entity.Property(x => x.Text).HasColumnName("text");
                entity.Property(x => x.SenderId).HasColumnName("from_user_id");
                entity.Property(x => x.RecipientId).HasColumnName("to_user_id");
                entity.Property(x => x.Received).HasColumnName("received");
                entity.Property(x => x.CreatedAt).HasColumnName("created_at");
                entity.HasOne<UserEntity>().WithMany().HasForeignKey(x => x.SenderId);
                entity.HasOne<UserEntity>().WithMany().HasForeignKey(x => x.RecipientId);
            });

            base.OnModelCreating(modelBuilder);
        }
    }
}
