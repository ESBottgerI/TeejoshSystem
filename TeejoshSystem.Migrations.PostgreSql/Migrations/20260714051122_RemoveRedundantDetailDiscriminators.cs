using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TeejoshSystem.Migrations.PostgreSql.Migrations
{
    /// <inheritdoc />
    public partial class RemoveRedundantDetailDiscriminators : Migration
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
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Discriminator",
                table: "varios",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Discriminator",
                table: "toy",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Discriminator",
                table: "tcg",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Discriminator",
                table: "hot_wheels",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Discriminator",
                table: "funko",
                type: "text",
                nullable: false,
                defaultValue: "");
        }
    }
}
