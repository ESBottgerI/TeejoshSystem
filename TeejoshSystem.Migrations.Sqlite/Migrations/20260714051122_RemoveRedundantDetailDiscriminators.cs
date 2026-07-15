using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TeejoshSystem.Migrations.Sqlite.Migrations
{
    /// <inheritdoc />
    public partial class RemoveRedundantDetailDiscriminators : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // EF reconstruye las tablas para DropColumn y puede reintroducir esta columna
            // desde las anotaciones de herencia. SQLite moderno soporta DROP COLUMN nativo.
            migrationBuilder.Sql("ALTER TABLE \"varios\" DROP COLUMN \"Discriminator\";");
            migrationBuilder.Sql("ALTER TABLE \"toy\" DROP COLUMN \"Discriminator\";");
            migrationBuilder.Sql("ALTER TABLE \"tcg\" DROP COLUMN \"Discriminator\";");
            migrationBuilder.Sql("ALTER TABLE \"hot_wheels\" DROP COLUMN \"Discriminator\";");
            migrationBuilder.Sql("ALTER TABLE \"funko\" DROP COLUMN \"Discriminator\";");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Discriminator",
                table: "varios",
                type: "TEXT",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Discriminator",
                table: "toy",
                type: "TEXT",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Discriminator",
                table: "tcg",
                type: "TEXT",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Discriminator",
                table: "hot_wheels",
                type: "TEXT",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Discriminator",
                table: "funko",
                type: "TEXT",
                nullable: false,
                defaultValue: "");
        }
    }
}
