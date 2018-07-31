using Microsoft.EntityFrameworkCore;

namespace NCoreUtils.OAuth2.Data
{
    class OAuth2DbContext : DbContext
    {
        public OAuth2DbContext(DbContextOptions<OAuth2DbContext> options) : base(options) { }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            builder.Entity<File>(b =>
            {
                b.HasKey(e => e.Id);
                b.HasAlternateKey(e => e.IdName);
                b.HasIndex(e => e.Created);
                b.HasIndex(e => e.Updated);
                b.Property(e => e.IdName).HasMaxLength(320).IsUnicode(false).IsRequired(true);
                b.Property(e => e.OriginalName).IsUnicode(true).IsRequired(true);
                b.Property(e => e.MediaType).HasMaxLength(320).IsUnicode(false).IsRequired(true);
            });

            builder.Entity<ClientApplication>(b =>
            {
                b.HasKey(e => e.Id);
                b.Property(e => e.Name).HasMaxLength(250).IsUnicode(true).IsRequired(true);
                b.Property(e => e.Description).HasMaxLength(8000).IsUnicode(true).IsRequired(false);
            });

            builder.Entity<Domain>(b =>
            {
                b.HasKey(e => e.Id);
                b.Property(e => e.DomainName).HasMaxLength(4000).IsUnicode(true).IsRequired(false);
                b.HasIndex(e => e.DomainName).IsUnique(true);
                b.HasOne(e => e.ClientApplication).WithMany(e => e.Domains).HasForeignKey(e => e.ClientApplicationId).OnDelete(DeleteBehavior.Cascade);
            });

            builder.Entity<User>(b =>
            {
                b.HasKey(e => e.Id);
                b.Property(e => e.HonorificPrefix).HasMaxLength(60).IsUnicode(true).IsRequired(false);
                b.Property(e => e.FamilyName).HasMaxLength(250).IsUnicode(true).IsRequired(true);
                b.Property(e => e.GivenName).HasMaxLength(2000).IsUnicode(true).IsRequired(false);
                b.Property(e => e.MiddleName).HasMaxLength(2000).IsUnicode(true).IsRequired(false);
                b.Property(e => e.Email).HasMaxLength(500).IsUnicode(true).IsRequired(true);
                b.Property(e => e.Salt).HasMaxLength(128).IsUnicode(true).IsRequired(true);
                b.Property(e => e.Password).HasMaxLength(128).IsUnicode(true).IsRequired(true);
                b.HasIndex(e => new { e.Email, e.ClientApplicationId }).IsUnique(true);
                b.HasIndex(e => e.Created);
                b.HasIndex(e => e.Updated);
                b.HasOne(e => e.ClientApplication).WithMany(e => e.Users).HasForeignKey(e => e.ClientApplicationId).OnDelete(DeleteBehavior.Restrict);
                b.HasOne(e => e.Avatar).WithMany().HasForeignKey(e => e.AvatarId).IsRequired(false).OnDelete(DeleteBehavior.SetNull);
            });

            builder.Entity<Permission>(b =>
            {
                b.HasKey(e => e.Id);
                b.Property(e => e.Name).HasMaxLength(200).IsUnicode(false).IsRequired(true);
                b.Property(e => e.Description).HasMaxLength(4000).IsUnicode(true).IsRequired(false);
                b.HasIndex(e => new { e.Name, e.ClientApplicationId });
                b.HasOne(e => e.ClientApplication).WithMany(e => e.Permissions).HasForeignKey(e => e.ClientApplicationId).OnDelete(DeleteBehavior.Restrict);
            });

            builder.Entity<UserPermission>(b =>
            {
                b.HasKey(e => new { e.UserId, e.PermissionId });
                b.HasOne(e => e.Permission).WithMany(e => e.Users).HasForeignKey(e => e.PermissionId).OnDelete(DeleteBehavior.Cascade);
                b.HasOne(e => e.User).WithMany(e => e.Permissions).HasForeignKey(e => e.UserId).OnDelete(DeleteBehavior.Cascade);
            });

            builder.Entity<RefreshToken>(b =>
            {
                b.HasKey(e => e.Id);
                b.Property(e => e.Scopes).IsUnicode(false).IsRequired(true);
                b.HasIndex(e => new { e.State, e.UserId, e.IssuedAt, e.ExpiresAt, e.Scopes });
                b.HasOne(e => e.User).WithMany(e => e.RefreshTokens).HasForeignKey(e => e.UserId).OnDelete(DeleteBehavior.Cascade);
            });

            builder.Entity<AuthorizationCode>(b =>
            {
                b.HasKey(e => e.Id);
                b.Property(e => e.Id).ValueGeneratedNever();
                b.Property(e => e.Scopes).IsUnicode(false).IsRequired(true);
                b.Property(e => e.RedirectUri).IsUnicode(false).IsRequired(true);
                b.HasOne(e => e.User).WithMany(e => e.AuthorizationCodes).HasForeignKey(e => e.UserId).OnDelete(DeleteBehavior.Cascade);
            });
        }
    }
}