using Microsoft.EntityFrameworkCore;

namespace NCoreUtils.OAuth2.Data
{
    public class RefreshTokenDbContext : DbContext
    {
        public RefreshTokenDbContext(DbContextOptions<RefreshTokenDbContext> options) : base(options) { }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.Entity<RefreshToken>(b =>
            {
                b.ToTable("refresh_token");
                b.HasKey(e => e.Id);
                b.Property(e => e.Id)
                    .ValueGeneratedOnAdd()
                    .HasColumnName("id");
                b.Property(e => e.Sub)
                    .HasMaxLength(128)
                    .IsUnicode(true)
                    .IsRequired(true)
                    .HasColumnName("sub");
                b.Property(e => e.Issuer)
                    .HasMaxLength(320)
                    .IsUnicode(true)
                    .IsRequired(true)
                    .HasColumnName("issuer");
                b.Property(e => e.Email)
                    .HasMaxLength(768)
                    .IsUnicode(true)
                    .IsRequired(false)
                    .HasColumnName("email");
                b.Property(e => e.Username)
                    .HasMaxLength(320)
                    .IsUnicode(true)
                    .IsRequired(true)
                    .HasColumnName("username");
                b.Property(e => e.Scopes)
                    .IsUnicode(true)
                    .IsRequired(true)
                    .HasColumnName("scopes");
                b.Property(e => e.IssuedAt)
                    .HasColumnName("issued_at");
                b.Property(e => e.ExpiresAt)
                    .HasColumnName("expires_at");
                b.HasIndex(e => new { e.Sub, e.IssuedAt });
            });

        }

    }
}