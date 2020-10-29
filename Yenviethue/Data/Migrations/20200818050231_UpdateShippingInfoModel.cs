using Microsoft.EntityFrameworkCore.Migrations;

namespace Yenviethue.Data.Migrations
{
    public partial class UpdateShippingInfoModel : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "OrderId",
                table: "ShippingInfo");

            migrationBuilder.AddColumn<int>(
                name: "PaymentMethod",
                table: "Order",
                nullable: false,
                defaultValue: 0);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PaymentMethod",
                table: "Order");

            migrationBuilder.AddColumn<int>(
                name: "OrderId",
                table: "ShippingInfo",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }
    }
}
