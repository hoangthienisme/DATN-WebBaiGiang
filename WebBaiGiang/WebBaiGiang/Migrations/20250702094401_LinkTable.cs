using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WebBaiGiang.Migrations
{
    /// <inheritdoc />
    public partial class LinkTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "HocPhanId",
                table: "BaiGiang",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_BaiGiang_HocPhanId",
                table: "BaiGiang",
                column: "HocPhanId");

            migrationBuilder.AddForeignKey(
                name: "FK_BaiGiang_HocPhan_HocPhanId",
                table: "BaiGiang",
                column: "HocPhanId",
                principalTable: "HocPhan",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_BaiGiang_HocPhan_HocPhanId",
                table: "BaiGiang");

            migrationBuilder.DropIndex(
                name: "IX_BaiGiang_HocPhanId",
                table: "BaiGiang");

            migrationBuilder.DropColumn(
                name: "HocPhanId",
                table: "BaiGiang");
        }
    }
}
