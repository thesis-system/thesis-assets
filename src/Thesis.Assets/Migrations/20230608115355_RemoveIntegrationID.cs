using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Thesis.Assets.Migrations
{
    /// <inheritdoc />
    public partial class RemoveIntegrationID : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Categories_IntegrationId",
                table: "Categories");

            migrationBuilder.DropIndex(
                name: "IX_Assets_IntegrationId",
                table: "Assets");

            migrationBuilder.DropColumn(
                name: "IntegrationId",
                table: "Categories");

            migrationBuilder.DropColumn(
                name: "IntegrationId",
                table: "Assets");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "IntegrationId",
                table: "Categories",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "IntegrationId",
                table: "Assets",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

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
    }
}
