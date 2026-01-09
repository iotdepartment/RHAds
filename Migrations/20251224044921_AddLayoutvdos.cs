using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RHAds.Migrations
{
    /// <inheritdoc />
    public partial class AddLayoutvdos : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Columnas",
                table: "Slides",
                newName: "Y");

            migrationBuilder.RenameColumn(
                name: "AlturaVH",
                table: "Slides",
                newName: "X");

            migrationBuilder.AddColumn<int>(
                name: "Height",
                table: "Slides",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "Width",
                table: "Slides",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Height",
                table: "Slides");

            migrationBuilder.DropColumn(
                name: "Width",
                table: "Slides");

            migrationBuilder.RenameColumn(
                name: "Y",
                table: "Slides",
                newName: "Columnas");

            migrationBuilder.RenameColumn(
                name: "X",
                table: "Slides",
                newName: "AlturaVH");
        }
    }
}
