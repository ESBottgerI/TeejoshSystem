using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using TeejoshSystem.Infrastructure.Adapters.Outbound.Persistence;

#nullable disable

namespace TeejoshSystem.Infrastructure.Migrations;

[DbContext(typeof(InventarioDbContext))]
[Migration("20260712000100_AddTipoProductoSnapshotVenta")]
public sealed class AddTipoProductoSnapshotVenta : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<string>(
            name: "product_type",
            table: "sale_detail",
            type: "TEXT",
            maxLength: 20,
            nullable: false,
            defaultValue: "HotWheels");
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropColumn(
            name: "product_type",
            table: "sale_detail");

    }
}
