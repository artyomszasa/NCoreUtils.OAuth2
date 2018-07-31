using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

namespace NCoreUtils.OAuth2.Data.EntityFrameworkCore.Migrations
{
    public partial class AddAvatar : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "AvatarId",
                table: "User",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "File",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.SerialColumn),
                    Created = table.Column<long>(nullable: false),
                    Updated = table.Column<long>(nullable: false),
                    IdName = table.Column<string>(unicode: false, maxLength: 320, nullable: false),
                    OriginalName = table.Column<string>(nullable: false),
                    MediaType = table.Column<string>(unicode: false, maxLength: 320, nullable: false),
                    State = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_File", x => x.Id);
                    table.UniqueConstraint("AK_File_IdName", x => x.IdName);
                });

            migrationBuilder.CreateIndex(
                name: "IX_User_AvatarId",
                table: "User",
                column: "AvatarId");

            migrationBuilder.CreateIndex(
                name: "IX_File_Created",
                table: "File",
                column: "Created");

            migrationBuilder.CreateIndex(
                name: "IX_File_Updated",
                table: "File",
                column: "Updated");

            migrationBuilder.AddForeignKey(
                name: "FK_User_File_AvatarId",
                table: "User",
                column: "AvatarId",
                principalTable: "File",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_User_File_AvatarId",
                table: "User");

            migrationBuilder.DropTable(
                name: "File");

            migrationBuilder.DropIndex(
                name: "IX_User_AvatarId",
                table: "User");

            migrationBuilder.DropColumn(
                name: "AvatarId",
                table: "User");
        }
    }
}
