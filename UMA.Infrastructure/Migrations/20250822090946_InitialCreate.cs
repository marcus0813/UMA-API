using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace UMA.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    ID = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    FirstName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    LastName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Email = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Password = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ProfilePictureUrl = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    RefreshToken = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.ID);
                });

            migrationBuilder.InsertData(
                table: "Users",
                columns: new[] { "ID", "CreatedAt", "Email", "FirstName", "LastName", "Password", "ProfilePictureUrl", "RefreshToken", "UpdatedAt" },
                values: new object[,]
                {
                    { new Guid("72f04621-e341-453d-b596-7427da8bdd98"), new DateTime(2025, 8, 22, 9, 9, 46, 375, DateTimeKind.Utc).AddTicks(8607), "alfred.kok@email.com", "Alfred", "Kok", "$2b$10$oibRNoAhGh.iiaMCe7T7bubV4fa17PBchNop8WxO4EbSJCSElVJeK", "", "", null },
                    { new Guid("fa7cfbb5-911a-418d-a39d-101dbd10c00b"), new DateTime(2025, 8, 22, 9, 9, 46, 288, DateTimeKind.Utc).AddTicks(5267), "marcus.kok@email.com", "Marcus", "Kok", "$2b$10$keaG7Umfr2fr0XZ9PnF.wuiYFTzkrU1uLZyh4Z.tQE6YSn.QXlbfO", "", "", null }
                });

            migrationBuilder.CreateIndex(
                name: "IX_Users_Email",
                table: "Users",
                column: "Email",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Users");
        }
    }
}
