using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Forto4kiParser.Migrations
{
    /// <inheritdoc />
    public partial class AddMinCount : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "MinCount",
                table: "Filters",
                type: "int",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "MinCount",
                table: "Filters");
        }
    }
}
