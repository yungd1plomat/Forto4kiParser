using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Forto4kiParser.Migrations
{
    /// <inheritdoc />
    public partial class AddExclusions : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Exclusions",
                table: "Filters",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Exclusions",
                table: "Filters");
        }
    }
}
