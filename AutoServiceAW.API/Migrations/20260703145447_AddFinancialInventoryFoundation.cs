using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AutoServiceAW.API.Migrations
{
    /// <inheritdoc />
    public partial class AddFinancialInventoryFoundation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "labor_cost",
                table: "tasks",
                type: "decimal(10,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "materials_purchase_cost",
                table: "tasks",
                type: "decimal(10,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<string>(
                name: "brand",
                table: "task_parts",
                type: "varchar(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "")
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<decimal>(
                name: "purchase_price",
                table: "task_parts",
                type: "decimal(10,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<string>(
                name: "quality_tier",
                table: "task_parts",
                type: "varchar(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "STANDARD")
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "presentation",
                table: "inventory_items",
                type: "varchar(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "")
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<decimal>(
                name: "purchase_price",
                table: "inventory_items",
                type: "decimal(10,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<string>(
                name: "quality_tier",
                table: "inventory_items",
                type: "varchar(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "STANDARD")
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "specification",
                table: "inventory_items",
                type: "varchar(120)",
                maxLength: 120,
                nullable: false,
                defaultValue: "")
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "unit_measure",
                table: "inventory_items",
                type: "varchar(30)",
                maxLength: 30,
                nullable: false,
                defaultValue: "UNIT")
                .Annotation("MySql:CharSet", "utf8mb4");
            
            migrationBuilder.Sql(
                "UPDATE inventory_items " +
                "SET purchase_price = ROUND(unit_price * 0.70, 2) " +
                "WHERE purchase_price = 0"
            );

            migrationBuilder.Sql(
                "UPDATE tasks " +
                "SET labor_cost = ROUND(labor_price * 0.50, 2) " +
                "WHERE labor_cost = 0"
            );

            migrationBuilder.Sql(
                "UPDATE task_parts tp " +
                "INNER JOIN inventory_items i ON i.id = tp.inventory_item_id " +
                "SET tp.purchase_price = i.purchase_price, " +
                "tp.brand = i.brand, " +
                "tp.quality_tier = i.quality_tier"
            );

            migrationBuilder.Sql(
                "UPDATE tasks t " +
                "SET materials_cost = COALESCE((" +
                "SELECT SUM(tp.unit_price * tp.quantity) " +
                "FROM task_parts tp " +
                "WHERE tp.task_id = t.id), 0)"
            );

            migrationBuilder.Sql(
                "UPDATE tasks t " +
                "SET materials_purchase_cost = COALESCE((" +
                "SELECT SUM(tp.purchase_price * tp.quantity) " +
                "FROM task_parts tp " +
                "WHERE tp.task_id = t.id), 0)"
            );
            migrationBuilder.Sql(
                "UPDATE work_orders wo " +
                "SET price = COALESCE((" +
                "SELECT SUM(t.labor_price + t.materials_cost) " +
                "FROM tasks t " +
                "WHERE t.work_order_id = wo.id), wo.price) " +
                "WHERE EXISTS (" +
                "SELECT 1 " +
                "FROM tasks t2 " +
                "WHERE t2.work_order_id = wo.id)"
            );
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "labor_cost",
                table: "tasks");

            migrationBuilder.DropColumn(
                name: "materials_purchase_cost",
                table: "tasks");

            migrationBuilder.DropColumn(
                name: "brand",
                table: "task_parts");

            migrationBuilder.DropColumn(
                name: "purchase_price",
                table: "task_parts");

            migrationBuilder.DropColumn(
                name: "quality_tier",
                table: "task_parts");

            migrationBuilder.DropColumn(
                name: "presentation",
                table: "inventory_items");

            migrationBuilder.DropColumn(
                name: "purchase_price",
                table: "inventory_items");

            migrationBuilder.DropColumn(
                name: "quality_tier",
                table: "inventory_items");

            migrationBuilder.DropColumn(
                name: "specification",
                table: "inventory_items");

            migrationBuilder.DropColumn(
                name: "unit_measure",
                table: "inventory_items");
        }
    }
}
