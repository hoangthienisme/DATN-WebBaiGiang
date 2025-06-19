using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WebBaiGiang.Migrations
{
    /// <inheritdoc />
    public partial class AddTableBaiGiangLopHoc : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
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

            migrationBuilder.CreateTable(
                name: "LopHocBaiGiangs",
                columns: table => new
                {
                    LopHocId = table.Column<int>(type: "int", nullable: false),
                    BaiGiangId = table.Column<int>(type: "int", nullable: false),
                    AddedDate = table.Column<DateTime>(type: "datetime", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LopHocBaiGiangs", x => new { x.LopHocId, x.BaiGiangId });
                    table.ForeignKey(
                        name: "FK_LopHocBaiGiangs_BaiGiang_BaiGiangId",
                        column: x => x.BaiGiangId,
                        principalTable: "BaiGiang",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_LopHocBaiGiangs_LopHoc_LopHocId",
                        column: x => x.LopHocId,
                        principalTable: "LopHoc",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_LopHocBaiGiangs_BaiGiangId",
                table: "LopHocBaiGiangs",
                column: "BaiGiangId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "LopHocBaiGiangs");

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
    }
}
