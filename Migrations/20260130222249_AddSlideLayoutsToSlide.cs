using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RHAds.Migrations
{
    /// <inheritdoc />
    public partial class AddSlideLayoutsToSlide : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_SlideLayouts_Slides_SlideId",
                table: "SlideLayouts");

            migrationBuilder.AddForeignKey(
                name: "FK_SlideLayouts_Slides_SlideId",
                table: "SlideLayouts",
                column: "SlideId",
                principalTable: "Slides",
                principalColumn: "SlideId",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_SlideLayouts_Slides_SlideId",
                table: "SlideLayouts");

            migrationBuilder.AddForeignKey(
                name: "FK_SlideLayouts_Slides_SlideId",
                table: "SlideLayouts",
                column: "SlideId",
                principalTable: "Slides",
                principalColumn: "SlideId",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
