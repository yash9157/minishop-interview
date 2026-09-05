using Microsoft.EntityFrameworkCore.Migrations;
using MySql.EntityFrameworkCore.Metadata;

#nullable disable

namespace MiniShop.EntityFrameworkCore.Migrations
{
    /// <inheritdoc />
    public partial class AddMultiTenancy : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_Products_CategoryId",
                table: "Products",
                column: "CategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_Orders_CustomerId",
                table: "Orders",
                column: "CustomerId");

            migrationBuilder.CreateIndex(
                name: "IX_OrderItems_OrderId",
                table: "OrderItems",
                column: "OrderId");

            migrationBuilder.DropIndex(
                name: "IX_Products_CategoryId_Name",
                table: "Products");

            migrationBuilder.DropIndex(
                name: "IX_Products_Sku",
                table: "Products");

            migrationBuilder.DropIndex(
                name: "IX_Orders_CustomerId_OrderDateUtc",
                table: "Orders");

            migrationBuilder.DropIndex(
                name: "IX_Orders_OrderNumber",
                table: "Orders");

            migrationBuilder.DropIndex(
                name: "IX_Orders_Status_OrderDateUtc",
                table: "Orders");

            migrationBuilder.DropIndex(
                name: "IX_OrderItems_OrderId_ProductId",
                table: "OrderItems");

            migrationBuilder.DropIndex(
                name: "IX_Customers_Email",
                table: "Customers");

            migrationBuilder.DropIndex(
                name: "IX_Categories_Name",
                table: "Categories");

            migrationBuilder.AddColumn<long>(
                name: "TenantId",
                table: "Products",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "TenantId",
                table: "Orders",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "TenantId",
                table: "OrderItems",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "TenantId",
                table: "Customers",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "TenantId",
                table: "Categories",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "TenantId",
                table: "AspNetUsers",
                type: "bigint",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "Tenants",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn),
                    Code = table.Column<string>(type: "varchar(120)", maxLength: 120, nullable: false),
                    Name = table.Column<string>(type: "varchar(120)", maxLength: 120, nullable: false),
                    IsActive = table.Column<bool>(type: "tinyint(1)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Tenants", x => x.Id);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.InsertData(
                table: "Tenants",
                columns: new[] { "Id", "Code", "Name", "IsActive" },
                values: new object[,]
                {
                    { 1L, "minishop", "MiniShop", true },
                    { 2L, "novamart", "NovaMart", true }
                });

            migrationBuilder.Sql("UPDATE `Products` SET `TenantId` = 1 WHERE `TenantId` IS NULL;");
            migrationBuilder.Sql("UPDATE `Orders` SET `TenantId` = 1 WHERE `TenantId` IS NULL;");
            migrationBuilder.Sql("UPDATE `OrderItems` SET `TenantId` = 1 WHERE `TenantId` IS NULL;");
            migrationBuilder.Sql("UPDATE `Customers` SET `TenantId` = 1 WHERE `TenantId` IS NULL;");
            migrationBuilder.Sql("UPDATE `Categories` SET `TenantId` = 1 WHERE `TenantId` IS NULL;");
            migrationBuilder.Sql("UPDATE `AspNetUsers` SET `TenantId` = 1 WHERE `TenantId` IS NULL;");

            migrationBuilder.AlterColumn<long>(
                name: "TenantId",
                table: "Products",
                type: "bigint",
                nullable: false,
                oldClrType: typeof(long),
                oldType: "bigint",
                oldNullable: true);

            migrationBuilder.AlterColumn<long>(
                name: "TenantId",
                table: "Orders",
                type: "bigint",
                nullable: false,
                oldClrType: typeof(long),
                oldType: "bigint",
                oldNullable: true);

            migrationBuilder.AlterColumn<long>(
                name: "TenantId",
                table: "OrderItems",
                type: "bigint",
                nullable: false,
                oldClrType: typeof(long),
                oldType: "bigint",
                oldNullable: true);

            migrationBuilder.AlterColumn<long>(
                name: "TenantId",
                table: "Customers",
                type: "bigint",
                nullable: false,
                oldClrType: typeof(long),
                oldType: "bigint",
                oldNullable: true);

            migrationBuilder.AlterColumn<long>(
                name: "TenantId",
                table: "Categories",
                type: "bigint",
                nullable: false,
                oldClrType: typeof(long),
                oldType: "bigint",
                oldNullable: true);

            migrationBuilder.AlterColumn<long>(
                name: "TenantId",
                table: "AspNetUsers",
                type: "bigint",
                nullable: false,
                oldClrType: typeof(long),
                oldType: "bigint",
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Products_TenantId_CategoryId_Name",
                table: "Products",
                columns: new[] { "TenantId", "CategoryId", "Name" });

            migrationBuilder.CreateIndex(
                name: "IX_Products_TenantId_Sku",
                table: "Products",
                columns: new[] { "TenantId", "Sku" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Orders_TenantId_CustomerId_OrderDateUtc",
                table: "Orders",
                columns: new[] { "TenantId", "CustomerId", "OrderDateUtc" });

            migrationBuilder.CreateIndex(
                name: "IX_Orders_TenantId_OrderNumber",
                table: "Orders",
                columns: new[] { "TenantId", "OrderNumber" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Orders_TenantId_Status_OrderDateUtc",
                table: "Orders",
                columns: new[] { "TenantId", "Status", "OrderDateUtc" });

            migrationBuilder.CreateIndex(
                name: "IX_OrderItems_TenantId_OrderId_ProductId",
                table: "OrderItems",
                columns: new[] { "TenantId", "OrderId", "ProductId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_OrderItems_TenantId_ProductId",
                table: "OrderItems",
                columns: new[] { "TenantId", "ProductId" });

            migrationBuilder.CreateIndex(
                name: "IX_Customers_TenantId_Email",
                table: "Customers",
                columns: new[] { "TenantId", "Email" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Categories_TenantId_Name",
                table: "Categories",
                columns: new[] { "TenantId", "Name" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUsers_TenantId",
                table: "AspNetUsers",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_Tenants_Code",
                table: "Tenants",
                column: "Code",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_AspNetUsers_Tenants_TenantId",
                table: "AspNetUsers",
                column: "TenantId",
                principalTable: "Tenants",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Categories_Tenants_TenantId",
                table: "Categories",
                column: "TenantId",
                principalTable: "Tenants",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Customers_Tenants_TenantId",
                table: "Customers",
                column: "TenantId",
                principalTable: "Tenants",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_OrderItems_Tenants_TenantId",
                table: "OrderItems",
                column: "TenantId",
                principalTable: "Tenants",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Orders_Tenants_TenantId",
                table: "Orders",
                column: "TenantId",
                principalTable: "Tenants",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Products_Tenants_TenantId",
                table: "Products",
                column: "TenantId",
                principalTable: "Tenants",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AspNetUsers_Tenants_TenantId",
                table: "AspNetUsers");

            migrationBuilder.DropForeignKey(
                name: "FK_Categories_Tenants_TenantId",
                table: "Categories");

            migrationBuilder.DropForeignKey(
                name: "FK_Customers_Tenants_TenantId",
                table: "Customers");

            migrationBuilder.DropForeignKey(
                name: "FK_OrderItems_Tenants_TenantId",
                table: "OrderItems");

            migrationBuilder.DropForeignKey(
                name: "FK_Orders_Tenants_TenantId",
                table: "Orders");

            migrationBuilder.DropForeignKey(
                name: "FK_Products_Tenants_TenantId",
                table: "Products");

            migrationBuilder.DropTable(
                name: "Tenants");

            migrationBuilder.DropIndex(
                name: "IX_Products_CategoryId",
                table: "Products");

            migrationBuilder.DropIndex(
                name: "IX_Products_TenantId_CategoryId_Name",
                table: "Products");

            migrationBuilder.DropIndex(
                name: "IX_Products_TenantId_Sku",
                table: "Products");

            migrationBuilder.DropIndex(
                name: "IX_Orders_CustomerId",
                table: "Orders");

            migrationBuilder.DropIndex(
                name: "IX_Orders_TenantId_CustomerId_OrderDateUtc",
                table: "Orders");

            migrationBuilder.DropIndex(
                name: "IX_Orders_TenantId_OrderNumber",
                table: "Orders");

            migrationBuilder.DropIndex(
                name: "IX_Orders_TenantId_Status_OrderDateUtc",
                table: "Orders");

            migrationBuilder.DropIndex(
                name: "IX_OrderItems_OrderId",
                table: "OrderItems");

            migrationBuilder.DropIndex(
                name: "IX_OrderItems_TenantId_OrderId_ProductId",
                table: "OrderItems");

            migrationBuilder.DropIndex(
                name: "IX_OrderItems_TenantId_ProductId",
                table: "OrderItems");

            migrationBuilder.DropIndex(
                name: "IX_Customers_TenantId_Email",
                table: "Customers");

            migrationBuilder.DropIndex(
                name: "IX_Categories_TenantId_Name",
                table: "Categories");

            migrationBuilder.DropIndex(
                name: "IX_AspNetUsers_TenantId",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "TenantId",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "TenantId",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "TenantId",
                table: "OrderItems");

            migrationBuilder.DropColumn(
                name: "TenantId",
                table: "Customers");

            migrationBuilder.DropColumn(
                name: "TenantId",
                table: "Categories");

            migrationBuilder.DropColumn(
                name: "TenantId",
                table: "AspNetUsers");

            migrationBuilder.CreateIndex(
                name: "IX_Products_CategoryId_Name",
                table: "Products",
                columns: new[] { "CategoryId", "Name" });

            migrationBuilder.CreateIndex(
                name: "IX_Products_Sku",
                table: "Products",
                column: "Sku",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Orders_CustomerId_OrderDateUtc",
                table: "Orders",
                columns: new[] { "CustomerId", "OrderDateUtc" });

            migrationBuilder.CreateIndex(
                name: "IX_Orders_OrderNumber",
                table: "Orders",
                column: "OrderNumber",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Orders_Status_OrderDateUtc",
                table: "Orders",
                columns: new[] { "Status", "OrderDateUtc" });

            migrationBuilder.CreateIndex(
                name: "IX_OrderItems_OrderId_ProductId",
                table: "OrderItems",
                columns: new[] { "OrderId", "ProductId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Customers_Email",
                table: "Customers",
                column: "Email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Categories_Name",
                table: "Categories",
                column: "Name",
                unique: true);
        }
    }
}
