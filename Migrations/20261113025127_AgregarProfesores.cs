using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace UdiAyudaProfe.Migrations
{
    /// <inheritdoc />
    public partial class AgregarProfesores : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ProfesorId",
                table: "Cursos",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "Profesores",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Nombre = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Email = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    Especialidad = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Profesores", x => x.Id);
                });

            migrationBuilder.UpdateData(
                table: "Cursos",
                keyColumn: "Id",
                keyValue: 1,
                column: "ProfesorId",
                value: 1);

            migrationBuilder.UpdateData(
                table: "Cursos",
                keyColumn: "Id",
                keyValue: 2,
                column: "ProfesorId",
                value: 2);

            migrationBuilder.UpdateData(
                table: "Cursos",
                keyColumn: "Id",
                keyValue: 3,
                column: "ProfesorId",
                value: 3);

            migrationBuilder.InsertData(
                table: "Profesores",
                columns: new[] { "Id", "Email", "Especialidad", "Nombre" },
                values: new object[,]
                {
                    { 1, "laura.fernandez@udi.edu", "Programación", "Laura Fernández" },
                    { 2, "jorge.medina@udi.edu", "Bases de Datos", "Jorge Medina" },
                    { 3, "sofia.ramirez@udi.edu", "Desarrollo Web", "Sofía Ramírez" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_Cursos_ProfesorId",
                table: "Cursos",
                column: "ProfesorId");

            migrationBuilder.CreateIndex(
                name: "IX_Profesores_Email",
                table: "Profesores",
                column: "Email",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Cursos_Profesores_ProfesorId",
                table: "Cursos",
                column: "ProfesorId",
                principalTable: "Profesores",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Cursos_Profesores_ProfesorId",
                table: "Cursos");

            migrationBuilder.DropTable(
                name: "Profesores");

            migrationBuilder.DropIndex(
                name: "IX_Cursos_ProfesorId",
                table: "Cursos");

            migrationBuilder.DropColumn(
                name: "ProfesorId",
                table: "Cursos");
        }
    }
}
