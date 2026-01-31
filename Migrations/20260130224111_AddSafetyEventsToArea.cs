using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RHAds.Migrations
{
    /// <inheritdoc />
    public partial class AddSafetyEventsToArea : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_SafetyEvents_Areas_AreaId",
                table: "SafetyEvents");

            migrationBuilder.AddForeignKey(
                name: "FK_SafetyEvents_Areas_AreaId",
                table: "SafetyEvents",
                column: "AreaId",
                principalTable: "Areas",
                principalColumn: "AreaId",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_SafetyEvents_Areas_AreaId",
                table: "SafetyEvents");

            migrationBuilder.AddForeignKey(
                name: "FK_SafetyEvents_Areas_AreaId",
                table: "SafetyEvents",
                column: "AreaId",
                principalTable: "Areas",
                principalColumn: "AreaId");
        }
    }
}
