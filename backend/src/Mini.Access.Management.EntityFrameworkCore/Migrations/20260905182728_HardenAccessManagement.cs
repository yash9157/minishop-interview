using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Mini.Access.Management.EntityFrameworkCore.Migrations
{
    /// <inheritdoc />
    public partial class HardenAccessManagement : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "RequestHash",
                table: "IdempotencyRecords",
                type: "varchar(64)",
                maxLength: 64,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "ResponseJson",
                table: "IdempotencyRecords",
                type: "longtext",
                nullable: false);

            migrationBuilder.AddColumn<int>(
                name: "StatusCode",
                table: "IdempotencyRecords",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<long>(
                name: "Version",
                table: "AccessRequests",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "RequestHash",
                table: "IdempotencyRecords");

            migrationBuilder.DropColumn(
                name: "ResponseJson",
                table: "IdempotencyRecords");

            migrationBuilder.DropColumn(
                name: "StatusCode",
                table: "IdempotencyRecords");

            migrationBuilder.DropColumn(
                name: "Version",
                table: "AccessRequests");
        }
    }
}
