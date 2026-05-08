using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TeejoshSystem.Infrastructure.Adapters.Outbound.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddRolToUsuario : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Discriminator",
                table: "varios");

            migrationBuilder.DropColumn(
                name: "Discriminator",
                table: "toy");

            migrationBuilder.DropColumn(
                name: "Discriminator",
                table: "tcg");

            migrationBuilder.DropColumn(
                name: "Discriminator",
                table: "hot_wheels");

            migrationBuilder.DropColumn(
                name: "Discriminator",
                table: "funko");

            migrationBuilder.AddColumn<string>(
                name: "rol",
                table: "app_user",
                type: "TEXT",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "rol",
                table: "app_user");

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
