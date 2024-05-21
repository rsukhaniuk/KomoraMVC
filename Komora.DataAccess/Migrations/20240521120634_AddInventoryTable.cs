using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Komora.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class AddInventoryTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Inventory",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ProductId = table.Column<int>(type: "int", nullable: false),
                    ExpirationDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    PlanDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    PlanQuantity = table.Column<double>(type: "float", nullable: false),
                    IncomeDate = table.Column<double>(type: "float", nullable: false),
                    IncomeQuantity = table.Column<double>(type: "float", nullable: false),
                    Remaindate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    RemainQuantity = table.Column<double>(type: "float", nullable: false),
                    WasteDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    WasteQuantity = table.Column<double>(type: "float", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Inventory", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Inventory_Products_ProductId",
                        column: x => x.ProductId,
                        principalTable: "Products",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Inventory_ProductId",
                table: "Inventory",
                column: "ProductId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Inventory");
        }
    }
}
