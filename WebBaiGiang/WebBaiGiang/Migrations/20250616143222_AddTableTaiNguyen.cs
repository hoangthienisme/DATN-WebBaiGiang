using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WebBaiGiang.Migrations
{
    /// <inheritdoc />
    public partial class AddTableTaiNguyen : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "TaiNguyen",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    BaiGiangId = table.Column<int>(type: "int", nullable: false),
                    Url = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    Loai = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TaiNguyen", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TaiNguyen_BaiGiang_BaiGiangId",
                        column: x => x.BaiGiangId,
                        principalTable: "BaiGiang",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_TaiNguyen_BaiGiangId",
                table: "TaiNguyen",
                column: "BaiGiangId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "TaiNguyen");
        }
    }
}
