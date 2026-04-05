using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TeejoshSystem.Infrastructure.Adapters.Outbound.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddConstraintsAndIndexes : Migration
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

            migrationBuilder.AddCheckConstraint(
                name: "check_dimensions",
                table: "varios",
                sql: "height > 0 AND width > 0 AND (length IS NULL OR length > 0)");

            migrationBuilder.AddCheckConstraint(
                name: "check_players",
                table: "toy",
                sql: "max_players >= min_players");

            migrationBuilder.CreateIndex(
                name: "IX_tcg_pack_name_FranquiciaId",
                table: "tcg_pack",
                columns: new[] { "name", "FranquiciaId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_tcg_expansion_name_FranquiciaId",
                table: "tcg_expansion",
                columns: new[] { "name", "FranquiciaId" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropCheckConstraint(
                name: "check_dimensions",
                table: "varios");

            migrationBuilder.DropCheckConstraint(
                name: "check_players",
                table: "toy");

            migrationBuilder.DropIndex(
                name: "IX_tcg_pack_name_FranquiciaId",
                table: "tcg_pack");

            migrationBuilder.DropIndex(
                name: "IX_tcg_expansion_name_FranquiciaId",
                table: "tcg_expansion");

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
