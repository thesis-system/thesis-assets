using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Thesis.Assets.Migrations
{
    /// <inheritdoc />
    public partial class AddIndexToIntegrationId : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_Categories_IntegrationId",
                table: "Categories",
                column: "IntegrationId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Assets_IntegrationId",
                table: "Assets",
                column: "IntegrationId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Categories_IntegrationId",
                table: "Categories");

            migrationBuilder.DropIndex(
                name: "IX_Assets_IntegrationId",
                table: "Assets");
        }
    }
}
