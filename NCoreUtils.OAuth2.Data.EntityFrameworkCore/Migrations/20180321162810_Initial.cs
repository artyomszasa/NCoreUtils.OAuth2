using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using System;
using System.Collections.Generic;

namespace NCoreUtils.OAuth2.Data.EntityFrameworkCore.Migrations
{
    public partial class Initial : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ClientApplication",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.SerialColumn),
                    Description = table.Column<string>(maxLength: 8000, nullable: true),
                    Name = table.Column<string>(maxLength: 250, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ClientApplication", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Domain",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.SerialColumn),
                    ClientApplicationId = table.Column<int>(nullable: false),
                    DomainName = table.Column<string>(maxLength: 4000, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Domain", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Domain_ClientApplication_ClientApplicationId",
                        column: x => x.ClientApplicationId,
                        principalTable: "ClientApplication",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Permission",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.SerialColumn),
                    ClientApplicationId = table.Column<int>(nullable: false),
                    Description = table.Column<string>(maxLength: 4000, nullable: true),
                    Name = table.Column<string>(unicode: false, maxLength: 200, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Permission", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Permission_ClientApplication_ClientApplicationId",
                        column: x => x.ClientApplicationId,
                        principalTable: "ClientApplication",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "User",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.SerialColumn),
                    ClientApplicationId = table.Column<int>(nullable: false),
                    Created = table.Column<long>(nullable: false),
                    Email = table.Column<string>(maxLength: 500, nullable: false),
                    FamilyName = table.Column<string>(maxLength: 250, nullable: false),
                    GivenName = table.Column<string>(maxLength: 2000, nullable: true),
                    HonorificPrefix = table.Column<string>(maxLength: 60, nullable: true),
                    MiddleName = table.Column<string>(maxLength: 2000, nullable: true),
                    Password = table.Column<string>(maxLength: 128, nullable: false),
                    Salt = table.Column<string>(maxLength: 128, nullable: false),
                    State = table.Column<int>(nullable: false),
                    Updated = table.Column<long>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_User", x => x.Id);
                    table.ForeignKey(
                        name: "FK_User_ClientApplication_ClientApplicationId",
                        column: x => x.ClientApplicationId,
                        principalTable: "ClientApplication",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "AuthorizationCode",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    ExpiresAt = table.Column<long>(nullable: false),
                    IssuedAt = table.Column<long>(nullable: false),
                    RedirectUri = table.Column<string>(unicode: false, nullable: false),
                    Scopes = table.Column<string>(unicode: false, nullable: false),
                    UserId = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AuthorizationCode", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AuthorizationCode_User_UserId",
                        column: x => x.UserId,
                        principalTable: "User",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "RefreshToken",
                columns: table => new
                {
                    Id = table.Column<long>(nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.SerialColumn),
                    ExpiresAt = table.Column<long>(nullable: false),
                    IssuedAt = table.Column<long>(nullable: false),
                    LastUsed = table.Column<long>(nullable: true),
                    Scopes = table.Column<string>(unicode: false, nullable: false),
                    State = table.Column<int>(nullable: false),
                    UserId = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RefreshToken", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RefreshToken_User_UserId",
                        column: x => x.UserId,
                        principalTable: "User",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "UserPermission",
                columns: table => new
                {
                    UserId = table.Column<int>(nullable: false),
                    PermissionId = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserPermission", x => new { x.UserId, x.PermissionId });
                    table.ForeignKey(
                        name: "FK_UserPermission_Permission_PermissionId",
                        column: x => x.PermissionId,
                        principalTable: "Permission",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_UserPermission_User_UserId",
                        column: x => x.UserId,
                        principalTable: "User",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AuthorizationCode_UserId",
                table: "AuthorizationCode",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_Domain_ClientApplicationId",
                table: "Domain",
                column: "ClientApplicationId");

            migrationBuilder.CreateIndex(
                name: "IX_Domain_DomainName",
                table: "Domain",
                column: "DomainName",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Permission_ClientApplicationId",
                table: "Permission",
                column: "ClientApplicationId");

            migrationBuilder.CreateIndex(
                name: "IX_Permission_Name_ClientApplicationId",
                table: "Permission",
                columns: new[] { "Name", "ClientApplicationId" });

            migrationBuilder.CreateIndex(
                name: "IX_RefreshToken_UserId",
                table: "RefreshToken",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_RefreshToken_State_UserId_IssuedAt_ExpiresAt_Scopes",
                table: "RefreshToken",
                columns: new[] { "State", "UserId", "IssuedAt", "ExpiresAt", "Scopes" });

            migrationBuilder.CreateIndex(
                name: "IX_User_ClientApplictionId",
                table: "User",
                column: "ClientApplictionId");

            migrationBuilder.CreateIndex(
                name: "IX_User_Created",
                table: "User",
                column: "Created");

            migrationBuilder.CreateIndex(
                name: "IX_User_Updated",
                table: "User",
                column: "Updated");

            migrationBuilder.CreateIndex(
                name: "IX_User_Email_ClientApplictionId",
                table: "User",
                columns: new[] { "Email", "ClientApplictionId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_UserPermission_PermissionId",
                table: "UserPermission",
                column: "PermissionId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AuthorizationCode");

            migrationBuilder.DropTable(
                name: "Domain");

            migrationBuilder.DropTable(
                name: "RefreshToken");

            migrationBuilder.DropTable(
                name: "UserPermission");

            migrationBuilder.DropTable(
                name: "Permission");

            migrationBuilder.DropTable(
                name: "User");

            migrationBuilder.DropTable(
                name: "ClientApplication");
        }
    }
}
