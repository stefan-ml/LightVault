using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LightVault.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddAuditHashChain : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DELETE FROM AuditEntries");
            migrationBuilder.RenameColumn(
                name: "Timestamp",
                table: "AuditEntries",
                newName: "TimestampUtc");

            migrationBuilder.AddColumn<string>(
                name: "Hash",
                table: "AuditEntries",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "PrevHash",
                table: "AuditEntries",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Hash",
                table: "AuditEntries");

            migrationBuilder.DropColumn(
                name: "PrevHash",
                table: "AuditEntries");

            migrationBuilder.RenameColumn(
                name: "TimestampUtc",
                table: "AuditEntries",
                newName: "Timestamp");
        }
    }
}
