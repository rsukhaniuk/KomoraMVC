using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Komora.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class AddUserColumnInventoryTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "UserId",
                table: "Inventory",
                type: "nvarchar(450)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "IX_Inventory_UserId",
                table: "Inventory",
                column: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_Inventory_AspNetUsers_UserId",
                table: "Inventory",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Inventory_AspNetUsers_UserId",
                table: "Inventory");

            migrationBuilder.DropIndex(
                name: "IX_Inventory_UserId",
                table: "Inventory");

            migrationBuilder.DropColumn(
                name: "UserId",
                table: "Inventory");
        }
    }
}
