using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RHAds.Migrations
{
    /// <inheritdoc />
    public partial class AddUsuariosAreasSlidesRelations : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Slides_Areas_AreaDestinoId",
                table: "Slides");

            migrationBuilder.AddColumn<bool>(
                name: "EsInstitucional",
                table: "Areas",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateTable(
                name: "Usuarios",
                columns: table => new
                {
                    UsuarioId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Nombre = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Email = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    PasswordHash = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Rol = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    AreaId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Usuarios", x => x.UsuarioId);
                    table.ForeignKey(
                        name: "FK_Usuarios_Areas_AreaId",
                        column: x => x.AreaId,
                        principalTable: "Areas",
                        principalColumn: "AreaId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Usuarios_AreaId",
                table: "Usuarios",
                column: "AreaId");

            migrationBuilder.AddForeignKey(
                name: "FK_Slides_Areas_AreaDestinoId",
                table: "Slides",
                column: "AreaDestinoId",
                principalTable: "Areas",
                principalColumn: "AreaId",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Slides_Areas_AreaDestinoId",
                table: "Slides");

            migrationBuilder.DropTable(
                name: "Usuarios");

            migrationBuilder.DropColumn(
                name: "EsInstitucional",
                table: "Areas");

            migrationBuilder.AddForeignKey(
                name: "FK_Slides_Areas_AreaDestinoId",
                table: "Slides",
                column: "AreaDestinoId",
                principalTable: "Areas",
                principalColumn: "AreaId");
        }
    }
}
