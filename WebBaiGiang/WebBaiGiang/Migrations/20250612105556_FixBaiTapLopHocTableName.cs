using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WebBaiGiang.Migrations
{
    /// <inheritdoc />
    public partial class FixBaiTapLopHocTableName : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_BaiTapLopHoc_BaiTap_BaiTapId",
                table: "BaiTapLopHoc");

            migrationBuilder.DropForeignKey(
                name: "FK_BaiTapLopHoc_LopHoc_LopHocId",
                table: "BaiTapLopHoc");

            migrationBuilder.DropPrimaryKey(
                name: "PK_BaiTapLopHoc",
                table: "BaiTapLopHoc");

            migrationBuilder.RenameTable(
                name: "BaiTapLopHoc",
                newName: "BaiTapLopHocs");

            migrationBuilder.RenameIndex(
                name: "IX_BaiTapLopHoc_LopHocId",
                table: "BaiTapLopHocs",
                newName: "IX_BaiTapLopHocs_LopHocId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_BaiTapLopHocs",
                table: "BaiTapLopHocs",
                columns: new[] { "BaiTapId", "LopHocId" });

            migrationBuilder.AddForeignKey(
                name: "FK_BaiTapLopHocs_BaiTap_BaiTapId",
                table: "BaiTapLopHocs",
                column: "BaiTapId",
                principalTable: "BaiTap",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_BaiTapLopHocs_LopHoc_LopHocId",
                table: "BaiTapLopHocs",
                column: "LopHocId",
                principalTable: "LopHoc",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_BaiTapLopHocs_BaiTap_BaiTapId",
                table: "BaiTapLopHocs");

            migrationBuilder.DropForeignKey(
                name: "FK_BaiTapLopHocs_LopHoc_LopHocId",
                table: "BaiTapLopHocs");

            migrationBuilder.DropPrimaryKey(
                name: "PK_BaiTapLopHocs",
                table: "BaiTapLopHocs");

            migrationBuilder.RenameTable(
                name: "BaiTapLopHocs",
                newName: "BaiTapLopHoc");

            migrationBuilder.RenameIndex(
                name: "IX_BaiTapLopHocs_LopHocId",
                table: "BaiTapLopHoc",
                newName: "IX_BaiTapLopHoc_LopHocId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_BaiTapLopHoc",
                table: "BaiTapLopHoc",
                columns: new[] { "BaiTapId", "LopHocId" });

            migrationBuilder.AddForeignKey(
                name: "FK_BaiTapLopHoc_BaiTap_BaiTapId",
                table: "BaiTapLopHoc",
                column: "BaiTapId",
                principalTable: "BaiTap",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_BaiTapLopHoc_LopHoc_LopHocId",
                table: "BaiTapLopHoc",
                column: "LopHocId",
                principalTable: "LopHoc",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
