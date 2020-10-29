using Microsoft.EntityFrameworkCore.Migrations;

namespace Yenviethue.Data.Migrations
{
    public partial class UdateOrder_ShippingInfo_OrderProduct : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Comments",
                table: "ShippingInfo");

            migrationBuilder.AddColumn<string>(
                name: "NameProduct",
                table: "OrderProduct",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "NameProduct",
                table: "OrderProduct");

            migrationBuilder.AddColumn<string>(
                name: "Comments",
                table: "ShippingInfo",
                type: "nvarchar(max)",
                nullable: true);
        }
    }
}
