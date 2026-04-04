using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TeejoshSystem.Infrastructure.Adapters.Outbound.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "funko_special_feature",
                columns: table => new
                {
                    id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    name = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_funko_special_feature", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "funko_subtype",
                columns: table => new
                {
                    id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    name = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_funko_subtype", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "hot_wheels_category",
                columns: table => new
                {
                    id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    name = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_hot_wheels_category", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "product",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    type = table.Column<string>(type: "TEXT", nullable: false),
                    name = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    price = table.Column<decimal>(type: "decimal(10,2)", nullable: false),
                    units = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_product", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "tcg_expansion",
                columns: table => new
                {
                    id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    name = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    FranquiciaId = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tcg_expansion", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "tcg_franchise",
                columns: table => new
                {
                    id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    name = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tcg_franchise", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "tcg_pack",
                columns: table => new
                {
                    id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    name = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    FranquiciaId = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tcg_pack", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "funko",
                columns: table => new
                {
                    product_id = table.Column<int>(type: "INTEGER", nullable: false),
                    box_number = table.Column<int>(type: "INTEGER", nullable: false),
                    license = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    subtype = table.Column<int>(type: "INTEGER", nullable: false),
                    special_caracteristic = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_funko", x => x.product_id);
                    table.ForeignKey(
                        name: "FK_funko_product_product_id",
                        column: x => x.product_id,
                        principalTable: "product",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "hot_wheels",
                columns: table => new
                {
                    product_id = table.Column<int>(type: "INTEGER", nullable: false),
                    model = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    year = table.Column<int>(type: "INTEGER", nullable: false),
                    serie = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    category_id = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_hot_wheels", x => x.product_id);
                    table.ForeignKey(
                        name: "FK_hot_wheels_product_product_id",
                        column: x => x.product_id,
                        principalTable: "product",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "tcg",
                columns: table => new
                {
                    product_id = table.Column<int>(type: "INTEGER", nullable: false),
                    pack_id = table.Column<int>(type: "INTEGER", nullable: false),
                    expansion_id = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tcg", x => x.product_id);
                    table.ForeignKey(
                        name: "FK_tcg_product_product_id",
                        column: x => x.product_id,
                        principalTable: "product",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "toy",
                columns: table => new
                {
                    product_id = table.Column<int>(type: "INTEGER", nullable: false),
                    min_years_old = table.Column<int>(type: "INTEGER", nullable: false),
                    min_players = table.Column<int>(type: "INTEGER", nullable: false),
                    max_players = table.Column<int>(type: "INTEGER", nullable: false),
                    is_board_game = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_toy", x => x.product_id);
                    table.ForeignKey(
                        name: "FK_toy_product_product_id",
                        column: x => x.product_id,
                        principalTable: "product",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "varios",
                columns: table => new
                {
                    product_id = table.Column<int>(type: "INTEGER", nullable: false),
                    brand = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    height = table.Column<decimal>(type: "TEXT", nullable: false),
                    width = table.Column<decimal>(type: "TEXT", nullable: false),
                    length = table.Column<decimal>(type: "TEXT", nullable: false),
                    material = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    ilustration = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_varios", x => x.product_id);
                    table.ForeignKey(
                        name: "FK_varios_product_product_id",
                        column: x => x.product_id,
                        principalTable: "product",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_funko_special_feature_name",
                table: "funko_special_feature",
                column: "name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_funko_subtype_name",
                table: "funko_subtype",
                column: "name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_hot_wheels_category_name",
                table: "hot_wheels_category",
                column: "name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_tcg_expansion_name",
                table: "tcg_expansion",
                column: "name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_tcg_franchise_name",
                table: "tcg_franchise",
                column: "name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_tcg_pack_name",
                table: "tcg_pack",
                column: "name",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "funko");

            migrationBuilder.DropTable(
                name: "funko_special_feature");

            migrationBuilder.DropTable(
                name: "funko_subtype");

            migrationBuilder.DropTable(
                name: "hot_wheels");

            migrationBuilder.DropTable(
                name: "hot_wheels_category");

            migrationBuilder.DropTable(
                name: "tcg");

            migrationBuilder.DropTable(
                name: "tcg_expansion");

            migrationBuilder.DropTable(
                name: "tcg_franchise");

            migrationBuilder.DropTable(
                name: "tcg_pack");

            migrationBuilder.DropTable(
                name: "toy");

            migrationBuilder.DropTable(
                name: "varios");

            migrationBuilder.DropTable(
                name: "product");
        }
    }
}
