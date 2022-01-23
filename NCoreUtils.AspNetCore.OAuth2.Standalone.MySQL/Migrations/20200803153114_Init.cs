using Microsoft.EntityFrameworkCore.Migrations;
using MySql.EntityFrameworkCore.Metadata;

namespace NCoreUtils.AspNetCore.OAuth2.Migrations
{
    public partial class Init : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "refresh_token",
                columns: table => new
                {
                    id = table.Column<int>(nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn),
                    sub = table.Column<string>(maxLength: 128, nullable: false),
                    issuer = table.Column<string>(maxLength: 320, nullable: false),
                    email = table.Column<string>(maxLength: 768, nullable: true),
                    username = table.Column<string>(maxLength: 320, nullable: false),
                    scopes = table.Column<string>(nullable: false),
                    issued_at = table.Column<long>(nullable: false),
                    expires_at = table.Column<long>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_refresh_token", x => x.id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_refresh_token_sub_issued_at",
                table: "refresh_token",
                columns: new[] { "sub", "issued_at" });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "refresh_token");
        }
    }
}
