using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Thesis.Assets.Migrations
{
    /// <inheritdoc />
    public partial class AddPlaceData : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<double>(
                name: "Latitude",
                table: "Assets",
                type: "double precision",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "Longitude",
                table: "Assets",
                type: "double precision",
                nullable: false,
                defaultValue: 0.0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Latitude",
                table: "Assets");

            migrationBuilder.DropColumn(
                name: "Longitude",
                table: "Assets");
        }
    }
}
