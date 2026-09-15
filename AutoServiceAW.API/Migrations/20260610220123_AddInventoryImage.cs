using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AutoServiceAW.API.Migrations
{
    /// <inheritdoc />
    public partial class AddInventoryImage : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "image",
                table: "inventory_items",
                type: "longtext",
                nullable: false)
                .Annotation("MySql:CharSet", "utf8mb4");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "image",
                table: "inventory_items");
        }
    }
}
