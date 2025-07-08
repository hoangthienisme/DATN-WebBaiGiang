using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WebBaiGiang.Migrations
{
    /// <inheritdoc />
    public partial class updateLop : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_LopHoc_Khoa",
                table: "LopHoc");

            migrationBuilder.RenameColumn(
                name: "Khoa_id",
                table: "LopHoc",
                newName: "KhoaId");

            migrationBuilder.RenameIndex(
                name: "IX_LopHoc_Khoa_id",
                table: "LopHoc",
                newName: "IX_LopHoc_KhoaId");

            migrationBuilder.AlterColumn<int>(
                name: "KhoaId",
                table: "LopHoc",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddForeignKey(
                name: "FK_LopHoc_Khoa_KhoaId",
                table: "LopHoc",
                column: "KhoaId",
                principalTable: "Khoa",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_LopHoc_Khoa_KhoaId",
                table: "LopHoc");

            migrationBuilder.RenameColumn(
                name: "KhoaId",
                table: "LopHoc",
                newName: "Khoa_id");

            migrationBuilder.RenameIndex(
                name: "IX_LopHoc_KhoaId",
                table: "LopHoc",
                newName: "IX_LopHoc_Khoa_id");

            migrationBuilder.AlterColumn<int>(
                name: "Khoa_id",
                table: "LopHoc",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_LopHoc_Khoa",
                table: "LopHoc",
                column: "Khoa_id",
                principalTable: "Khoa",
                principalColumn: "Id");
        }
    }
}