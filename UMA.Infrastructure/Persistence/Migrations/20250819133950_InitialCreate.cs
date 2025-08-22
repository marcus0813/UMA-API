using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

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
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.ID);
                });

            migrationBuilder.InsertData(
                table: "Users",
                columns: new[] { "ID", "CreatedAt", "Email", "FirstName", "LastName", "Password", "ProfilePictureUrl", "UpdatedAt" },
                values: new object[] { new Guid("4c13bdf2-12cb-486d-bd2c-24c23d916a6d"), new DateTime(2025, 8, 19, 13, 39, 49, 964, DateTimeKind.Utc).AddTicks(7937), "marcus.kok@email.com", "Marcus", "Kok", "$2b$10$LB9HbCHkEleGIDuoRrpyGe58krqx4bMjIF.a5SJWqPaRy7saOFhoi", "", new DateTime(2025, 8, 19, 13, 39, 49, 877, DateTimeKind.Utc).AddTicks(944) });

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
