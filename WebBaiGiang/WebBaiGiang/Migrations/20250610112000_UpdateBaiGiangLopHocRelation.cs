using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WebBaiGiang.Migrations
{
    /// <inheritdoc />
    public partial class UpdateBaiGiangLopHocRelation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK__BaiGiang__Class___4F7CD00D",
                table: "BaiGiang");

            migrationBuilder.DropIndex(
                name: "IX_BaiGiang_Class_id",
                table: "BaiGiang");

            migrationBuilder.DropColumn(
                name: "Class_id",
                table: "BaiGiang");

            migrationBuilder.AddColumn<int>(
                name: "BaiGiang_id",
                table: "LopHoc",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_LopHoc_BaiGiang_id",
                table: "LopHoc",
                column: "BaiGiang_id");

            migrationBuilder.AddForeignKey(
                name: "FK_LopHoc_BaiGiang",
                table: "LopHoc",
                column: "BaiGiang_id",
                principalTable: "BaiGiang",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_LopHoc_BaiGiang",
                table: "LopHoc");

            migrationBuilder.DropIndex(
                name: "IX_LopHoc_BaiGiang_id",
                table: "LopHoc");

            migrationBuilder.DropColumn(
                name: "BaiGiang_id",
                table: "LopHoc");

            migrationBuilder.AddColumn<int>(
                name: "Class_id",
                table: "BaiGiang",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_BaiGiang_Class_id",
                table: "BaiGiang",
                column: "Class_id");

            migrationBuilder.AddForeignKey(
                name: "FK__BaiGiang__Class___4F7CD00D",
                table: "BaiGiang",
                column: "Class_id",
                principalTable: "LopHoc",
                principalColumn: "Id");
        }
    }
}
