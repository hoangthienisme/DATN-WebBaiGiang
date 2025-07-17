using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WebBaiGiang.Migrations
{
    /// <inheritdoc />
    public partial class fixbug : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Thêm cột OriginalId
            migrationBuilder.AddColumn<int>(
                name: "OriginalId",
                table: "BaiGiang",
                type: "int",
                nullable: true);

            // Tạo index
            migrationBuilder.CreateIndex(
                name: "IX_BaiGiang_OriginalId",
                table: "BaiGiang",
                column: "OriginalId");

            // Tạo foreign key
            migrationBuilder.AddForeignKey(
                name: "FK_BaiGiang_BaiGiang_OriginalId",
                table: "BaiGiang",
                column: "OriginalId",
                principalTable: "BaiGiang",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Xoá foreign key
            migrationBuilder.DropForeignKey(
                name: "FK_BaiGiang_BaiGiang_OriginalId",
                table: "BaiGiang");

            // Xoá index
            migrationBuilder.DropIndex(
                name: "IX_BaiGiang_OriginalId",
                table: "BaiGiang");

            // Xoá cột
            migrationBuilder.DropColumn(
                name: "OriginalId",
                table: "BaiGiang");
        }
    }
}
