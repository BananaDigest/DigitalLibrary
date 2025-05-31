using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DAL.Migrations
{
    /// <inheritdoc />
    public partial class AddBookTypeEntity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AvailableTypes",
                table: "Books");

            migrationBuilder.RenameColumn(
                name: "OrderType",
                table: "Orders",
                newName: "OrderTypeId");

            migrationBuilder.CreateTable(
                name: "BookTypeEntity",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BookTypeEntity", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "BookBookType",
                columns: table => new
                {
                    BookId = table.Column<int>(type: "int", nullable: false),
                    BookTypeId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BookBookType", x => new { x.BookId, x.BookTypeId });
                    table.ForeignKey(
                        name: "FK_BookBookType_BookTypeEntity_BookTypeId",
                        column: x => x.BookTypeId,
                        principalTable: "BookTypeEntity",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_BookBookType_Books_BookId",
                        column: x => x.BookId,
                        principalTable: "Books",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Orders_OrderTypeId",
                table: "Orders",
                column: "OrderTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_BookBookType_BookTypeId",
                table: "BookBookType",
                column: "BookTypeId");

            migrationBuilder.AddForeignKey(
                name: "FK_Orders_BookTypeEntity_OrderTypeId",
                table: "Orders",
                column: "OrderTypeId",
                principalTable: "BookTypeEntity",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Orders_BookTypeEntity_OrderTypeId",
                table: "Orders");

            migrationBuilder.DropTable(
                name: "BookBookType");

            migrationBuilder.DropTable(
                name: "BookTypeEntity");

            migrationBuilder.DropIndex(
                name: "IX_Orders_OrderTypeId",
                table: "Orders");

            migrationBuilder.RenameColumn(
                name: "OrderTypeId",
                table: "Orders",
                newName: "OrderType");

            migrationBuilder.AddColumn<int>(
                name: "AvailableTypes",
                table: "Books",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }
    }
}
