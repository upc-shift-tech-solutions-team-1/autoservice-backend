using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AutoServiceAW.API.Migrations
{
    /// <inheritdoc />
    public partial class UpdateMechanicEntity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "max_capacity",
                table: "mechanics",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "workshop_id",
                table: "mechanics",
                type: "longtext",
                nullable: false)
                .Annotation("MySql:CharSet", "utf8mb4");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "max_capacity",
                table: "mechanics");

            migrationBuilder.DropColumn(
                name: "workshop_id",
                table: "mechanics");
        }
    }
}
