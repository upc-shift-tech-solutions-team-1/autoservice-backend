using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AutoServiceAW.API.Migrations
{
    /// <inheritdoc />
    public partial class AddEstimatedTime : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "estimated_time",
                table: "tasks",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "estimated_time",
                table: "tasks");
        }
    }
}
