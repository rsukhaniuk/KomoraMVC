using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Komora.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class UpdateMenuTable2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Menu_Meals_MealId",
                table: "Menu");

            migrationBuilder.DropForeignKey(
                name: "FK_Menu_Recipes_RecipeId",
                table: "Menu");

            migrationBuilder.DropIndex(
                name: "IX_Menu_MealId",
                table: "Menu");

            migrationBuilder.DropIndex(
                name: "IX_Menu_RecipeId",
                table: "Menu");

            migrationBuilder.DropColumn(
                name: "MealId",
                table: "Menu");

            migrationBuilder.DropColumn(
                name: "RecipeId",
                table: "Menu");

            migrationBuilder.DropColumn(
                name: "Servings",
                table: "Menu");

            migrationBuilder.AddColumn<string>(
                name: "UserId",
                table: "Menu",
                type: "nvarchar(450)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "IX_Menu_UserId",
                table: "Menu",
                column: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_Menu_AspNetUsers_UserId",
                table: "Menu",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Menu_AspNetUsers_UserId",
                table: "Menu");

            migrationBuilder.DropIndex(
                name: "IX_Menu_UserId",
                table: "Menu");

            migrationBuilder.DropColumn(
                name: "UserId",
                table: "Menu");

            migrationBuilder.AddColumn<int>(
                name: "MealId",
                table: "Menu",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "RecipeId",
                table: "Menu",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "Servings",
                table: "Menu",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_Menu_MealId",
                table: "Menu",
                column: "MealId");

            migrationBuilder.CreateIndex(
                name: "IX_Menu_RecipeId",
                table: "Menu",
                column: "RecipeId");

            migrationBuilder.AddForeignKey(
                name: "FK_Menu_Meals_MealId",
                table: "Menu",
                column: "MealId",
                principalTable: "Meals",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Menu_Recipes_RecipeId",
                table: "Menu",
                column: "RecipeId",
                principalTable: "Recipes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
