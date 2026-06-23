using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Retainica.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddAiSettingsModel : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Model",
                table: "UserAiSettings",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Model",
                table: "UserAiSettings");
        }
    }
}
