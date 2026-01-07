using Microsoft.EntityFrameworkCore;

namespace ShareItems_WebApp.Entities
{
    public class UserContext : DbContext
    {
        public UserContext(DbContextOptions<UserContext> options) : base(options)
        {
        }
        public DbSet<UserCredential> UserCredentials { get; set; }
        public DbSet<Note> Notes { get; set; }
        public DbSet<NoteFile> NoteFiles { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            
            modelBuilder.Entity<UserCredential>().ToTable("UserCredentials");

            // Configure Note entity
            modelBuilder.Entity<Note>(entity =>
            {
                entity.ToTable("Notes");
                entity.HasKey(n => n.Id);
                entity.HasIndex(n => n.Code).IsUnique();
                entity.Property(n => n.Code).IsRequired().HasMaxLength(500);
                entity.Property(n => n.Content).HasMaxLength(50000);
                entity.Property(n => n.Pin).HasMaxLength(500);
            });

            // Configure NoteFile entity
            modelBuilder.Entity<NoteFile>(entity =>
            {
                entity.ToTable("NoteFiles");
                entity.HasKey(nf => nf.Id);
                entity.Property(nf => nf.FileName).IsRequired().HasMaxLength(255);
                entity.Property(nf => nf.StoredFileName).IsRequired().HasMaxLength(255);
                entity.Property(nf => nf.FileType).IsRequired().HasMaxLength(50);
                entity.Property(nf => nf.ContentType).IsRequired().HasMaxLength(100);
                entity.Property(nf => nf.FilePath).IsRequired().HasMaxLength(500);

                // Configure one-to-many relationship
                entity.HasOne(nf => nf.Note)
                    .WithMany(n => n.Files)
                    .HasForeignKey(nf => nf.NoteId)
                    .OnDelete(DeleteBehavior.Cascade);
            });
        }
    }
}
