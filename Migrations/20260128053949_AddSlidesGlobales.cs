using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RHAds.Migrations
{
    /// <inheritdoc />
    public partial class AddSlidesGlobales : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "AreaDestinoId",
                table: "Slides",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "EsGlobal",
                table: "Slides",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateIndex(
                name: "IX_Slides_AreaDestinoId",
                table: "Slides",
                column: "AreaDestinoId");

            migrationBuilder.CreateIndex(
                name: "IX_SafetyEvents_AreaId",
                table: "SafetyEvents",
                column: "AreaId");

            migrationBuilder.AddForeignKey(
                name: "FK_SafetyEvents_Areas_AreaId",
                table: "SafetyEvents",
                column: "AreaId",
                principalTable: "Areas",
                principalColumn: "AreaId");

            migrationBuilder.AddForeignKey(
                name: "FK_Slides_Areas_AreaDestinoId",
                table: "Slides",
                column: "AreaDestinoId",
                principalTable: "Areas",
                principalColumn: "AreaId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_SafetyEvents_Areas_AreaId",
                table: "SafetyEvents");

            migrationBuilder.DropForeignKey(
                name: "FK_Slides_Areas_AreaDestinoId",
                table: "Slides");

            migrationBuilder.DropIndex(
                name: "IX_Slides_AreaDestinoId",
                table: "Slides");

            migrationBuilder.DropIndex(
                name: "IX_SafetyEvents_AreaId",
                table: "SafetyEvents");

            migrationBuilder.DropColumn(
                name: "AreaDestinoId",
                table: "Slides");

            migrationBuilder.DropColumn(
                name: "EsGlobal",
                table: "Slides");
        }
    }
}
