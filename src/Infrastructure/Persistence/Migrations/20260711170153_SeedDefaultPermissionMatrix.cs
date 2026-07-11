using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class SeedDefaultPermissionMatrix : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "PermissionMatrixEntries",
                columns: new[] { "Id", "Accion", "CreatedAt", "Permitido", "Recurso", "Rol", "UpdatedAt" },
                values: new object[,]
                {
                    { new Guid("00000000-0000-0000-0000-000000000001"), 0, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), true, 0, 0, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { new Guid("00000000-0000-0000-0000-000000000002"), 1, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), true, 0, 0, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { new Guid("00000000-0000-0000-0000-000000000003"), 2, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), true, 0, 0, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { new Guid("00000000-0000-0000-0000-000000000004"), 3, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), true, 0, 0, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { new Guid("00000000-0000-0000-0000-000000000005"), 0, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), true, 1, 0, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { new Guid("00000000-0000-0000-0000-000000000006"), 1, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), true, 1, 0, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { new Guid("00000000-0000-0000-0000-000000000007"), 2, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), true, 1, 0, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { new Guid("00000000-0000-0000-0000-000000000008"), 3, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), true, 1, 0, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { new Guid("00000000-0000-0000-0000-000000000009"), 0, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), true, 2, 0, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { new Guid("00000000-0000-0000-0000-000000000010"), 1, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), true, 2, 0, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { new Guid("00000000-0000-0000-0000-000000000011"), 2, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), true, 2, 0, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { new Guid("00000000-0000-0000-0000-000000000012"), 3, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), true, 2, 0, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { new Guid("00000000-0000-0000-0000-000000000013"), 0, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), true, 3, 0, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { new Guid("00000000-0000-0000-0000-000000000014"), 1, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), true, 3, 0, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { new Guid("00000000-0000-0000-0000-000000000015"), 2, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), true, 3, 0, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { new Guid("00000000-0000-0000-0000-000000000016"), 3, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), true, 3, 0, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { new Guid("00000000-0000-0000-0000-000000000017"), 0, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), false, 0, 1, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { new Guid("00000000-0000-0000-0000-000000000018"), 1, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), true, 0, 1, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { new Guid("00000000-0000-0000-0000-000000000019"), 2, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), false, 0, 1, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { new Guid("00000000-0000-0000-0000-000000000020"), 3, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), false, 0, 1, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { new Guid("00000000-0000-0000-0000-000000000021"), 0, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), false, 1, 1, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { new Guid("00000000-0000-0000-0000-000000000022"), 1, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), true, 1, 1, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { new Guid("00000000-0000-0000-0000-000000000023"), 2, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), false, 1, 1, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { new Guid("00000000-0000-0000-0000-000000000024"), 3, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), false, 1, 1, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { new Guid("00000000-0000-0000-0000-000000000025"), 0, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), false, 2, 1, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { new Guid("00000000-0000-0000-0000-000000000026"), 1, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), true, 2, 1, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { new Guid("00000000-0000-0000-0000-000000000027"), 2, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), false, 2, 1, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { new Guid("00000000-0000-0000-0000-000000000028"), 3, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), false, 2, 1, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { new Guid("00000000-0000-0000-0000-000000000029"), 0, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), true, 3, 1, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { new Guid("00000000-0000-0000-0000-000000000030"), 1, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), true, 3, 1, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { new Guid("00000000-0000-0000-0000-000000000031"), 2, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), true, 3, 1, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { new Guid("00000000-0000-0000-0000-000000000032"), 3, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), true, 3, 1, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { new Guid("00000000-0000-0000-0000-000000000033"), 0, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), false, 0, 2, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { new Guid("00000000-0000-0000-0000-000000000034"), 1, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), false, 0, 2, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { new Guid("00000000-0000-0000-0000-000000000035"), 2, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), false, 0, 2, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { new Guid("00000000-0000-0000-0000-000000000036"), 3, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), false, 0, 2, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { new Guid("00000000-0000-0000-0000-000000000037"), 0, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), false, 1, 2, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { new Guid("00000000-0000-0000-0000-000000000038"), 1, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), false, 1, 2, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { new Guid("00000000-0000-0000-0000-000000000039"), 2, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), false, 1, 2, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { new Guid("00000000-0000-0000-0000-000000000040"), 3, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), false, 1, 2, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { new Guid("00000000-0000-0000-0000-000000000041"), 0, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), false, 2, 2, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { new Guid("00000000-0000-0000-0000-000000000042"), 1, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), false, 2, 2, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { new Guid("00000000-0000-0000-0000-000000000043"), 2, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), false, 2, 2, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { new Guid("00000000-0000-0000-0000-000000000044"), 3, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), false, 2, 2, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { new Guid("00000000-0000-0000-0000-000000000045"), 0, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), false, 3, 2, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { new Guid("00000000-0000-0000-0000-000000000046"), 1, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), true, 3, 2, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { new Guid("00000000-0000-0000-0000-000000000047"), 2, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), false, 3, 2, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { new Guid("00000000-0000-0000-0000-000000000048"), 3, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), false, 3, 2, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "PermissionMatrixEntries",
                keyColumn: "Id",
                keyValue: new Guid("00000000-0000-0000-0000-000000000001"));

            migrationBuilder.DeleteData(
                table: "PermissionMatrixEntries",
                keyColumn: "Id",
                keyValue: new Guid("00000000-0000-0000-0000-000000000002"));

            migrationBuilder.DeleteData(
                table: "PermissionMatrixEntries",
                keyColumn: "Id",
                keyValue: new Guid("00000000-0000-0000-0000-000000000003"));

            migrationBuilder.DeleteData(
                table: "PermissionMatrixEntries",
                keyColumn: "Id",
                keyValue: new Guid("00000000-0000-0000-0000-000000000004"));

            migrationBuilder.DeleteData(
                table: "PermissionMatrixEntries",
                keyColumn: "Id",
                keyValue: new Guid("00000000-0000-0000-0000-000000000005"));

            migrationBuilder.DeleteData(
                table: "PermissionMatrixEntries",
                keyColumn: "Id",
                keyValue: new Guid("00000000-0000-0000-0000-000000000006"));

            migrationBuilder.DeleteData(
                table: "PermissionMatrixEntries",
                keyColumn: "Id",
                keyValue: new Guid("00000000-0000-0000-0000-000000000007"));

            migrationBuilder.DeleteData(
                table: "PermissionMatrixEntries",
                keyColumn: "Id",
                keyValue: new Guid("00000000-0000-0000-0000-000000000008"));

            migrationBuilder.DeleteData(
                table: "PermissionMatrixEntries",
                keyColumn: "Id",
                keyValue: new Guid("00000000-0000-0000-0000-000000000009"));

            migrationBuilder.DeleteData(
                table: "PermissionMatrixEntries",
                keyColumn: "Id",
                keyValue: new Guid("00000000-0000-0000-0000-000000000010"));

            migrationBuilder.DeleteData(
                table: "PermissionMatrixEntries",
                keyColumn: "Id",
                keyValue: new Guid("00000000-0000-0000-0000-000000000011"));

            migrationBuilder.DeleteData(
                table: "PermissionMatrixEntries",
                keyColumn: "Id",
                keyValue: new Guid("00000000-0000-0000-0000-000000000012"));

            migrationBuilder.DeleteData(
                table: "PermissionMatrixEntries",
                keyColumn: "Id",
                keyValue: new Guid("00000000-0000-0000-0000-000000000013"));

            migrationBuilder.DeleteData(
                table: "PermissionMatrixEntries",
                keyColumn: "Id",
                keyValue: new Guid("00000000-0000-0000-0000-000000000014"));

            migrationBuilder.DeleteData(
                table: "PermissionMatrixEntries",
                keyColumn: "Id",
                keyValue: new Guid("00000000-0000-0000-0000-000000000015"));

            migrationBuilder.DeleteData(
                table: "PermissionMatrixEntries",
                keyColumn: "Id",
                keyValue: new Guid("00000000-0000-0000-0000-000000000016"));

            migrationBuilder.DeleteData(
                table: "PermissionMatrixEntries",
                keyColumn: "Id",
                keyValue: new Guid("00000000-0000-0000-0000-000000000017"));

            migrationBuilder.DeleteData(
                table: "PermissionMatrixEntries",
                keyColumn: "Id",
                keyValue: new Guid("00000000-0000-0000-0000-000000000018"));

            migrationBuilder.DeleteData(
                table: "PermissionMatrixEntries",
                keyColumn: "Id",
                keyValue: new Guid("00000000-0000-0000-0000-000000000019"));

            migrationBuilder.DeleteData(
                table: "PermissionMatrixEntries",
                keyColumn: "Id",
                keyValue: new Guid("00000000-0000-0000-0000-000000000020"));

            migrationBuilder.DeleteData(
                table: "PermissionMatrixEntries",
                keyColumn: "Id",
                keyValue: new Guid("00000000-0000-0000-0000-000000000021"));

            migrationBuilder.DeleteData(
                table: "PermissionMatrixEntries",
                keyColumn: "Id",
                keyValue: new Guid("00000000-0000-0000-0000-000000000022"));

            migrationBuilder.DeleteData(
                table: "PermissionMatrixEntries",
                keyColumn: "Id",
                keyValue: new Guid("00000000-0000-0000-0000-000000000023"));

            migrationBuilder.DeleteData(
                table: "PermissionMatrixEntries",
                keyColumn: "Id",
                keyValue: new Guid("00000000-0000-0000-0000-000000000024"));

            migrationBuilder.DeleteData(
                table: "PermissionMatrixEntries",
                keyColumn: "Id",
                keyValue: new Guid("00000000-0000-0000-0000-000000000025"));

            migrationBuilder.DeleteData(
                table: "PermissionMatrixEntries",
                keyColumn: "Id",
                keyValue: new Guid("00000000-0000-0000-0000-000000000026"));

            migrationBuilder.DeleteData(
                table: "PermissionMatrixEntries",
                keyColumn: "Id",
                keyValue: new Guid("00000000-0000-0000-0000-000000000027"));

            migrationBuilder.DeleteData(
                table: "PermissionMatrixEntries",
                keyColumn: "Id",
                keyValue: new Guid("00000000-0000-0000-0000-000000000028"));

            migrationBuilder.DeleteData(
                table: "PermissionMatrixEntries",
                keyColumn: "Id",
                keyValue: new Guid("00000000-0000-0000-0000-000000000029"));

            migrationBuilder.DeleteData(
                table: "PermissionMatrixEntries",
                keyColumn: "Id",
                keyValue: new Guid("00000000-0000-0000-0000-000000000030"));

            migrationBuilder.DeleteData(
                table: "PermissionMatrixEntries",
                keyColumn: "Id",
                keyValue: new Guid("00000000-0000-0000-0000-000000000031"));

            migrationBuilder.DeleteData(
                table: "PermissionMatrixEntries",
                keyColumn: "Id",
                keyValue: new Guid("00000000-0000-0000-0000-000000000032"));

            migrationBuilder.DeleteData(
                table: "PermissionMatrixEntries",
                keyColumn: "Id",
                keyValue: new Guid("00000000-0000-0000-0000-000000000033"));

            migrationBuilder.DeleteData(
                table: "PermissionMatrixEntries",
                keyColumn: "Id",
                keyValue: new Guid("00000000-0000-0000-0000-000000000034"));

            migrationBuilder.DeleteData(
                table: "PermissionMatrixEntries",
                keyColumn: "Id",
                keyValue: new Guid("00000000-0000-0000-0000-000000000035"));

            migrationBuilder.DeleteData(
                table: "PermissionMatrixEntries",
                keyColumn: "Id",
                keyValue: new Guid("00000000-0000-0000-0000-000000000036"));

            migrationBuilder.DeleteData(
                table: "PermissionMatrixEntries",
                keyColumn: "Id",
                keyValue: new Guid("00000000-0000-0000-0000-000000000037"));

            migrationBuilder.DeleteData(
                table: "PermissionMatrixEntries",
                keyColumn: "Id",
                keyValue: new Guid("00000000-0000-0000-0000-000000000038"));

            migrationBuilder.DeleteData(
                table: "PermissionMatrixEntries",
                keyColumn: "Id",
                keyValue: new Guid("00000000-0000-0000-0000-000000000039"));

            migrationBuilder.DeleteData(
                table: "PermissionMatrixEntries",
                keyColumn: "Id",
                keyValue: new Guid("00000000-0000-0000-0000-000000000040"));

            migrationBuilder.DeleteData(
                table: "PermissionMatrixEntries",
                keyColumn: "Id",
                keyValue: new Guid("00000000-0000-0000-0000-000000000041"));

            migrationBuilder.DeleteData(
                table: "PermissionMatrixEntries",
                keyColumn: "Id",
                keyValue: new Guid("00000000-0000-0000-0000-000000000042"));

            migrationBuilder.DeleteData(
                table: "PermissionMatrixEntries",
                keyColumn: "Id",
                keyValue: new Guid("00000000-0000-0000-0000-000000000043"));

            migrationBuilder.DeleteData(
                table: "PermissionMatrixEntries",
                keyColumn: "Id",
                keyValue: new Guid("00000000-0000-0000-0000-000000000044"));

            migrationBuilder.DeleteData(
                table: "PermissionMatrixEntries",
                keyColumn: "Id",
                keyValue: new Guid("00000000-0000-0000-0000-000000000045"));

            migrationBuilder.DeleteData(
                table: "PermissionMatrixEntries",
                keyColumn: "Id",
                keyValue: new Guid("00000000-0000-0000-0000-000000000046"));

            migrationBuilder.DeleteData(
                table: "PermissionMatrixEntries",
                keyColumn: "Id",
                keyValue: new Guid("00000000-0000-0000-0000-000000000047"));

            migrationBuilder.DeleteData(
                table: "PermissionMatrixEntries",
                keyColumn: "Id",
                keyValue: new Guid("00000000-0000-0000-0000-000000000048"));
        }
    }
}
