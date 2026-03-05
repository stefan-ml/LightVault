using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LightVault.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddSecretRotationMetadata : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "ExpiresAt",
                table: "SecretVersions",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsRevoked",
                table: "SecretVersions",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ExpiresAt",
                table: "SecretVersions");

            migrationBuilder.DropColumn(
                name: "IsRevoked",
                table: "SecretVersions");
        }
    }
}
