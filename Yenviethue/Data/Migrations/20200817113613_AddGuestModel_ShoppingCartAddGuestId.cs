using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Yenviethue.Data.Migrations
{
    public partial class AddGuestModel_ShoppingCartAddGuestId : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ShippingInfo_Order_OrderId",
                table: "ShippingInfo");

            migrationBuilder.DropForeignKey(
                name: "FK_ShippingInfo_AspNetUsers_UserId",
                table: "ShippingInfo");

            migrationBuilder.DropIndex(
                name: "IX_ShippingInfo_OrderId",
                table: "ShippingInfo");

            migrationBuilder.DropIndex(
                name: "IX_ShippingInfo_UserId",
                table: "ShippingInfo");

            migrationBuilder.AddColumn<string>(
                name: "GuestId",
                table: "ShoppingCart",
                nullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "UserId",
                table: "ShippingInfo",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)",
                oldNullable: true);

            migrationBuilder.CreateTable(
                name: "Guest",
                columns: table => new
                {
                    Id = table.Column<string>(nullable: false),
                    ExpirationDate = table.Column<DateTime>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Guest", x => x.Id);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Guest");

            migrationBuilder.DropColumn(
                name: "GuestId",
                table: "ShoppingCart");

            migrationBuilder.AlterColumn<string>(
                name: "UserId",
                table: "ShippingInfo",
                type: "nvarchar(450)",
                nullable: true,
                oldClrType: typeof(string),
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_ShippingInfo_OrderId",
                table: "ShippingInfo",
                column: "OrderId");

            migrationBuilder.CreateIndex(
                name: "IX_ShippingInfo_UserId",
                table: "ShippingInfo",
                column: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_ShippingInfo_Order_OrderId",
                table: "ShippingInfo",
                column: "OrderId",
                principalTable: "Order",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ShippingInfo_AspNetUsers_UserId",
                table: "ShippingInfo",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
